using System.Collections.Generic;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public abstract class DeletedTrigger : BaseTrigger
    {
        protected DeletedTrigger(string eventName, WorkflowRelatedTo relatedTo) : base(eventName, relatedTo) { }

        public override void PopulateFromRequest(CreateTemplateTrigger request){}
        public override void PopulateDocument(TemplateTrigger document){}

        public override IEnumerable<BaseTriggerProperty> Serialize()
        {
            return new BaseTriggerProperty[0];
        }

        public override void Deserialize(IList<BaseTriggerProperty> triggerProperties){}

        public override IEnumerable<FilterCondition> GetFilter()
        {
            return new FilterCondition[0];
        }
    }
}