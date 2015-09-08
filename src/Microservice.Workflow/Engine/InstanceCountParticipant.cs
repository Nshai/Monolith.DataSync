using System;
using System.Activities.Tracking;
using log4net;

namespace Microservice.Workflow.Engine
{
    public class InstanceCountParticipant : TrackingParticipant
    {
        private readonly IWorkflowHost host;

        public InstanceCountParticipant(IWorkflowHost host)
        {
            this.host = host;
        }

        private readonly ILog logger = LogManager.GetLogger(typeof(InstanceCountParticipant));

        protected override void Track(TrackingRecord record, TimeSpan timeout)
        {
            var instanceRecord = record as WorkflowInstanceRecord;
            if (instanceRecord != null)
            {
                var templateId = new Guid(instanceRecord.WorkflowDefinitionIdentity.Name);
                switch (instanceRecord.State)
                {
                    case "Started":
                    case "Resumed":
                        host.IncrementInstanceCount(templateId);
                        break;
                    case "Aborted":
                    case "Unloaded":
                        host.DecrementInstanceCount(templateId);
                        break;
                }
                logger.DebugFormat("InstanceId={0} InstanceState={1}", instanceRecord.InstanceId, instanceRecord.State);
            }
        }
    }
}
