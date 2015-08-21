namespace Microservice.Workflow.Domain
{
    public class ClientDeletedTrigger : DeletedTrigger
    {
        public ClientDeletedTrigger() : base("ClientDeleted", WorkflowRelatedTo.Client) { }
    }
}