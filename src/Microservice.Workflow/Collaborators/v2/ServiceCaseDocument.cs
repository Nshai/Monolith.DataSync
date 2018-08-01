namespace Microservice.Workflow.Collaborators.v2
{
    public class ServiceCaseDocument
    {
        public UserRefDocument ServicingAdministrator { get; set; }

        public UserRefDocument Paraplanner { get; set; }
    }
}
