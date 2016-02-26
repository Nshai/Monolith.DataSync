using System;
using System.ServiceModel;

namespace Microservice.Workflow.v1
{
    public interface IWorkflowControlClient : ICommunicationObject, IDisposable
    {
        void Abort();
        void Close();
        void Terminate(Guid instanceId);
        void Unsuspend(Guid instanceId);
    }
}