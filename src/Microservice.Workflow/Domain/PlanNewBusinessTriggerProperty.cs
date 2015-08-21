using System;

namespace Microservice.Workflow.Domain
{
    [Serializable]
    public class PlanNewBusinessTriggerProperty : BaseTriggerProperty
    {
        public virtual bool IsPreExisting { get; set; }
    }
}