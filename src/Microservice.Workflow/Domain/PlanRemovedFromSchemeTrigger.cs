namespace Microservice.Workflow.Domain
{
    public class PlanRemovedFromSchemeTrigger : DeletedTrigger
    {
        public PlanRemovedFromSchemeTrigger() : base("GroupSchemePlanRemoved", WorkflowRelatedTo.Plan) { }
    }
}