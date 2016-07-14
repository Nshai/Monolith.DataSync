using System;

namespace Microservice.Workflow.Migrator.Collaborators
{
    public class DelayLog : ILogDetail, IEquatable<DelayLog>
    {
        public DateTime DelayUntil { get; set; }

        public bool Equals(DelayLog other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return DelayUntil.Equals(other.DelayUntil);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DelayLog) obj);
        }

        public override int GetHashCode()
        {
            return DelayUntil.GetHashCode();
        }
    }
}
