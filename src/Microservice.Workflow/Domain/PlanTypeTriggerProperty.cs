using System;

namespace Microservice.Workflow.Domain
{
    [Serializable]
    public class PlanTypeTriggerProperty : BaseTriggerProperty
    {
        public virtual int ProductTypeId { get; set; }
    }
}