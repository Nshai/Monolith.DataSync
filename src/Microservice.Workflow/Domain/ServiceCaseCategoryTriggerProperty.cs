using System;

namespace Microservice.Workflow.Domain
{
    [Serializable]
    public class ServiceCaseCategoryTriggerProperty : BaseTriggerProperty
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}