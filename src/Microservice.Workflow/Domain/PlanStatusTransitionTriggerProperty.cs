using System;

namespace Microservice.Workflow.Domain
{
    [Serializable]
    public class PlanStatusTransitionTriggerProperty : BaseTriggerProperty, IStatusTransitionTriggerProperty
    {
        public virtual int StatusFromId { get; set; }
        public virtual int StatusToId { get; set; }
    }
}