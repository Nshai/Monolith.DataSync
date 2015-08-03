using System;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    [Serializable]
    public class PlanNewBusinessTriggerProperty : BaseTriggerProperty
    {
        public virtual bool IsPreExisting { get; set; }
    }
}