using System.Collections.Generic;
using Microservice.Workflow.Collaborators.v1.Events;

namespace Microservice.Workflow.Domain
{
    public class ClientCreatedTrigger : BaseTrigger
    {
        public ClientCreatedTrigger() : base("ClientCreated", WorkflowRelatedTo.Client) {}
        public int? ClientStatusId { get; set; }
        public int[] ClientCategories { get; set; }

        public override void PopulateFromRequest(CreateTemplateTrigger request)
        {
            ClientStatusId = request.ClientStatusId;
            ClientCategories = request.ClientCategories;
        }

        public override void PopulateDocument(TemplateTrigger document)
        {
            document.ClientStatusId = ClientStatusId;
            document.ClientCategories = ClientCategories;
        }

        public override IEnumerable<BaseTriggerProperty> Serialize()
        {
            if (ClientStatusId.HasValue)
                yield return new ClientStatusTriggerProperty {StatusId = ClientStatusId.Value};
            foreach (ClientCategoryTriggerProperty property in SetPropertyArray<ClientCategoryTriggerProperty, int>(t => t.CategoryId, ClientCategories))
                yield return property;
        }

        public override void Deserialize(IList<BaseTriggerProperty> triggerProperties)
        {
            ClientStatusId = GetPropertyValue<ClientStatusTriggerProperty, int>(triggerProperties, t => t.StatusId);
            ClientCategories = GetPropertyArray<ClientCategoryTriggerProperty, int>(triggerProperties, t => t.CategoryId);
        }

        public override IEnumerable<FilterCondition> GetFilter()
        {
            if (ClientStatusId.HasValue)
                yield return ODataBuilder.BuildFilterForProperty<ClientCreated, int>(x => x.ServiceStatusId, ClientStatusId.Value);
            
            foreach (int id in ClientCategories ?? new int[0])
                yield return ODataBuilder.BuildFilterForProperty<ClientCreated, int>(x => x.CategoryId, id);
        }
    }
}