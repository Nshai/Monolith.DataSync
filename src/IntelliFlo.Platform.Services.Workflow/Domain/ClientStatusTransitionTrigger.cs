using System.Collections.Generic;
using System.Linq;
using IntelliFlo.Platform.Services.Workflow.Collaborators.v1.Events;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class ClientStatusTransitionTrigger : StatusTransitionTrigger
    {
        public ClientStatusTransitionTrigger() : base("ClientServiceStatusUpdated", WorkflowRelatedTo.Client) { }

        public override IEnumerable<BaseTriggerProperty> Serialize()
        {
            if (StatusTransition != null)
            {
                Check.IsTrue(StatusTransition.FromStatusId.HasValue, "FromStatusId must have a value");
                Check.IsTrue(StatusTransition.ToStatusId.HasValue, "ToStatusId must have a value");

                yield return new ClientStatusTransitionTriggerProperty()
                {
                    StatusFromId = StatusTransition.FromStatusId.Value,
                    StatusToId = StatusTransition.ToStatusId.Value
                };
            }
        }

        public override void Deserialize(IList<BaseTriggerProperty> triggerProperties)
        {
            var property = triggerProperties.OfType<ClientStatusTransitionTriggerProperty>().SingleOrDefault();
            StatusTransition = property != null ? new StatusTransition(property.StatusFromId, property.StatusToId) : null;
        }

        public override IEnumerable<FilterCondition> GetFilter()
        {
            yield return ODataBuilder.BuildFilterForProperty<ClientServiceStatusUpdated, string>(x => x.ServiceStatusIdTransition, StatusTransition.ToString());
        }
    }
}