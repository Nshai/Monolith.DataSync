﻿using System;
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
