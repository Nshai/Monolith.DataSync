namespace Microservice.Workflow.Collaborators.v1.Events
{
    public class PlanCreated
    {
        public int ProviderId { get; set; }
        public int ProductTypeId { get; set; }
        public bool IsPreExisting { get; set; }
    }
}