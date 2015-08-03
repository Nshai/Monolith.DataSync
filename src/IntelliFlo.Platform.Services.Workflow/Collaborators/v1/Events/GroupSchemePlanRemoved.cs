namespace IntelliFlo.Platform.Services.Workflow.Collaborators.v1.Events
{
    public class GroupSchemePlanRemoved
    {
        public int ProviderId { get; set; }
        public int ProductTypeId { get; set; }
        public int SchemeId { get; set; }
    }
}