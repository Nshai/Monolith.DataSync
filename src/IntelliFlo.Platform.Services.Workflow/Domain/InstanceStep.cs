using System;
using IntelliFlo.Platform.NHibernate;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class InstanceStep : EqualityAndHashCodeProvider<InstanceStep, Guid>
    {
        public virtual Guid InstanceId { get; set; }
        public virtual int StepIndex { get; set; }
        public virtual string Step { get; set; }
        public virtual LogData[] Data { get; set; }
        public virtual bool IsComplete { get; set; }
        public virtual DateTime TimestampUtc { get; set; }
    }

}