namespace Microservice.Workflow.Collaborators.v1
{
    public class AdviserDocument
    {
        public int AdviserId { get; set; }
        public int? ManagerId { get; set; }
        public int? TnCCoachId { get; set; }
        public int? TnCCoachPartyId { get; set; }
    }
}
