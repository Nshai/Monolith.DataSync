using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace IntelliFlo.Platform.Services.Workflow.v1.Resources
{
    public class DynamicWorkflowClient : ClientBase<IDynamicWorkflow>, IDynamicWorkflow
    {
        public DynamicWorkflowClient() { }
        public DynamicWorkflowClient(string endpointConfigurationName) : base(endpointConfigurationName) { }
        public DynamicWorkflowClient(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress) { }
        public DynamicWorkflowClient(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress) { }
        public DynamicWorkflowClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress) { }
        
        public Guid Create(WorkflowContext context)
        {
            return Channel.Create(context);
        }

        public void CreateAsync(WorkflowContext context)
        {
            Channel.CreateAsync(context);
        }

        public void Resume(ResumeContext context)
        {
            Channel.Resume(context);
        }
    }
}
