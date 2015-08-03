create PROCEDURE [dbo].[SpNAuditTemplateTrigger]
	@StampUser varchar (255),
	@TemplateTriggerId int,
	@StampAction char(1)
AS

INSERT INTO TTemplateTriggerAudit 
( TemplateTriggerId, TemplateTriggerSetId, TenantId, ConcurrencyId, 
  StampAction, StampDateTime, StampUser) 

Select TemplateTriggerId, TemplateTriggerSetId, TenantId, ConcurrencyId, 
	@StampAction, GetDate(), @StampUser

FROM TTemplateTrigger
WHERE TemplateTriggerId = @TemplateTriggerId

IF @@ERROR != 0 GOTO errh

RETURN (0)

errh:
RETURN (100)