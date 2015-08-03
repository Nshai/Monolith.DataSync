Create PROCEDURE [dbo].[SpNAuditServiceCaseStatusTransitionTrigger]
	@StampUser varchar (255),
	@ServiceCaseStatusTransitionTriggerId int,
	@StampAction char(1)
AS

INSERT INTO TServiceCaseStatusTransitionTriggerAudit 
( ServiceCaseStatusTransitionTriggerId, AdviceCaseFromId, AdviceCaseToId, TenantId, ConcurrencyId, 
  StampAction, StampDateTime, StampUser) 

Select ServiceCaseStatusTransitionTriggerId, AdviceCaseFromId, AdviceCaseToId, TenantId, ConcurrencyId, 
	@StampAction, GetDate(), @StampUser

FROM TServiceCaseStatusTransitionTrigger
WHERE ServiceCaseStatusTransitionTriggerId = @ServiceCaseStatusTransitionTriggerId

IF @@ERROR != 0 GOTO errh

RETURN (0)

errh:
RETURN (100)