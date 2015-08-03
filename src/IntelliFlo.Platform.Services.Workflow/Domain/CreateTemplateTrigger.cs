namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class CreateTemplateTrigger
    {
        public string Type { get; set; }

        public CreateTemplateTriggerStatusTransitionDefinition StatusTransition { get; set; }
        public int? ClientStatusId { get; set; }
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

        public class CreateTemplateTriggerStatusTransitionDefinition
        {
            public int? FromStatusId { get; set; }
            public int? ToStatusId { get; set; }
        }
    }
}