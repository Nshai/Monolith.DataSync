namespace Microservice.Workflow.Collaborators.v1
{
    public class SubscribeRequest
    {
        public string EventType { get; set; }
        public int EntityId { get; set; }
        public string Filter { get; set; }
        public string CallbackUrl { get; set; }
        public bool IsPersistent { get; set; }
    }
}