using IntelliFlo.Platform.Http;
using Microservice.Workflow.Domain;

namespace Microservice.Workflow.v1.Contracts
{
    public class TemplateCategoryDocument : Representation
    {
        public int TemplateCategoryId { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; }
        public bool IsArchived { get; set; }

        public override string Href
        {
            get { return LinkTemplates.TemplateCategory.Self.CreateLink(new { version = LocalConstants.ServiceVersion1, templateCategoryId = TemplateCategoryId }).Href; }
            set { }
        }

        public override string Rel
        {
            get { return LinkTemplates.TemplateCategory.Self.Rel; }
            set { }
        }

        protected override void CreateHypermedia()
        {
        }
    }
}