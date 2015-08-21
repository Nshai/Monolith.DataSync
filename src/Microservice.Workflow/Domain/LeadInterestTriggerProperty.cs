using System;

namespace Microservice.Workflow.Domain
{
    [Serializable]
    public class LeadInterestTriggerProperty : BaseTriggerProperty
    {
        public int InterestId { get; set; }
        public string InterestName { get; set; }
    }
}