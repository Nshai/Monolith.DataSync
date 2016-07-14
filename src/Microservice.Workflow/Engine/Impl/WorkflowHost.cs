﻿using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activities;
using System.ServiceModel.Channels;
using System.Threading;
using System.Xaml;
using System.Xml.Linq;
using log4net;
using Microservice.Workflow.Domain;
using Microservice.Workflow.Properties;
using Microservice.Workflow.v1;

namespace Microservice.Workflow.Engine.Impl
{
    public class WorkflowHost : IWorkflowHost
    {
        private readonly IWorkflowClientFactory workflowClientFactory;
        private readonly IDictionary<Guid, WorkflowServiceHost> services = new Dictionary<Guid, WorkflowServiceHost>();
        private readonly IDictionary<Guid, Counter> templateInstanceCount = new Dictionary<Guid, Counter>();
        private readonly GenerationList<Guid> templatesToBePurged = new GenerationList<Guid>();
        private volatile object lockObj = new object();
        private readonly Binding binding;
        private readonly ILog logger = LogManager.GetLogger(typeof(WorkflowHost));
        private readonly Timer timer;
        private bool shuttingDown;
        private int lastServiceCount;
        private int lastInstanceCount;

        public WorkflowHost(IWorkflowClientFactory workflowClientFactory)
        {
            logger.Info("Action=WorkflowHostStarting");

            this.workflowClientFactory = workflowClientFactory;
            binding = new BasicHttpBinding(BasicHttpSecurityMode.None);

            var interval = Convert.ToInt32(ConfigurationManager.AppSettings["templatePurgeIntervalSeconds"]);
            timer = new Timer(Purge, null, TimeSpan.FromSeconds(interval), TimeSpan.FromSeconds(interval));

            logger.Info("Action=WorkflowHostStarted");
        }

        private void Purge(object state)
        {
            try
            {
                if (shuttingDown) return;

                if (!Monitor.TryEnter(lockObj)) return;
                try
                {
                    if (shuttingDown) return;

                    templatesToBePurged.Promote(templateInstanceCount.Where(k => k.Value.Value == 0).Select(k => k.Key));

                    var templatesToPurge = templatesToBePurged.GetGeneration(templatesToBePurged.MaxGenerationIndex);
                    if (templatesToPurge.Any())
                    {
                        foreach (var templateId in templatesToPurge.Where(templateId => services.ContainsKey(templateId)))
                        {
                            CloseService(templateId);
                        }
                    }
                    LogResourceUsage();
                }
                finally
                {
                    Monitor.Exit(lockObj);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Purge failed", ex);
            }
        }

        private void CloseService(Guid templateId)
        {
            lock (lockObj)
            {
                if (services.ContainsKey(templateId))
                {
                    var service = services[templateId];

                    service.TryClose();
                    services.Remove(templateId);

                    logger.Info($"Action=TemplateUnloaded TemplateId={templateId}");
                }

                templateInstanceCount.Remove(templateId);
            }
        }

        private void LogResourceUsage()
        {
            try
            {
                var serviceCount = services.Count;
                var instanceCount = templateInstanceCount.ToList().Sum(t => t.Value.Value);
                if (lastServiceCount == serviceCount && lastInstanceCount == instanceCount) return;
                logger.InfoFormat("loadedTemplates={0} totalInstances={1}", serviceCount, instanceCount);
                lastServiceCount = serviceCount;
                lastInstanceCount = instanceCount;
            }
            catch (Exception ex)
            {
                logger.Error("Failed to log resource usage", ex);
            }
        }

        public void IncrementInstanceCount(Guid templateId, Guid instanceId)
        {
            if (shuttingDown) return;

            if(templateInstanceCount.ContainsKey(templateId))
                templateInstanceCount[templateId].Increment();
            
            LogResourceUsage();
        }

        public void DecrementInstanceCount(Guid templateId, Guid instanceId)
        {
            if (shuttingDown) return;

            if (templateInstanceCount.ContainsKey(templateId))
                templateInstanceCount[templateId].Decrement();

            LogResourceUsage();
        }

        public void Initialise(TemplateDefinition template)
        {
            if(shuttingDown)
                throw new ServerTooBusyException("Service shutting down");

            var templateId = template.Id;
            var hostUri = GetHostUri(templateId);
            if (services.ContainsKey(templateId))
            {
                if (IsServiceRunning(templateId))
                    return;
            }

            lock (lockObj)
            {
                if (services.ContainsKey(templateId))
                {
                    if (IsServiceRunning(templateId))
                        return;
                }

                var serviceImpl = GetWorkflowService(template);
                var host = new WorkflowServiceHost(serviceImpl, new Uri(hostUri));
                host.Description.Behaviors.Add(new DatabaseTrackingBehavior());
                host.Description.Behaviors.Add(new InstanceCountBehavior(this));

                host.AddServiceEndpoint(XName.Get("IDynamicWorkflow", "http://intelliflo.com/dynamicworkflow/2014/06"), binding, hostUri);
                host.AddServiceEndpoint(new WorkflowControlEndpoint(binding, new EndpointAddress(GetHostUri(templateId, "wce"))));

                templateInstanceCount.Add(templateId, new Counter());

                try
                {
                    host.Open();
                }
                finally
                {
                    services.Add(templateId, host);
                }

                logger.InfoFormat($"Action=TemplateInitialise TemplateId={templateId}");
            }
        }

        private bool IsServiceRunning(Guid templateId)
        {
            var service = services[templateId];
            if (service.State == CommunicationState.Faulted)
            {
                CloseService(templateId);
                return false;
            }
            return true;
        }

        private static WorkflowService GetWorkflowService(TemplateDefinition template)
        {
            using (var reader = new StringReader(template.Definition))
            using (var xamlReader = ActivityXamlServices.CreateBuilderReader(new XamlXmlReader(reader)))
            {
                var workflow = XamlServices.Load(xamlReader) as WorkflowService;
                if (workflow == null)
                    throw new Exception("Template definition was not a valid WorkflowService");

                workflow.DefinitionIdentity = new WorkflowIdentity() { Name = template.Id.ToString() };

                return workflow;
            }
        }

        public Guid Create(TemplateDefinition template, WorkflowContext context)
        {
            Initialise(template);
            var hostUri = GetHostUri(template.Id);

            var client = workflowClientFactory.GetDynamicClient(binding, new EndpointAddress(hostUri));
            var result = client.Call(c => c.Create(context));
            if(!result.Success)
            {
                throw new ServiceClientException("Failed to create instance");
            }
            return result.Result;
        }

        public void CreateAsync(TemplateDefinition template, WorkflowContext context)
        {
            Initialise(template);
            var hostUri = GetHostUri(template.Id);

            var client = workflowClientFactory.GetDynamicClient(binding, new EndpointAddress(hostUri));
            if (!client.Call(c => c.CreateAsync(context)).Success)
            {
                throw new ServiceClientException("Failed to create instance");
            }
        }

        public void Resume(TemplateDefinition template, ResumeContext context)
        {
            Initialise(template);
            var hostUri = GetHostUri(template.Id);

            var client = workflowClientFactory.GetDynamicClient(binding, new EndpointAddress(hostUri));
            if (!client.Call(c => c.Resume(context)).Success)
            {
                throw new ServiceClientException("Failed to resume instance");
            }
        }

        public void Abort(TemplateDefinition template, Guid instanceId)
        {
            Initialise(template);
            var hostUri = GetHostUri(template.Id, "wce");

            var client = workflowClientFactory.GetControlClient(binding, new EndpointAddress(hostUri));
            if (!client.Call(c => c.Terminate(instanceId)).Success)
            {
                throw new ServiceClientException("Failed to abort instance");
            }
        }

        public void Unsuspend(TemplateDefinition template, Guid instanceId)
        {
            Initialise(template);
            var hostUri = GetHostUri(template.Id, "wce");

            var client = workflowClientFactory.GetControlClient(binding, new EndpointAddress(hostUri));
            if (!client.Call(c => c.Unsuspend(instanceId)).Success)
            {
                throw new ServiceClientException("Failed to unsuspend instance");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;
            shuttingDown = true;

            timer.Dispose();

            lock (lockObj)
            {
                var templateIds = services.Keys.ToList();
                foreach (var templateId in templateIds)
                {
                    try
                    {
                        CloseService(templateId);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Failed to shutdown service {templateId}", ex);
                    }
                }

                services.Clear();
                templateInstanceCount.Clear();
            }
        }

        private static string GetHostUri(Guid templateId, string suffix = null)
        {
            var preparedSuffix = string.IsNullOrEmpty(suffix) ? "" : string.Format("/{0}", suffix);

            var baseAddress = Settings.Default.BaseAddress;
            var localAddress = baseAddress.Replace("*", "localhost").TrimEnd('/');

            return string.Format("{0}/services/{1}{2}", localAddress, templateId, preparedSuffix);
        }
    }
}