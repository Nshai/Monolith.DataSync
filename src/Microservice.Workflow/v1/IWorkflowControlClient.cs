using System;

namespace Microservice.Workflow.v1
{
    public interface IWorkflowControlClient : IDisposable
    {
        void Abort();
        void Close();
        void Terminate(Guid instanceId);
        void Unsuspend(Guid instanceId);
    }
}