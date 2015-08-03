using System.Collections.Generic;
using System.Linq;
using IntelliFlo.Platform.Services.Workflow.Collaborators.v1.Events;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class PlanStatusTransitionTrigger : BaseTrigger
    {
        public PlanStatusTransitionTrigger() : base("PlanStatusUpdated", WorkflowRelatedTo.Plan) { }

        public int[] PlanTypes { get; set; }
        public int[] PlanProviders { get; set; }
        public StatusTransition StatusTransition { get; set; }

        public override void PopulateFromRequest(CreateTemplateTrigger request)
        {
            StatusTransition = new StatusTransition(request.StatusTransition.FromStatusId.Value, request.StatusTransition.ToStatusId.Value);
        }

        public override void PopulateDocument(TemplateTrigger document)
        {
            document.StatusTransition = new TemplateTrigger.StatusTransitionDefinition(){ FromStatusId = StatusTransition.FromStatusId.Value, ToStatusId = StatusTransition.ToStatusId.Value};
        }

        public override IEnumerable<BaseTriggerProperty> Serialize()
        {
            foreach (var property in SetPropertyArray<PlanTypeTriggerProperty, int>(t => t.ProductTypeId, PlanTypes))
                yield return property;
            foreach (var property in SetPropertyArray<PlanProviderTriggerProperty, int>(t => t.ProviderId, PlanProviders))
                yield return property;
            if (StatusTransition != null)
            {
                Check.IsTrue(StatusTransition.FromStatusId.HasValue, "FromStatusId must have a value");
                Check.IsTrue(StatusTransition.ToStatusId.HasValue, "ToStatusId must have a value");

                yield return new PlanStatusTransitionTriggerProperty()
                {
                    StatusFromId = StatusTransition.FromStatusId.Value,
                    StatusToId = StatusTransition.ToStatusId.Value
                };
            }
        }

        public override void Deserialize(IList<BaseTriggerProperty> triggerProperties)
        {
            PlanTypes = GetPropertyArray<PlanTypeTriggerProperty, int>(triggerProperties, t => t.ProductTypeId);
            PlanProviders = GetPropertyArray<PlanProviderTriggerProperty, int>(triggerProperties, t => t.ProviderId);
            var property = triggerProperties.OfType<PlanStatusTransitionTriggerProperty>().SingleOrDefault();
            StatusTransition = property != null ? new StatusTransition(property.StatusFromId, property.StatusToId) : null;
        }

        public override IEnumerable<FilterCondition> GetFilter()
        {
            foreach (var id in PlanProviders ?? new int[0])
            {
                yield return ODataBuilder.BuildFilterForProperty<PlanStatusUpdated, int>(x => x.ProviderId, id);
            }
            foreach (var id in PlanTypes ?? new int[0])
            {
                yield return ODataBuilder.BuildFilterForProperty<PlanStatusUpdated, int>(x => x.ProductTypeId, id);
            }
            yield return ODataBuilder.BuildFilterForProperty<PlanStatusUpdated, int>(x => x.FromStatusId, StatusTransition.FromStatusId.Value);
            yield return ODataBuilder.BuildFilterForProperty<PlanStatusUpdated, int>(x => x.ToStatusId, StatusTransition.ToStatusId.Value);
        }
    }
}