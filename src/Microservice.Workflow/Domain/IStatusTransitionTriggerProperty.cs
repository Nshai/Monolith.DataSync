namespace Microservice.Workflow.Domain
{
    public interface IStatusTransitionTriggerProperty
    {
        int StatusFromId { get; set; }
        int StatusToId { get; set; }
    }
}