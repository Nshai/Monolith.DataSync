using IntelliFlo.Platform.Http;
using IntelliFlo.Platform.Services.Workflow.Domain;

namespace IntelliFlo.Platform.Services.Workflow.v1.Contracts
{
    public class TemplateRoleDocument : Representation
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public int TemplateVersionId { get; set; }
        public int RoleId { get; set; }

        public override string Href
        {
            get { return LinkTemplates.TemplateRole.Self.CreateLink(new { version = LocalConstants.ServiceVersion1, templateId = TemplateId, stepId = Id }).Href; }
            set { }
        }

        public override string Rel
        {
            get { return LinkTemplates.TemplateRole.Self.Rel; }
            set { }
        }

        protected override void CreateHypermedia()
        {
            Links.Add(LinkTemplates.TemplateRole.Template.CreateLink(new { version = LocalConstants.ServiceVersion1, templateId = TemplateId }));
        }
    }
}