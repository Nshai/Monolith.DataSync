using System.ServiceModel;
using System.ServiceModel.Activities;
using System.ServiceModel.Channels;

namespace IntelliFlo.Platform.Services.Workflow.v1.Resources
{
    public class WorkflowClientFactory : IWorkflowClientFactory
    {
        public IDynamicWorkflow GetDynamicClient(string endpointConfigurationName, EndpointAddress remoteAddress)
        {
            return new DynamicWorkflowClient(endpointConfigurationName, remoteAddress);
        }

        public IWorkflowControlClient GetControlClient(string endpointConfigurationName, EndpointAddress remoteAddress)
        {
            return new WorkflowControlClientWrapper(new WorkflowControlClient(endpointConfigurationName, remoteAddress));
        }

        public IDynamicWorkflow GetDynamicClient(Binding binding, EndpointAddress remoteAddress)
        {
            return new DynamicWorkflowClient(binding, remoteAddress);
        }

        public IWorkflowControlClient GetControlClient(Binding binding, EndpointAddress remoteAddress)
        {
            return new WorkflowControlClientWrapper(new WorkflowControlClient(binding, remoteAddress));
        }
    }
}
