using System;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    [Serializable]
    public class PlanProviderTriggerProperty : BaseTriggerProperty
    {
        public int ProviderId { get; set; }
        public string ProviderName { get; set; }
    }
}