using System;

namespace Microservice.Workflow.Migrator.Collaborators
{
    public class CreateTaskLog : ILogDetail, IEquatable<CreateTaskLog>
    {
        public int TaskId { get; set; }
        public int? AssignedUserId { get; set; }
        public int? AssignedRoleId { get; set; }

        public bool Equals(CreateTaskLog other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TaskId == other.TaskId && AssignedRoleId == other.AssignedRoleId && AssignedUserId == other.AssignedUserId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CreateTaskLog) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = TaskId;
                if (AssignedUserId.HasValue)
                    hashCode = (hashCode * 397) ^ AssignedUserId.Value;
                if (AssignedRoleId.HasValue)
                    hashCode = (hashCode * 397) ^ AssignedRoleId.Value;
                return hashCode;
            }
        }
    }
}