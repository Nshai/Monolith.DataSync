using System;

namespace Microservice.Workflow.Domain
{
    [Serializable]
    public class ClientStatusTransitionTriggerProperty : BaseTriggerProperty, IStatusTransitionTriggerProperty
    {
        public int StatusFromId { get; set; }
        public int StatusToId { get; set; }
    }
}