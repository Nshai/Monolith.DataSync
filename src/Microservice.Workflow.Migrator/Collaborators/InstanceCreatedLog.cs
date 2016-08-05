using System;

namespace Microservice.Workflow.Migrator.Collaborators
{
    public class InstanceCreatedLog : ILogDetail, IEquatable<InstanceCreatedLog>
    {
        public Guid TemplateId { get; set; }
        public int UserId { get; set; }
        public int TenantId { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public int RelatedEntityId { get; set; }
        public string ParentEntityType { get; set; }
        public int ParentEntityId { get; set; }
        public DateTime CreatedUtc { get; set; }

        public bool Equals(InstanceCreatedLog other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TemplateId.Equals(other.TemplateId) && UserId == other.UserId && TenantId == other.TenantId && string.Equals(EntityType, other.EntityType) && EntityId == other.EntityId && RelatedEntityId == other.RelatedEntityId && string.Equals(ParentEntityType, other.ParentEntityType) && ParentEntityId == other.ParentEntityId && CreatedUtc.Equals(other.CreatedUtc);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((InstanceCreatedLog) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = TemplateId.GetHashCode();
                hashCode = (hashCode*397) ^ UserId;
                hashCode = (hashCode*397) ^ TenantId;
                hashCode = (hashCode*397) ^ (EntityType != null ? EntityType.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ EntityId;
                hashCode = (hashCode*397) ^ RelatedEntityId;
                hashCode = (hashCode*397) ^ (ParentEntityType != null ? ParentEntityType.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ ParentEntityId;
                hashCode = (hashCode*397) ^ CreatedUtc.GetHashCode();
                return hashCode;
            }
        }
    }
}