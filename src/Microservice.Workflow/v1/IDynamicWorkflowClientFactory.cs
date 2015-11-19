using System.ServiceModel;
using System.ServiceModel.Channels;
using Microservice.Workflow.v1.Resources;

namespace Microservice.Workflow.v1
{
    public interface IWorkflowClientFactory 
    {
        DynamicWorkflowClient GetDynamicClient(string endpointConfigurationName, EndpointAddress remoteAddress);
        IWorkflowControlClient GetControlClient(string endpointConfigurationName, EndpointAddress remoteAddress);
        DynamicWorkflowClient GetDynamicClient(Binding binding, EndpointAddress remoteAddress);
        IWorkflowControlClient GetControlClient(Binding binding, EndpointAddress remoteAddress);
    }
}