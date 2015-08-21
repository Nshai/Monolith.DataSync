using IntelliFlo.Platform.Http;
using Microservice.Workflow.Domain;

namespace Microservice.Workflow.v1.Contracts
{
    public class TemplateTriggerDocument : Representation
    {
        public int TemplateId { get; set; }
        public int TemplateVersionId { get; set; }

        public string TriggerType { get; set; }

        public int? ClientStatusId { get; set; }
        public StatusTransitionDefinition StatusTransition { get; set; }
        public int[] ClientCategories { get; set; }

        public int[] GeneralInsuranceResponses { get; set; }
        public int[] LifeAssuranceResponses { get; set; }
        public int[] ProtectionResponses { get; set; }
        public int[] InvestmentResponses { get; set; }
        public int[] PensionResponses { get; set; }
        public int[] MortagageResponses { get; set; }

        public int[] PlanTypes { get; set; }
        public int[] PlanProviders { get; set; }
        public bool? IsPreExisting { get; set; }

        public bool? GroupSchemeNewMembers { get; set; }
        public bool? GroupSchemeMemberRejoin { get; set; }

        public int[] ServiceCaseCategories { get; set; }

        public class StatusTransitionDefinition
        {
            public int FromStatusId { get; set; }
            public int ToStatusId { get; set; }
        }

        public override string Href
        {
            get { return LinkTemplates.TemplateTrigger.Self.CreateLink(new { version = LocalConstants.ServiceVersion1, templateId = TemplateId }).Href; }
            set { }
        }

        public override string Rel
        {
            get { return LinkTemplates.TemplateTrigger.Self.Rel; }
            set { }
        }

        protected override void CreateHypermedia()
        {
            Links.Add(LinkTemplates.TemplateTrigger.Template.CreateLink(new { version = LocalConstants.ServiceVersion1, templateId = TemplateId }));
        }
    }
}