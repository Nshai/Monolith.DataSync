Create PROCEDURE [dbo].[SpNAuditPlanTypeTrigger]
	@StampUser varchar (255),
	@PlanTypeTriggerId int,
	@StampAction char(1)
AS

INSERT INTO TPlanTypeTriggerAudit 
( PlanTypeTriggerId, RefPlanTypeProductSubTypeId, TenantId, ConcurrencyId, 
  StampAction, StampDateTime, StampUser) 

Select PlanTypeTriggerId, RefPlanTypeProductSubTypeId, TenantId, ConcurrencyId, 
	@StampAction, GetDate(), @StampUser

FROM TPlanTypeTrigger
WHERE PlanTypeTriggerId = @PlanTypeTriggerId

IF @@ERROR != 0 GOTO errh

RETURN (0)

errh:
RETURN (100)