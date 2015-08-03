using System;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    [Serializable]
    public class ClientCategoryTriggerProperty : BaseTriggerProperty
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}