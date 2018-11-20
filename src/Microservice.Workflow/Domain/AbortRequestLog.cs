using System;

namespace Microservice.Workflow.Domain
{
    public class AbortRequestLog : ILogDetail, IEquatable<AbortRequestLog>
    {
        public int UserRequested { get; set; }
        public string Reason { get; set; }

        public bool Equals(AbortRequestLog other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return UserRequested == other.UserRequested && Reason == other.Reason;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AbortRequestLog) obj);
        }

        public override int GetHashCode()
        {
            return UserRequested;
        }

        public override string ToString()
        {
            return string.Format("Instance aborted by user {0}", UserRequested);
        }
    }
}
