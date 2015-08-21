namespace Microservice.Workflow.Domain
{
    public class LeadDeletedTrigger : DeletedTrigger
    {
        public LeadDeletedTrigger() : base("LeadDeleted", WorkflowRelatedTo.Lead) { }
    }
}