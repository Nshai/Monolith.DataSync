using System;
using IntelliFlo.Platform.NHibernate;

namespace Microservice.Workflow.Domain
{
    public class InstanceHistory : EqualityAndHashCodeProvider<InstanceHistory, int>
    {
        public static Guid ErroredStepId = new Guid("{4B5B328D-01DE-4677-8640-E3CAB565699C}");
        public static Guid AbortedStepId = new Guid("{9F466DCF-B836-4992-9EC3-463AB9E6EB21}");
        public static Guid CompletedStepId = new Guid("{71645014-8E43-4E0A-B83E-F813A9EA1B30}");

        protected InstanceHistory() {}
        public InstanceHistory(Guid instanceId, Guid stepId, string step, DateTime timestampUtc)
        {
            InstanceId = instanceId;
            StepId = stepId;
            Step = step;
            TimestampUtc = timestampUtc;
        }

        public virtual Guid InstanceId { get; set; }
        public virtual int TenantId { get; set; }
        public virtual Guid StepId { get; set; }
        public virtual string Step { get; set; }

        public virtual LogData Data { get; set; }
        public virtual bool IsComplete { get; set; }
        public virtual DateTime TimestampUtc { get; set; }

        public static InstanceHistory Completed(Guid instanceId, DateTime timeStampUtc)
        {
            return new InstanceHistory(instanceId, CompletedStepId, StepName.Completed.ToString(), timeStampUtc) { IsComplete = true };
        }

        public static InstanceHistory Aborted(Guid instanceId, DateTime timeStampUtc)
        {
            return new InstanceHistory(instanceId, AbortedStepId, StepName.Aborted.ToString(), timeStampUtc) { IsComplete = true };
        }

        public static InstanceHistory Errored(Guid instanceId, DateTime timeStampUtc)
        {
            return new InstanceHistory(instanceId, ErroredStepId, StepName.Errored.ToString(), timeStampUtc) { IsComplete = true };
        }
    }
}