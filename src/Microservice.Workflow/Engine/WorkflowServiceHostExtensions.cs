using System;
using System.ServiceModel;
using System.ServiceModel.Activities;

namespace Microservice.Workflow.Engine
{
    public static class WorkflowServiceHostExtensions
    {
        public static void TryClose(this WorkflowServiceHost host)
        {
            try
            {
                host.Close();
            }
            catch (CommunicationException)
            {
                host.Abort();
            }
            catch (TimeoutException)
            {
                host.Abort();   
            }
        }
    }
}
