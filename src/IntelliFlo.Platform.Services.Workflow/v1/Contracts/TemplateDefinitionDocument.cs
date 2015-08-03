using System;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Services.Workflow.Domain;

namespace IntelliFlo.Platform.Services.Workflow.v1.Contracts
{
    public class TemplateDefinitionDocument : Representation
    {
        public Guid TemplateId { get; set; }
        public int TenantId { get; set; }
        public string Definition { get; set; }
        public bool InUse { get; set; }

        public override string Href
        {
            get { return LinkTemplates.TemplateDefinition.Self.CreateLink(new { version = LocalConstants.ServiceVersion1, templateId = TemplateId.ToString("N") }).Href; }
            set { }
        }

        public override string Rel
        {
            get { return LinkTemplates.TemplateDefinition.Self.Rel; }
            set { }
        }

        protected override void CreateHypermedia()
        {
        }
    }
}