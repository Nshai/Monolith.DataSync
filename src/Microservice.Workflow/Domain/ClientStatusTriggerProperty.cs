using System;

namespace Microservice.Workflow.Domain
{
    [Serializable]
    public class ClientStatusTriggerProperty : BaseTriggerProperty
    {
        public virtual int StatusId { get; set; }
    }
}