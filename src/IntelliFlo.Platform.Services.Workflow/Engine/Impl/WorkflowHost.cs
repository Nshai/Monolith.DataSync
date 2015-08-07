using System;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Activities;
using System.ServiceModel.Channels;
using System.Xaml;
using System.Xml.Linq;
using IntelliFlo.Platform.Services.Workflow.Domain;
using IntelliFlo.Platform.Services.Workflow.v1;

namespace IntelliFlo.Platform.Services.Workflow.Engine.Impl
{
    public class WorkflowHost : IWorkflowHost
    {
        private readonly IWorkflowClientFactory workflowClientFactory;
        private readonly IDictionary<Guid, WorkflowServiceHost> services = new Dictionary<Guid, WorkflowServiceHost>();
        private readonly Binding binding;

        public WorkflowHost(IWorkflowClientFactory workflowClientFactory)
        {
            this.workflowClientFactory = workflowClientFactory;
            binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
        }
        
        public void Initialise(TemplateDefinition template)
        {
            var hostUri = GetHostUri(template.Id);

            if (services.ContainsKey(template.Id)) return;

            using (var reader = new StringReader(template.Definition))
            using (var xamlReader = ActivityXamlServices.CreateBuilderReader(new XamlXmlReader(reader)))
            {
                var workflow = XamlServices.Load(xamlReader) as WorkflowService;
                if(workflow == null)
                    throw new Exception("Template definition was not a valid WorkflowService");

                var host = new WorkflowServiceHost(workflow, new Uri(hostUri));

                host.AddServiceEndpoint(XName.Get("IDynamicWorkflow", "http://intelliflo.com/dynamicworkflow/2014/06"), binding, hostUri);
                host.AddServiceEndpoint(new WorkflowControlEndpoint(binding, new EndpointAddress(GetHostUri(template.Id, "wce"))));
                host.Open();

                services.Add(template.Id, host);
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

        private static string GetHostUri(Guid templateId, string suffix = null)
        {
            var preparedSuffix = string.IsNullOrEmpty(suffix) ? "" : string.Format("/{0}", suffix);
            return string.Format("net.pipe://localhost/{0}{1}", templateId, preparedSuffix);
        }
    }
}