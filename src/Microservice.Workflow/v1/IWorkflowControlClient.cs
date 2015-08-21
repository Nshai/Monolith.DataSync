using System;

namespace Microservice.Workflow.v1
{
    public interface IWorkflowControlClient : IDisposable
    {
        void Abort(Guid instanceId);
        void Unsuspend(Guid instanceId);
    }
}