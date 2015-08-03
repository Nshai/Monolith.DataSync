Create PROCEDURE [dbo].[SpNAuditClientStatusTransitionTrigger]
	@StampUser varchar (255),
	@ClientStatusTransitionTriggerId int,
	@StampAction char(1)
AS

INSERT INTO TClientStatusTransitionTriggerAudit 
( ClientStatusTransitionTriggerId, ServiceStatusFromId, ServiceStatusToId, TenantId, ConcurrencyId, 
  StampAction, StampDateTime, StampUser) 

Select ClientStatusTransitionTriggerId, ServiceStatusFromId, ServiceStatusToId, TenantId, ConcurrencyId, 
	@StampAction, GetDate(), @StampUser

FROM TClientStatusTransitionTrigger
WHERE ClientStatusTransitionTriggerId = @ClientStatusTransitionTriggerId

IF @@ERROR != 0 GOTO errh

RETURN (0)

errh:
RETURN (100)