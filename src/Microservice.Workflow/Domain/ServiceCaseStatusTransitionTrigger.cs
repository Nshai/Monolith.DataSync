using System.Collections.Generic;
using System.Linq;
using IntelliFlo.Platform;
using Microservice.Workflow.Collaborators.v1.Events;

namespace Microservice.Workflow.Domain
{
    public class ServiceCaseStatusTransitionTrigger : StatusTransitionTrigger
    {
        public ServiceCaseStatusTransitionTrigger() : base("ServiceCaseStatusUpdated", WorkflowRelatedTo.ServiceCase) { }

        public override IEnumerable<BaseTriggerProperty> Serialize()
        {
            if (StatusTransition != null)
            {
                Check.IsTrue(StatusTransition.FromStatusId.HasValue, "FromStatusId must have a value");
                Check.IsTrue(StatusTransition.ToStatusId.HasValue, "ToStatusId must have a value");

                yield return new ServiceCaseStatusTransitionTriggerProperty()
                {
                    StatusFromId = StatusTransition.FromStatusId.Value,
                    StatusToId = StatusTransition.ToStatusId.Value
                };
            }
        }

        public override void Deserialize(IList<BaseTriggerProperty> triggerProperties)
        {
            var property = triggerProperties.OfType<ServiceCaseStatusTransitionTriggerProperty>().SingleOrDefault();
            StatusTransition = property != null ? new StatusTransition(property.StatusFromId, property.StatusToId) : null;
        }

        public override IEnumerable<FilterCondition> GetFilter()
        {
            yield return ODataBuilder.BuildFilterForProperty<ServiceCaseStatusUpdated, string>(x => x.StatusIdTransition, StatusTransition.ToString());
        }
    }
}