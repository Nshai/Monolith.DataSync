using System.Collections.Generic;

namespace Microservice.Workflow.Domain
{
    public class NoneTrigger : BaseTrigger
    {
        public override void PopulateFromRequest(CreateTemplateTrigger request){}

        public override IEnumerable<BaseTriggerProperty> Serialize()
        {
            return new BaseTriggerProperty[0];
        }

        public override void Deserialize(IList<BaseTriggerProperty> triggerProperties){}

        public override void PopulateDocument(TemplateTrigger document){}

        public override IEnumerable<FilterCondition> GetFilter()
        {
            return new FilterCondition[0];
        }
    }
}