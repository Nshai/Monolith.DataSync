using System;

namespace Microservice.Workflow.Migrator.Collaborators
{
    public class InstanceDocument : IRepresentation
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public int TenantId { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public int RelatedEntityId { get; set; }
        public string Status { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid CorrelationId { get; set; }
    }
}
