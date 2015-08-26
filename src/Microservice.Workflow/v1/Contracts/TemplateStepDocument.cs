using System;
using IntelliFlo.Platform.Http;
using Microservice.Workflow.Domain;

namespace Microservice.Workflow.v1.Contracts
{
    public class TemplateStepDocument : Representation
    {
        public Guid Id { get; set; }
        public int TemplateId { get; set; }
        public string Type { get; set; }
        public int? TaskTypeId { get; set; }
        public int? Delay { get; set; }
        public bool DelayBusinessDays { get; set; }
        public string Transition { get; set; }
        public string AssignedTo { get; set; }
        public int? AssignedToPartyId { get; set; }
        public int? AssignedToRoleId { get; set; }
        public string AssignedToRoleContext { get; set; }

        public override string Href
        {
            get { return LinkTemplates.TemplateStep.Self.CreateLink(new {version = LocalConstants.ServiceVersion1, templateId = TemplateId, stepId = Id }).Href; }
            set { }
        }

        public override string Rel
        {
            get { return LinkTemplates.TemplateStep.Self.Rel; }
            set { }
        }

        protected override void CreateHypermedia()
        {
            Links.Add(LinkTemplates.TemplateStep.Template.CreateLink(new { version = LocalConstants.ServiceVersion1, templateId = TemplateId }));
        }
    }
}
