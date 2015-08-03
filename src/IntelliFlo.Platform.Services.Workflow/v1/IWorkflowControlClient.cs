using System;

namespace IntelliFlo.Platform.Services.Workflow.v1
{
    public interface IWorkflowControlClient : IDisposable
    {
        void Abort(Guid instanceId);
        void Unsuspend(Guid instanceId);
    }
}