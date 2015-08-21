Create PROCEDURE [dbo].[SpNAuditAssignedRole]
	@StampUser varchar (255),
	@AssignedRoleId int,
	@StampAction char(1)
AS

INSERT INTO TAssignedRoleAudit 
( TenantId, TemplateVersionId, RunOnDemand, RoleId, ConcurrencyId, 
	AssignedRoleId, StampAction, StampDateTime, StampUser) 

Select TenantId, TemplateVersionId, RunOnDemand, RoleId, ConcurrencyId, 
	AssignedRoleId, @StampAction, GetDate(), @StampUser
FROM TAssignedRole
WHERE AssignedRoleId = @AssignedRoleId

IF @@ERROR != 0 GOTO errh

RETURN (0)

errh:
RETURN (100)

