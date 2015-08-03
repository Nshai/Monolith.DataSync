using System;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    [Serializable]
    public class PlanTypeTriggerProperty : BaseTriggerProperty
    {
        public virtual int ProductTypeId { get; set; }
    }
}