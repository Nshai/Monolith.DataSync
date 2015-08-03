using System;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    [Serializable]
    public class ClientStatusTriggerProperty : BaseTriggerProperty
    {
        public virtual int StatusId { get; set; }
    }
}