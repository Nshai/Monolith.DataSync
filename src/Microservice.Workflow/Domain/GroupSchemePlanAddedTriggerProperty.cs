using System;

namespace Microservice.Workflow.Domain
{
    [Serializable]
    public class GroupSchemePlanAddedTriggerProperty : BaseTriggerProperty
    {
        public bool TriggerForNewMembersOnly { get; set; }
        public bool TriggerForRejoin { get; set; }
    }
}