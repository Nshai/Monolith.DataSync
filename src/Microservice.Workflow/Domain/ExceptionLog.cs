using System;

namespace Microservice.Workflow.Domain
{
    public class ExceptionLog : ILogDetail, IEquatable<ExceptionLog>
    {
        public string ErrorId { get; set; }

        public bool Equals(ExceptionLog other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(ErrorId, other.ErrorId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ExceptionLog) obj);
        }

        public override int GetHashCode()
        {
            return (ErrorId != null ? ErrorId.GetHashCode() : 0);
        }
    }
}