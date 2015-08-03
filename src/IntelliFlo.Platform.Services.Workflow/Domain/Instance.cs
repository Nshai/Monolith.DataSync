using System;
using IntelliFlo.Platform.NHibernate;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class Instance : EqualityAndHashCodeProvider<Instance, Guid>
    {
        public const int DefaultVersion = 1;

        public virtual int UserId { get; set; }
        public virtual int TenantId { get; set; }
        public virtual TemplateDefinition Template { get; set; }
        public virtual string EntityType { get; set; }
        public virtual int EntityId { get; set; }
        public virtual string ParentEntityType { get; set; }
        public virtual int ParentEntityId { get; set; }
        public virtual int RelatedEntityId { get; set; }
        public virtual DateTime CreateDate { get; set; }
        public virtual string Status { get; set; }
        public virtual Guid UniqueId { get; set; }
        public virtual Guid CorrelationId { get; set; }
        public virtual int Version { get; set; }
    }
}
