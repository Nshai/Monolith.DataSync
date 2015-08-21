namespace Microservice.Workflow.Collaborators.v1
{
    public class PlanPatchRequest
    {
        public bool? IsVisibilityUpdatedByStatusChange { get; set; }
        public bool? IsVisibleToClient { get; set; }
    }
}
