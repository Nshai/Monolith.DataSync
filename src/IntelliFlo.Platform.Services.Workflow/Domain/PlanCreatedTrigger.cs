using System.Collections.Generic;
using IntelliFlo.Platform.Services.Workflow.Collaborators.v1.Events;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class PlanCreatedTrigger : BaseTrigger
    {
        public PlanCreatedTrigger() : base("PlanCreated", WorkflowRelatedTo.Plan) { }

        public override void PopulateFromRequest(CreateTemplateTrigger request)
        {
            PlanTypes = request.PlanTypes;
            PlanProviders = request.PlanProviders;
            IsPreExisting = request.IsPreExisting;
        }

        public override void PopulateDocument(TemplateTrigger document)
        {
            document.PlanTypes = PlanTypes;
            document.PlanProviders = PlanProviders;
            document.IsPreExisting = IsPreExisting;
        }

        public override IEnumerable<BaseTriggerProperty> Serialize()
        {
            foreach (var property in SetPropertyArray<PlanTypeTriggerProperty, int>(t => t.ProductTypeId, PlanTypes))
                yield return property;
            foreach (var property in SetPropertyArray<PlanProviderTriggerProperty, int>(t => t.ProviderId, PlanProviders))
                yield return property;
            if (IsPreExisting.HasValue && !IsPreExisting.Value)
                yield return SetPropertyValue<PlanNewBusinessTriggerProperty, bool>(t => t.IsPreExisting, IsPreExisting);
        }

        public override void Deserialize(IList<BaseTriggerProperty> triggerProperties)
        {
            PlanTypes = GetPropertyArray<PlanTypeTriggerProperty, int>(triggerProperties, t => t.ProductTypeId);
            PlanProviders = GetPropertyArray<PlanProviderTriggerProperty, int>(triggerProperties, t => t.ProviderId);
            IsPreExisting = GetPropertyValue<PlanNewBusinessTriggerProperty, bool>(triggerProperties, t => t.IsPreExisting);
        }

        public int[] PlanTypes { get; set; }
        public int[] PlanProviders { get; set; }
        public bool? IsPreExisting { get; set; }

        public override IEnumerable<FilterCondition> GetFilter()
        {
            foreach (var id in PlanProviders ?? new int[0])
            {
                yield return ODataBuilder.BuildFilterForProperty<PlanCreated, int>(x => x.ProviderId, id);
            }
            foreach (var id in PlanTypes ?? new int[0])
            {
                yield return ODataBuilder.BuildFilterForProperty<PlanCreated, int>(x => x.ProductTypeId, id);
            }
            if (IsPreExisting.HasValue)
                yield return ODataBuilder.BuildFilterForProperty<PlanCreated, bool>(x => x.IsPreExisting, IsPreExisting.Value);
        }
    }
}