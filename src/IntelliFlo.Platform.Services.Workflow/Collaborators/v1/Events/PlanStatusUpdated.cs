namespace IntelliFlo.Platform.Services.Workflow.Collaborators.v1.Events
{
    public class PlanStatusUpdated
    {
        public int ProviderId { get; set; }
        public string StatusTransition { get; set; }
        public int ProductTypeId { get; set; }
        public int FromStatusId { get; set; }
        public string FromIntellifloStatus { get; set; }
        public int ToStatusId { get; set; }
        public string ToIntellifloStatus { get; set; }
    }
}