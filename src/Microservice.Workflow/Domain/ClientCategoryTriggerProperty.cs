using System;

namespace Microservice.Workflow.Domain
{
    [Serializable]
    public class ClientCategoryTriggerProperty : BaseTriggerProperty
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}