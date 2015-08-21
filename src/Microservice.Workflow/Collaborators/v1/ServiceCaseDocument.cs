namespace Microservice.Workflow.Collaborators.v1
{
    public class ServiceCaseDocument
    {
        public int ServiceCaseId { get; set; }
        public int ClientId { get; set; }
        public int? JointClientId { get; set; }
        public int ServicingAdviserId { get; set; }
    }
}
