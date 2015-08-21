using System.Collections.Generic;
using Microservice.Workflow.Collaborators.v1.Events;

namespace Microservice.Workflow.Domain
{
    public class PlanAddedToSchemeTrigger : BaseTrigger
    {
        public PlanAddedToSchemeTrigger() : base("GroupSchemePlanAdded", WorkflowRelatedTo.Plan) { }

        public override void PopulateFromRequest(CreateTemplateTrigger request)
        {
            PlanTypes = request.PlanTypes;
            PlanProviders = request.PlanProviders;
            GroupSchemeNewMembers = request.GroupSchemeNewMembers;
            GroupSchemeMemberRejoin = request.GroupSchemeMemberRejoin;
        }

        public override void PopulateDocument(TemplateTrigger document)
        {
            document.PlanTypes = PlanTypes;
            document.PlanProviders = PlanProviders;
            document.GroupSchemeNewMembers = GroupSchemeNewMembers;
            document.GroupSchemeMemberRejoin = GroupSchemeMemberRejoin;
        }

        public override IEnumerable<BaseTriggerProperty> Serialize()
        {
            foreach (var property in SetPropertyArray<PlanTypeTriggerProperty, int>(t => t.ProductTypeId, PlanTypes))
                yield return property;
            foreach (var property in SetPropertyArray<PlanProviderTriggerProperty, int>(t => t.ProviderId, PlanProviders))
                yield return property;

            yield return new GroupSchemePlanAddedTriggerProperty()
            {
                TriggerForNewMembersOnly = GroupSchemeNewMembers ?? false,
                TriggerForRejoin = GroupSchemeMemberRejoin ??  false
            };
        }

        public override void Deserialize(IList<BaseTriggerProperty> triggerProperties)
        {
            PlanTypes = GetPropertyArray<PlanTypeTriggerProperty, int>(triggerProperties, t => t.ProductTypeId);
            PlanProviders = GetPropertyArray<PlanProviderTriggerProperty, int>(triggerProperties, t => t.ProviderId);
            GroupSchemeNewMembers = GetPropertyValue<GroupSchemePlanAddedTriggerProperty, bool>(triggerProperties, t => t.TriggerForNewMembersOnly);
            GroupSchemeMemberRejoin = GetPropertyValue<GroupSchemePlanAddedTriggerProperty, bool>(triggerProperties, t => t.TriggerForRejoin);
        }

        public int[] PlanTypes { get; set; }
        public int[] PlanProviders { get; set; }
        public bool? GroupSchemeNewMembers { get; set; }
        public bool? GroupSchemeMemberRejoin { get; set; }

        public override IEnumerable<FilterCondition> GetFilter()
        {
            foreach (var id in PlanProviders ?? new int[0])
            {
                yield return ODataBuilder.BuildFilterForProperty<GroupSchemePlanAdded, int>(x => x.ProviderId, id);
            }
            foreach (var id in PlanTypes ?? new int[0])
            {
                yield return ODataBuilder.BuildFilterForProperty<GroupSchemePlanAdded, int>(x => x.ProductTypeId, id);
            }
            var newMembers = GroupSchemeNewMembers ?? false;
            var rejoin = GroupSchemeMemberRejoin ?? false;
            if (newMembers == rejoin) yield break;
            if (newMembers)
                yield return ODataBuilder.BuildFilterForProperty<GroupSchemePlanAdded, bool>(x => x.IsNewMember, true);
            else
                yield return ODataBuilder.BuildFilterForProperty<GroupSchemePlanAdded, bool>(x => x.IsNewMember, false);
        }
    }
}