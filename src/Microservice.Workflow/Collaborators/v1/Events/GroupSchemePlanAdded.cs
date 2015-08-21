namespace Microservice.Workflow.Collaborators.v1.Events
{
    public class GroupSchemePlanAdded
    {
        public int ProviderId { get; set; }
        public int ProductTypeId { get; set; }
        public int SchemeId { get; set; }
        public bool IsNewMember { get; set; }
    }
}