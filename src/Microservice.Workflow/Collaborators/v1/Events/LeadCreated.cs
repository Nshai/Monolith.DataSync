namespace Microservice.Workflow.Collaborators.v1.Events
{
    public class LeadCreated
    {
        public InterestArea[] InterestAreas { get; set; }
    }

    public class InterestArea
    {
        public int InterestAreaId { get; set; }
    }
}