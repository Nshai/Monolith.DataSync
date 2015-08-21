Create PROCEDURE [dbo].[SpNAuditServiceCaseCategoryTrigger]
	@StampUser varchar (255),
	@ServiceCaseCategoryTriggerId int,
	@StampAction char(1)
AS

INSERT INTO TServiceCaseCategoryTriggerAudit 
(ServiceCaseCategoryTriggerId, AdviceCategoryId, TenantId, ConcurrencyId, 
  StampAction, StampDateTime, StampUser) 

Select ServiceCaseCategoryTriggerId, AdviceCategoryId, TenantId, ConcurrencyId, 
	@StampAction, GetDate(), @StampUser

FROM TServiceCaseCategoryTrigger
WHERE ServiceCaseCategoryTriggerId = @ServiceCaseCategoryTriggerId

IF @@ERROR != 0 GOTO errh

RETURN (0)

errh:
RETURN (100)