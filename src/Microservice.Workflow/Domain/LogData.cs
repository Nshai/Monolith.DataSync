using System;
using Newtonsoft.Json;

namespace Microservice.Workflow.Domain
{
    [Serializable]
    public class LogData : IEquatable<LogData>
    {
        [JsonConverter(typeof(JsonMappedToTypeNameTypeConverter<ILogDetail>))]
        public ILogDetail Detail { get; set; }

        public bool Equals(LogData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Detail, other.Detail);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LogData)obj);
        }

        public override int GetHashCode()
        {
            return (Detail != null ? Detail.GetHashCode() : 0);
        }
    }
}
