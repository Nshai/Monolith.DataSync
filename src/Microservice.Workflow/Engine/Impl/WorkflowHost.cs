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

                    templatesToBePurged.Promote(templateInstanceCount.Where(k => !delayedTemplates.Contains(k.Key) && k.Value.Count == 0).Select(k => k.Key));

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
                }

                templateInstanceCount.Remove(templateId);
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
                var templateRepository = new NHibernateRepository<TemplateDefinition>(session);
                return templateRepository.ReportAll<TemplateDefinition>("GetDelayedTemplates", new Parameter("Version", TemplateDefinition.DefaultVersion)).Select(t => t.Id);                
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

        public void Initialise(Guid templateId, WorkflowService serviceImpl)
        {
            var hostUri = GetHostUri(templateId);
            if (services.ContainsKey(templateId))
            {
                var service = services[templateId];
                if (service.State == CommunicationState.Faulted)
                {
                    CloseService(templateId);
                }
                else
                {
                    return;
                }
            }

            lock (lockObj)
            {
                if (services.ContainsKey(templateId)) return;
                
                var host = new WorkflowServiceHost(serviceImpl, new Uri(hostUri));
                host.Description.Behaviors.Add(new DatabaseTrackingBehavior());
                host.Description.Behaviors.Add(new InstanceCountBehavior(this));

                host.AddServiceEndpoint(XName.Get("IDynamicWorkflow", "http://intelliflo.com/dynamicworkflow/2014/06"), binding, hostUri);
                host.AddServiceEndpoint(new WorkflowControlEndpoint(binding, new EndpointAddress(GetHostUri(templateId, "wce"))));

                host.Open();

                services.Add(templateId, host);
                logger.InfoFormat("TemplateInitialise Id={0}", templateId);
            }
        }

        public void Initialise(TemplateDefinition template)
        {
            using (var reader = new StringReader(template.Definition))
            using (var xamlReader = ActivityXamlServices.CreateBuilderReader(new XamlXmlReader(reader)))
            {
                var workflow = XamlServices.Load(xamlReader) as WorkflowService;
                if (workflow == null)
                    throw new Exception("Template definition was not a valid WorkflowService");

                workflow.DefinitionIdentity = new WorkflowIdentity() { Name = template.Id.ToString() };

                Initialise(template.Id, workflow);
            }
        }

        public Guid Create(TemplateDefinition template, WorkflowContext context)
        {
            var instanceId = Guid.Empty;

            Initialise(template);
            var hostUri = GetHostUri(template.Id);

            var client = workflowClientFactory.GetDynamicClient(binding, new EndpointAddress(hostUri));
            try
            {
                instanceId = client.Create(context);
                client.Close();
            }
            catch (CommunicationException ex)
            {
                logger.Error("Service communication exception", ex);
                client.Abort();
            }
            catch (TimeoutException ex)
            {
                logger.Error("Service timeout exception", ex);
                client.Abort();
            }
            catch (Exception)
            {
                client.Abort();
                throw;
            }
            return instanceId;
        }

        public void CreateAsync(TemplateDefinition template, WorkflowContext context)
        {
            Initialise(template);
            var hostUri = GetHostUri(template.Id);

            var client = workflowClientFactory.GetDynamicClient(binding, new EndpointAddress(hostUri));
            try
            {
                client.CreateAsync(context);
                client.Close();
            }
            catch (CommunicationException ex)
            {
                logger.Error("Service communication exception", ex);
                client.Abort();
            }
            catch (TimeoutException ex)
            {
                logger.Error("Service timeout exception", ex);
                client.Abort();
            }
            catch (Exception)
            {
                client.Abort();
                throw;
            }
        }

        public void Resume(TemplateDefinition template, ResumeContext context)
        {
            Initialise(template);
            var hostUri = GetHostUri(template.Id);

            var client = workflowClientFactory.GetDynamicClient(binding, new EndpointAddress(hostUri));
            try
            {
                client.Resume(context);
                client.Close();
            }
            catch (CommunicationException ex)
            {
                logger.Error("Service communication exception", ex);
                client.Abort();
            }
            catch (TimeoutException ex)
            {
                logger.Error("Service timeout exception", ex);
                client.Abort();
            }
            catch (Exception)
            {
                client.Abort();
                throw;
            }
        }

        public void Abort(TemplateDefinition template, Guid instanceId)
        {
            Initialise(template);
            var hostUri = GetHostUri(template.Id, "wce");

            var client = workflowClientFactory.GetControlClient(binding, new EndpointAddress(hostUri));
            try
            {
                client.Terminate(instanceId);
                client.Close();
            }
            catch (CommunicationException ex)
            {
                logger.Error("Service communication exception", ex);
                client.Abort();
            }
            catch (TimeoutException ex)
            {
                logger.Error("Service timeout exception", ex);
                client.Abort();
            }
            catch (Exception)
            {
                client.Abort();
                throw;
            }
        }

        public void Unsuspend(TemplateDefinition template, Guid instanceId)
        {
            Initialise(template);
            var hostUri = GetHostUri(template.Id, "wce");

            var client = workflowClientFactory.GetControlClient(binding, new EndpointAddress(hostUri));
            try
            {
                client.Unsuspend(instanceId);
            }
            catch (CommunicationException ex)
            {
                logger.Error("Service communication exception", ex);
                client.Abort();
            }
            catch (TimeoutException ex)
            {
                logger.Error("Service timeout exception", ex);
                client.Abort();
            }
            catch (Exception)
            {
                client.Abort();
                throw;
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