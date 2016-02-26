using System;
using System.ServiceModel;
using System.ServiceModel.Activities;

namespace Microservice.Workflow.v1.Resources
{
    public class WorkflowControlClientWrapper : IWorkflowControlClient
    {
        private readonly WorkflowControlClient client;

        public WorkflowControlClientWrapper(WorkflowControlClient client)
        {
            this.client = client;
        }

        public void Abort()
        {
            client.Abort();
        }

        public void Close()
        {
            client.Close();
        }

        public void Close(TimeSpan timeout)
        {
            ((ICommunicationObject)client).Close(timeout);
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            return ((ICommunicationObject)client).BeginClose(callback, state);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ((ICommunicationObject)client).BeginClose(timeout, callback, state);
        }

        public void EndClose(IAsyncResult result)
        {
            ((ICommunicationObject)client).EndClose(result);
        }

        public void Open()
        {
            ((ICommunicationObject)client).Open();
        }

        public void Open(TimeSpan timeout)
        {
            ((ICommunicationObject)client).Open(timeout);
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            return ((ICommunicationObject)client).BeginOpen(callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ((ICommunicationObject)client).BeginOpen(timeout, callback, state);
        }

        public void EndOpen(IAsyncResult result)
        {
            ((ICommunicationObject)client).EndOpen(result);
        }

        public CommunicationState State
        {
            get { return client.State; }
        }

        public event EventHandler Closed;
        public event EventHandler Closing;
        public event EventHandler Faulted;
        public event EventHandler Opened;
        public event EventHandler Opening;

        public void Terminate(Guid instanceId)
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
