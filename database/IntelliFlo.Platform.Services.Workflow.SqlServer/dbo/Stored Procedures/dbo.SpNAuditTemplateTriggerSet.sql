Create PROCEDURE [dbo].[SpNAuditTemplateTriggerSet]
	@StampUser varchar (255),
	@TemplateTriggerSetId int,
	@StampAction char(1)
AS

INSERT INTO TTemplateTriggerSetAudit 
( TemplateTriggerSetId, TriggerType, TemplateVersionId, TenantId, ConcurrencyId, EventSubscriptionId, TriggerList,
  StampAction, StampDateTime, StampUser) 

Select TemplateTriggerSetId, TriggerType, TemplateVersionId, TenantId, ConcurrencyId, EventSubscriptionId, TriggerList,
	@StampAction, GetDate(), @StampUser

FROM TTemplateTriggerSet
WHERE TemplateTriggerSetId = @TemplateTriggerSetId

IF @@ERROR != 0 GOTO errh

RETURN (0)

errh:
RETURN (100)