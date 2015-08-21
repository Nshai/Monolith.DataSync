Create PROCEDURE [dbo].[SpNAuditPlanStatusTrigger]
	@StampUser varchar (255),
	@PlanStatusTriggerId int,
	@StampAction char(1)
AS

INSERT INTO TPlanStatusTriggerAudit
( PlanStatusTriggerId, PlanStatusFrom, PlanStatusTo, TenantId, ConcurrencyId, 
  StampAction, StampDateTime, StampUser) 

Select PlanStatusTriggerId, PlanStatusFrom, PlanStatusTo, TenantId, ConcurrencyId, 
	@StampAction, GetDate(), @StampUser

FROM TPlanStatusTrigger
WHERE PlanStatusTriggerId = @PlanStatusTriggerId

IF @@ERROR != 0 GOTO errh

RETURN (0)

errh:
RETURN (100)