﻿using System;

namespace Microservice.Workflow.Domain
{
    public class CreateTaskLog : ILogDetail, IEquatable<CreateTaskLog>
    {
        public int TaskId { get; set; }
        public int? AssignedUserId { get; set; }
        public int? AssignedRoleId { get; set; }
        public int? AssignedPartyId { get; set; }

        public bool Equals(CreateTaskLog other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TaskId == other.TaskId && AssignedUserId == other.AssignedUserId && AssignedRoleId == other.AssignedRoleId && AssignedPartyId == other.AssignedPartyId;
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
                if (AssignedPartyId.HasValue)
                    hashCode = (hashCode * 397) ^ AssignedPartyId.Value;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("Created task (id: {0}) assigned to {2} by user (id: {1})", TaskId, AssignedUserId, AssignedPartyId.HasValue ? string.Format("party (id: {0})", AssignedPartyId) : string.Format("role (id: {0})", AssignedRoleId));
        }
    }
}