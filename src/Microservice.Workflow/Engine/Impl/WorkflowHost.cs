using System;
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
using IntelliFlo.Platform;
using IntelliFlo.Platform.NHibernate.Repositories;
using log4net;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1;
using NHibernate;


namespace Microservice.Workflow.Engine.Impl
{
    public class WorkflowHost : IWorkflowHost
    {
        private readonly IWorkflowClientFactory workflowClientFactory;
        private readonly IDictionary<Guid, WorkflowServiceHost> services = new Dictionary<Guid, WorkflowServiceHost>();
        private readonly IDictionary<Guid, HashSet<Guid>> templateInstanceCount = new Dictionary<Guid, HashSet<Guid>>();
        private volatile object lockObj = new object();
        private readonly Binding binding;
        private readonly ILog logger = LogManager.GetLogger(typeof(WorkflowHost));
        private readonly Timer timer;
        private bool shuttingDown;
        private int lastServiceCount;
        private int lastInstanceCount;

        public WorkflowHost(IWorkflowClientFactory workflowClientFactory)
        {
            this.workflowClientFactory = workflowClientFactory;
            binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);

            var interval = Convert.ToInt32(ConfigurationManager.AppSettings["templatePurgeIntervalSeconds"]);
            timer = new Timer(Purge, null, TimeSpan.FromSeconds(interval), TimeSpan.FromSeconds(interval));
        }

        private void Purge(object state)
        {
            try
            {
                if (shuttingDown) return;
                var delayedTemplates = GetDelayedTemplates();

                if (!Monitor.TryEnter(lockObj)) return;
                try
                {
                    if (shuttingDown) return;

                    var templatesToPurge = templateInstanceCount.Where(k => !delayedTemplates.Contains(k.Key) && k.Value.Count == 0).Select(k => k.Key).ToList();
                    if (templatesToPurge.Any())
                    {
                        foreach (var templateId in templatesToPurge.Where(templateId => services.ContainsKey(templateId)))
                        {
                            services[templateId].Close();
                            services.Remove(templateId);
                        }
                        templateInstanceCount.RemoveAll(k => templatesToPurge.Contains(k.Key));
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

        private void LogResourceUsage()
        {
            var serviceCount = services.Count;
            var instanceCount = templateInstanceCount.Sum(t => t.Value.Count);
            if (lastServiceCount == serviceCount && lastInstanceCount == instanceCount) return;
            logger.InfoFormat("loadedTemplates={0} totalInstances={1}", serviceCount, instanceCount);
            lastServiceCount = serviceCount;
            lastInstanceCount = instanceCount;
        }

        private static IEnumerable<Guid> GetDelayedTemplates()
        {
            var sessionFactory = IoC.Resolve<ISessionFactory>(Constants.ContainerId);
            using (var session = sessionFactory.OpenSession())
            {
                var instanceStepRepository = new NHibernateRepository<InstanceStep>(session);
                var instanceRepository = new NHibernateRepository<Instance>(session);

                var pausedInstanceIds = (from step in instanceStepRepository.Query()
                    where step.IsComplete == false && step.Step == StepName.Delay.ToString()
                    select step.InstanceId).ToList();

                var templateGroups = instanceRepository.Query().Where(i => pausedInstanceIds.Contains(i.Id)).Select(i => i.Template);
                return templateGroups.Where(t => t.Version >= TemplateDefinition.DefaultVersion).Select(t => t.Id).ToArray();
            }
        }

        public void IncrementInstanceCount(Guid templateId, Guid instanceId)
        {
            if (shuttingDown) return;
            lock (lockObj)
            {
                if (shuttingDown) return;

                if (!templateInstanceCount.ContainsKey(templateId))
                {
                    var instanceList = new HashSet<Guid>() { instanceId };
                    templateInstanceCount.Add(templateId, instanceList);
                }
                else
                {
                    var instanceList = templateInstanceCount[templateId];
                    if (!instanceList.Contains(instanceId))
                    {
                        instanceList.Add(instanceId);
                    }
                }
                LogResourceUsage();
            }
        }

        public void DecrementInstanceCount(Guid templateId, Guid instanceId)
        {
            if (shuttingDown) return;
            lock (lockObj)
            {
                if (shuttingDown) return;
                var instanceList = templateInstanceCount[templateId];
                if (instanceList.Contains(instanceId))
                {
                    instanceList.Remove(instanceId);
                }
                LogResourceUsage();
            }
        }

        public void Initialise(TemplateDefinition template)
        {
            var hostUri = GetHostUri(template.Id);
            if (services.ContainsKey(template.Id)) return;

            lock (lockObj)
            {
                if (services.ContainsKey(template.Id)) return;

                using (var reader = new StringReader(template.Definition))
                using (var xamlReader = ActivityXamlServices.CreateBuilderReader(new XamlXmlReader(reader)))
                {
                    var workflow = XamlServices.Load(xamlReader) as WorkflowService;
                    if (workflow == null)
                        throw new Exception("Template definition was not a valid WorkflowService");

                    workflow.DefinitionIdentity = new WorkflowIdentity() { Name = template.Id.ToString()};

                    var host = new WorkflowServiceHost(workflow, new Uri(hostUri));
                    host.Description.Behaviors.Add(new DatabaseTrackingBehavior());
                    host.Description.Behaviors.Add(new InstanceCountBehavior(this));

                    host.AddServiceEndpoint(XName.Get("IDynamicWorkflow", "http://intelliflo.com/dynamicworkflow/2014/06"), binding, hostUri);
                    host.AddServiceEndpoint(new WorkflowControlEndpoint(binding, new EndpointAddress(GetHostUri(template.Id, "wce"))));
                    host.Open();

                    services.Add(template.Id, host);
                }
            }
        }

        public Guid Create(TemplateDefinition template, WorkflowContext context)
        {
            Initialise(template);
            var hostUri = GetHostUri(template.Id);

            using (var client = workflowClientFactory.GetDynamicClient(binding, new EndpointAddress(hostUri)))
            {
                return client.Create(context);
            }
        }

        public void CreateAsync(TemplateDefinition template, WorkflowContext context)
        {
            Initialise(template);
            var hostUri = GetHostUri(template.Id);

            using (var client = workflowClientFactory.GetDynamicClient(binding, new EndpointAddress(hostUri)))
            {
                client.CreateAsync(context);
            }
        }

        public void Resume(TemplateDefinition template, ResumeContext context)
        {
            Initialise(template);
            var hostUri = GetHostUri(template.Id);

            using (var client = workflowClientFactory.GetDynamicClient(binding, new EndpointAddress(hostUri)))
            {
                client.Resume(context);
            }
        }

        public void Abort(TemplateDefinition template, Guid instanceId)
        {
            Initialise(template);
            var hostUri = GetHostUri(template.Id, "wce");

            using (var client = workflowClientFactory.GetControlClient(binding, new EndpointAddress(hostUri)))
            {
                client.Abort(instanceId);
            }
        }

        public void Unsuspend(TemplateDefinition template, Guid instanceId)
        {
            Initialise(template);
            var hostUri = GetHostUri(template.Id, "wce");

            using (var client = workflowClientFactory.GetControlClient(binding, new EndpointAddress(hostUri)))
            {
                client.Unsuspend(instanceId);
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
                foreach (var service in services.Values)
                {
                    service.Close();
                }

                services.Clear();
                templateInstanceCount.Clear();
            }
        }

        private static string GetHostUri(Guid templateId, string suffix = null)
        {
            var preparedSuffix = string.IsNullOrEmpty(suffix) ? "" : string.Format("/{0}", suffix);
            return string.Format("net.pipe://localhost/{0}{1}", templateId, preparedSuffix);
        }
    }
}