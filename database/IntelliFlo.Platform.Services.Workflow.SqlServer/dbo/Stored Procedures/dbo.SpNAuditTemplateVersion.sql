Create PROCEDURE [dbo].[SpNAuditTemplateVersion]
	@StampUser varchar (255),
	@TemplateVersionId int,
	@StampAction char(1)
AS

INSERT INTO TTemplateVersionAudit 
( TenantId, TemplateVersionId, Guid,  Status, CreatedDate, OwnerUserId, Notes,
	ApplicableToGroupId, IncludeSubGroups, Definition, TriggerDefinition, ConcurrencyId, 
	TemplateId, StampAction, StampDateTime, StampUser) 

Select TenantId, TemplateVersionId, Guid, Status, CreatedDate, OwnerUserId, Notes,
	ApplicableToGroupId, IncludeSubGroups, Definition, TriggerDefinition, ConcurrencyId, 
	TemplateId, @StampAction, GetDate(), @StampUser

FROM TTemplateVersion
WHERE TemplateVersionId = @TemplateVersionId

IF @@ERROR != 0 GOTO errh

RETURN (0)

errh:
RETURN (100)

