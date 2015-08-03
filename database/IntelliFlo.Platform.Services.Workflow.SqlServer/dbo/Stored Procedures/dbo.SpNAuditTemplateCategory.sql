

Create PROCEDURE [dbo].[SpNAuditTemplateCategory]
	@StampUser varchar (255),
	@TemplateCategoryId int,
	@StampAction char(1)
AS

INSERT INTO TTemplateCategoryAudit 
	( TenantId, Name, IsArchived, ConcurrencyId, TemplateCategoryId, StampAction, StampDateTime, StampUser) 

Select TenantId, Name, IsArchived, ConcurrencyId, TemplateCategoryId, @StampAction, GetDate(), @StampUser
FROM TTemplateCategory
WHERE TemplateCategoryId = @TemplateCategoryId

IF @@ERROR != 0 GOTO errh

RETURN (0)

errh:
RETURN (100)


