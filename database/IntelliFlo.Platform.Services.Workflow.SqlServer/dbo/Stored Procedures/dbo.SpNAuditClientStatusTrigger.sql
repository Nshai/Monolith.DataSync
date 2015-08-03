Create PROCEDURE [dbo].[SpNAuditClientStatusTrigger]
	@StampUser varchar (255),
	@ClientStatusTriggerId int,
	@StampAction char(1)
AS

INSERT INTO TClientStatusTriggerAudit 
( ClientStatusTriggerId, ServiceStatusId, TenantId, ConcurrencyId, 
  StampAction, StampDateTime, StampUser) 

Select ClientStatusTriggerId, ServiceStatusId, TenantId, ConcurrencyId, 
	@StampAction, GetDate(), @StampUser

FROM TClientStatusTrigger
WHERE ClientStatusTriggerId = @ClientStatusTriggerId

IF @@ERROR != 0 GOTO errh

RETURN (0)

errh:
RETURN (100)