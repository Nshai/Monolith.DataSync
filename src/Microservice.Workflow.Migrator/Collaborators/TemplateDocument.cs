using System;

namespace Microservice.Workflow.Migrator.Collaborators
{
    public class TemplateDocument : IRepresentation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TenantId { get; set; }
        public int EventSubscriptionId { get; set; }
        public Guid Guid { get; set; }
        public bool InUse { get; set; }
    }
}
