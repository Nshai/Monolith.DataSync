using System.Collections.Generic;
using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Services.Workflow.Domain;

namespace IntelliFlo.Platform.Services.Workflow.v1.Contracts
{
    public class TemplateTriggerCollection : RepresentationCollection<TemplateTriggerDocument>
    {
        public TemplateTriggerCollection() { }
        public TemplateTriggerCollection(IList<TemplateTriggerDocument> res) : base(res) { }

        public int TemplateId { get; set; }
        public int TemplateVersionId { get; set; }

        public override string Href
        {
            get { return LinkTemplates.TemplateTrigger.Collection.CreateLink(new { version = LocalConstants.ServiceVersion1, templateId = TemplateId }).Href; }
            set { }
        }

        public override string Rel
        {
            get { return LinkTemplates.TemplateTrigger.Collection.Rel; }
            set { }
        }

        protected override void CreateHypermedia()
        {
        }
    }
}