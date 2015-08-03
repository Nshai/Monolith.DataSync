namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class TemplateTrigger
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
    }
}