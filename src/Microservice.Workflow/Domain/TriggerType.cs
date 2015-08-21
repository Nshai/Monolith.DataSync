namespace Microservice.Workflow.Domain
{
    public enum TriggerType
    {
        None,
        OnClientCreation,
        OnClientDeletion,
        OnClientStatusUpdate,
        OnLeadCreation,
        OnLeadDeletion,
        OnLeadStatusUpdate,
        OnPlanCreation,
        OnPlanStatusUpdate,
        OnServiceCaseCreation,
        OnServiceCaseDeletion,
        OnServiceCaseStatusUpdate,
        OnPlanAddedToScheme,
        OnPlanRemovedFromScheme
    }
}
