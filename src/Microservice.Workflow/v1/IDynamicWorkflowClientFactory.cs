using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microservice.Workflow.v1
{
    public interface IWorkflowClientFactory 
    {
        IDynamicWorkflow GetDynamicClient(string endpointConfigurationName, EndpointAddress remoteAddress);
        IWorkflowControlClient GetControlClient(string endpointConfigurationName, EndpointAddress remoteAddress);
        IDynamicWorkflow GetDynamicClient(Binding binding, EndpointAddress remoteAddress);
        IWorkflowControlClient GetControlClient(Binding binding, EndpointAddress remoteAddress);
    }
}