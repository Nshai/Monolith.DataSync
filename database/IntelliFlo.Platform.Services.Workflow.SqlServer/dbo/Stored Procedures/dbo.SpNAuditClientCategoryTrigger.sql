Create PROCEDURE [dbo].[SpNAuditClientCategoryTrigger]
	@StampUser varchar (255),
	@ClientCategoryTriggerId int,
	@StampAction char(1)
AS

INSERT INTO TClientCategoryTriggerAudit 
(ClientCategoryTriggerId, ClientTypeId, TenantId, ConcurrencyId, 
  StampAction, StampDateTime, StampUser) 

Select ClientCategoryTriggerId, ClientTypeId, TenantId, ConcurrencyId, 
	@StampAction, GetDate(), @StampUser

FROM TClientCategoryTrigger
WHERE ClientCategoryTriggerId = @ClientCategoryTriggerId

IF @@ERROR != 0 GOTO errh

RETURN (0)

errh:
RETURN (100)