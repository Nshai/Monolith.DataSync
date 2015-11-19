using System.ServiceModel;
using System.ServiceModel.Activities;
using System.ServiceModel.Channels;

namespace Microservice.Workflow.v1.Resources
{
    public class WorkflowClientFactory : IWorkflowClientFactory
    {
        public DynamicWorkflowClient GetDynamicClient(string endpointConfigurationName, EndpointAddress remoteAddress)
        {
            return new DynamicWorkflowClient(endpointConfigurationName, remoteAddress);
        }

        public IWorkflowControlClient GetControlClient(string endpointConfigurationName, EndpointAddress remoteAddress)
        {
            return new WorkflowControlClientWrapper(new WorkflowControlClient(endpointConfigurationName, remoteAddress));
        }

        public DynamicWorkflowClient GetDynamicClient(Binding binding, EndpointAddress remoteAddress)
        {
            return new DynamicWorkflowClient(binding, remoteAddress);
        }

        public IWorkflowControlClient GetControlClient(Binding binding, EndpointAddress remoteAddress)
        {
            return new WorkflowControlClientWrapper(new WorkflowControlClient(binding, remoteAddress));
        }
    }
}
