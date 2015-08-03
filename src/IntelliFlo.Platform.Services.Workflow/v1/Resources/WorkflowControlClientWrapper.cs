using System;
using System.ServiceModel.Activities;

namespace IntelliFlo.Platform.Services.Workflow.v1.Resources
{
    public class WorkflowControlClientWrapper : IWorkflowControlClient
    {
        private readonly WorkflowControlClient client;

        public WorkflowControlClientWrapper(WorkflowControlClient client)
        {
            this.client = client;
        }

        public void Abort(Guid instanceId)
        {
            client.Terminate(instanceId);
        }

        public void Unsuspend(Guid instanceId)
        {
            client.Unsuspend(instanceId);
        }

        public void Dispose()
        {
            var disposable = (client as IDisposable);
            if (disposable != null)
                disposable.Dispose();
        }
    }
}
