using System;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.Domain;

namespace Microservice.Workflow.v1.Contracts
{
    public class TemplateDocument : Representation
    {
        public int Id { get; set; }
        public int TemplateVersionId { get; set; }
        public string Name { get; set; }
        public int TenantId { get; set; }
        public bool? InUse { get; set; }
        public Guid Guid { get; set; }
        public int OwnerUserId { get; set; }
        public string Status { get; set; }
        public string RelatedTo { get; set; }
        public TemplateCategory Category { get; set; }
        public int? ApplicableToGroupId { get; set; }
        public bool? IncludeSubGroups { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Notes { get; set; }

        public class TemplateCategory
        {
            public int TemplateCategoryId { get; set; }
            public string Name { get; set; }
        }

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