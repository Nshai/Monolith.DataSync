using System;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.Domain;

namespace Microservice.Workflow.v1.Contracts
{
    public class TemplateMigrationDocument : Representation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TenantId { get; set; }
        public int EventSubscriptionId { get; set; }
        public Guid Guid { get; set; }
        public bool InUse { get; set; }

        public override string Href
        {
            get { return LinkTemplates.Template.Self.CreateLink(new { version = LocalConstants.ServiceVersion1, templateId = Id }).Href; }
            set { }
        }

        public override string Rel
        {
            get { return LinkTemplates.Template.Self.Rel; }
            set { }
        }

        protected override void CreateHypermedia()
        {
        }
    }
}
