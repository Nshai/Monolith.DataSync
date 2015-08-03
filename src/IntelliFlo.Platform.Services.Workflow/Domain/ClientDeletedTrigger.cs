namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class ClientDeletedTrigger : DeletedTrigger
    {
        public ClientDeletedTrigger() : base("ClientDeleted", WorkflowRelatedTo.Client) { }
    }
}