Create PROCEDURE [dbo].[SpNAuditTemplate]
	@StampUser varchar (255),
	@TemplateId int,
	@StampAction char(1)
AS

INSERT INTO TTemplateAudit 
( TenantId, Name, RelatedTo, TemplateCategoryId, CreatedDate, ConcurrencyId, 
	TemplateId, StampAction, StampDateTime, StampUser) 

Select TenantId, Name, RelatedTo, TemplateCategoryId, CreatedDate, ConcurrencyId,  
	TemplateId, @StampAction, GetDate(), @StampUser

FROM TTemplate
WHERE TemplateId = @TemplateId

IF @@ERROR != 0 GOTO errh

RETURN (0)

errh:
RETURN (100)

