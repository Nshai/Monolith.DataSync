using System;

namespace Microservice.Workflow.Domain
{
    public static class TemplateTriggerFactory
    {
        public static BaseTrigger CreateFromRequest(CreateTemplateTrigger request)
        {
            var type = (TriggerType)Enum.Parse(typeof(TriggerType), request.Type);
            var trigger = Create(type);
            trigger.PopulateFromRequest(request);

            return trigger;
        }

        public static BaseTrigger Create(TriggerType type)
        {
            switch (type)
            {
                case TriggerType.None:
                    return new NoneTrigger();
                case TriggerType.OnClientCreation:
                    return new ClientCreatedTrigger();
                case TriggerType.OnClientDeletion:
                    return new ClientDeletedTrigger();
                case TriggerType.OnClientStatusUpdate:
                    return new ClientStatusTransitionTrigger();
                case TriggerType.OnLeadCreation:
                    return new LeadCreatedTrigger();
                case TriggerType.OnLeadDeletion:
                    return new LeadDeletedTrigger();
                case TriggerType.OnLeadStatusUpdate:
                    return new LeadStatusTransitionTrigger();
                case TriggerType.OnPlanCreation:
                    return new PlanCreatedTrigger();
                case TriggerType.OnPlanStatusUpdate:
                    return new PlanStatusTransitionTrigger();
                case TriggerType.OnPlanAddedToScheme:
                    return new PlanAddedToSchemeTrigger();
                case TriggerType.OnPlanRemovedFromScheme:
                    return new PlanRemovedFromSchemeTrigger();
                case TriggerType.OnServiceCaseCreation:
                    return new ServiceCaseCreatedTrigger();
                case TriggerType.OnServiceCaseDeletion:
                    return new ServiceCaseDeletedTrigger();
                case TriggerType.OnServiceCaseStatusUpdate:
                    return new ServiceCaseStatusTransitionTrigger();

                default:
                    throw new ArgumentOutOfRangeException("type", "Trigger type not known");
            }
        }
    }
}
