namespace Microservice.Workflow.Domain
{
    public class ServiceCaseDeletedTrigger : DeletedTrigger
    {
        public ServiceCaseDeletedTrigger() : base("ServiceCaseDeleted", WorkflowRelatedTo.ServiceCase) { }
    }
}