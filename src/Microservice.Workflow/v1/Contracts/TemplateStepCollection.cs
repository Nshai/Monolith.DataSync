using System.Collections.Generic;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.Domain;

namespace Microservice.Workflow.v1.Contracts
{
    public class TemplateStepCollection : RepresentationCollection<TemplateStepDocument>
    {
        public TemplateStepCollection() {}
        public TemplateStepCollection(IList<TemplateStepDocument> res): base(res) {}

        public int TemplateId { get; set; }

        public override string Href
        {
            get { return LinkTemplates.TemplateStep.Collection.CreateLink(new { version = LocalConstants.ServiceVersion1, templateId = TemplateId }).Href; }
            set { }
        }

        public override string Rel
        {
            get { return LinkTemplates.TemplateStep.Collection.Rel; }
            set { }
        }

        protected override void CreateHypermedia()
        {
        }
    }
}