using System.Collections.Generic;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Services.Workflow.Domain;

namespace IntelliFlo.Platform.Services.Workflow.v1.Contracts
{
    public class TemplateRoleCollection : RepresentationCollection<TemplateRoleDocument>
    {
        public TemplateRoleCollection() {}
        public TemplateRoleCollection(IList<TemplateRoleDocument> res) : base(res) { }

        public int TemplateId { get; set; }
        public int TemplateVersionId { get; set; }

        public override string Href
        {
            get { return LinkTemplates.TemplateRole.Collection.CreateLink(new { version = LocalConstants.ServiceVersion1, templateId = TemplateId }).Href; }
            set { }
        }

        public override string Rel
        {
            get { return LinkTemplates.TemplateRole.Collection.Rel; }
            set { }
        }

        protected override void CreateHypermedia()
        {
        }
    }
}