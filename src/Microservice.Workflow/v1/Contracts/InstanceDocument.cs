using System;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.Domain;

namespace Microservice.Workflow.v1.Contracts
{
    public class InstanceDocument : Representation
    {
        public Guid Id { get; set; }
        public InstanceTemplate Template { get; set; }
        public int UserId { get; set; }
        public int TenantId { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public int RelatedEntityId { get; set; }
        public string Status { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid CorrelationId { get; set; }

        public class InstanceTemplate
        {
            public Guid TemplateId { get; set; }
            public string Name { get; set; }
        }

        public override string Href
        {
            get { return LinkTemplates.Instance.Self.CreateLink(new { version = LocalConstants.ServiceVersion1, instanceId = Id.ToString("N") }).Href; }
            set { }
        }

        public override string Rel
        {
            get { return LinkTemplates.Instance.Self.Rel; }
            set { }
        }

        protected override void CreateHypermedia()
        {
            Links.Add(LinkTemplates.Instance.History.CreateLink(new { version = LocalConstants.ServiceVersion1, instanceId = Id.ToString("N") }));
        }
    }
}
