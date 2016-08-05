using System;
using IntelliFlo.Platform.NHibernate;

namespace Microservice.Workflow.Domain
{
    public class InstanceStep : EqualityAndHashCodeProvider<InstanceStep, string>
    {
        public virtual Guid InstanceId { get; set; }
        public virtual int TenantId { get; set; }
        public virtual Guid StepId { get; set; }
        public virtual int StepIndex { get; set; }
        public virtual string Step { get; set; }
        public virtual LogData[] Data { get; set; }
        public virtual bool IsComplete { get; set; }
        public virtual DateTime TimestampUtc { get; set; }
    }

}