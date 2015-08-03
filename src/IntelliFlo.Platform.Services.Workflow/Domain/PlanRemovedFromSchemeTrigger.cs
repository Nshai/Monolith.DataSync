namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class PlanRemovedFromSchemeTrigger : DeletedTrigger
    {
        public PlanRemovedFromSchemeTrigger() : base("GroupSchemePlanRemoved", WorkflowRelatedTo.Plan) { }
    }
}