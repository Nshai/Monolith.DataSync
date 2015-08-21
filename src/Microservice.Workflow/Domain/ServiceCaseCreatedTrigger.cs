using System.Collections.Generic;
using System.Linq;
using Microservice.Workflow.Collaborators.v1.Events;

namespace Microservice.Workflow.Domain
{
    public class ServiceCaseCreatedTrigger : BaseTrigger
    {
        public ServiceCaseCreatedTrigger() : base("ServiceCaseCreated", WorkflowRelatedTo.ServiceCase) { }

        public override void PopulateFromRequest(CreateTemplateTrigger request)
        {
            ServiceCaseCategories = request.ServiceCaseCategories;
        }

        public override void PopulateDocument(TemplateTrigger document)
        {
            document.ServiceCaseCategories = ServiceCaseCategories;
        }

        public override IEnumerable<BaseTriggerProperty> Serialize()
        {
            return SetPropertyArray<ServiceCaseCategoryTriggerProperty, int>(t => t.CategoryId, ServiceCaseCategories);
        }

        public override void Deserialize(IList<BaseTriggerProperty> triggerProperties)
        {
            ServiceCaseCategories = GetPropertyArray<ServiceCaseCategoryTriggerProperty, int>(triggerProperties, t => t.CategoryId);
        }

        public int[] ServiceCaseCategories { get; set; }

        public override IEnumerable<FilterCondition> GetFilter()
        {
            var categories = ServiceCaseCategories ?? new int[0];
            return categories.Select(id => ODataBuilder.BuildFilterForProperty<ServiceCaseCreated, int>(x => x.CategoryId, id));
        }
    }
}