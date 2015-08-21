using System;

namespace Microservice.Workflow.Domain
{
    [Serializable]
    public class PlanProviderTriggerProperty : BaseTriggerProperty
    {
        public int ProviderId { get; set; }
        public string ProviderName { get; set; }
    }
}