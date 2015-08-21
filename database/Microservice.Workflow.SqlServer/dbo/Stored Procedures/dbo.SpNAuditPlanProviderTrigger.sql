Create PROCEDURE [dbo].[SpNAuditPlanProviderTrigger]
	@StampUser varchar (255),
	@PlanProviderTriggerId int,
	@StampAction char(1)
AS

INSERT INTO TPlanProviderTriggerAudit
( PlanProviderTriggerId, PlanProviderId, TenantId, ConcurrencyId, 
  StampAction, StampDateTime, StampUser) 

Select PlanProviderTriggerId, PlanProviderId, TenantId, ConcurrencyId, 
	@StampAction, GetDate(), @StampUser

FROM TPlanProviderTrigger
WHERE PlanProviderTriggerId = @PlanProviderTriggerId

IF @@ERROR != 0 GOTO errh

RETURN (0)

errh:
RETURN (100)