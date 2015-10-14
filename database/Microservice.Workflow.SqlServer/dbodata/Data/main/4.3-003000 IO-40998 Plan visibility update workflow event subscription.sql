USE workflow

DECLARE @ScriptGUID UNIQUEIDENTIFIER
      , @Comments VARCHAR(255)
      , @ErrorMessage VARCHAR(MAX)
	
SELECT @ScriptGUID = '0208844C-6E37-49A9-BB42-12C0F2E60839'
      , @Comments = '4.3-003000 IO-40998 Plan visibility update workflow event subscription'
      
IF EXISTS (SELECT 1 FROM TExecutedDataScript WHERE ScriptGUID = @ScriptGUID)
	RETURN; 

DECLARE @TemplateGuid UNIQUEIDENTIFIER = 'DAFB179F-FC00-4E82-984F-8DC0D7304705'
		, @TenantId INT = 0
		, @UserId INT = 0
		, @TemplateId INT = 111111
		, @maxTemplateId INT

IF(OBJECT_ID('tempdb..#templates') IS NOT NULL) DROP TABLE #templates
CREATE TABLE #templates(TemplateId INT, Name VARCHAR(255), [Guid] UNIQUEIDENTIFIER, SubscriptionContext VARCHAR(255))

INSERT INTO #templates(TemplateId, Name, [Guid], SubscriptionContext)
SELECT @TemplateId, 'Plan visibility update', @TemplateGuid, '(FromIntellifloStatus eq ''In force'' or FromIntellifloStatus eq ''Paid Up'') or (ToIntellifloStatus eq ''In force'' or ToIntellifloStatus eq ''Paid Up'' or ToIntellifloStatus eq ''Draft'')'

SELECT @maxTemplateId = ISNULL(MAX(TemplateId), 0) + 1 
FROM TTemplate

BEGIN TRANSACTION
BEGIN TRY

DELETE FROM workflow..TTemplateCategory
WHERE Name = 'System'

DELETE FROM workflow..TTemplateVersion
WHERE TemplateId IN (@TemplateId, 111112)

DELETE FROM workflow..TTemplate
WHERE TemplateId IN (@TemplateId, 111112)

IF NOT EXISTS(SELECT 1 FROM TTemplateCategory WHERE Name = 'System' and TenantId = @TenantId)
INSERT INTO TTemplateCategory(TenantId,	Name,	IsArchived,	ConcurrencyId)
SELECT	@TenantId,	'System',	0,	1

SET IDENTITY_INSERT TTemplate ON
INSERT INTO TTemplate (TemplateId, TenantId,	Name,	RelatedTo,	TemplateCategoryId,	CreatedDate,	ConcurrencyId)
	OUTPUT INSERTED.TemplateId, INSERTED.TenantId, INSERTED.Name, INSERTED.RelatedTo, INSERTED.TemplateCategoryId, INSERTED.CreatedDate, INSERTED.ConcurrencyId, 'C', GETDATE(),'0000'
	INTO TTemplateAudit(TemplateId, TenantId, Name, RelatedTo, TemplateCategoryId, CreatedDate, ConcurrencyId, StampAction, StampDateTime, StampUser)
SELECT	TemplateId, @TenantId,	Name,	'Plan',	1,	GETDATE(),	1
FROM #templates
SET IDENTITY_INSERT TTemplate OFF

DBCC CHECKIDENT('TTemplate', RESEED, @maxTemplateId)

INSERT INTO TTemplateVersion(TenantId,	TemplateId,	[Guid],	[Status],	CreatedDate,	OwnerUserId,	ApplicableToGroupId,	IncludeSubGroups,	[Definition],	ConcurrencyId)
	OUTPUT INSERTED.TemplateVersionId, INSERTED.TenantId,	INSERTED.TemplateId,	INSERTED.[Guid],	INSERTED.[Status],	INSERTED.CreatedDate,	INSERTED.OwnerUserId,	INSERTED.ApplicableToGroupId,	INSERTED.IncludeSubGroups,	INSERTED.[Definition],	INSERTED.ConcurrencyId, 'C', GETDATE(),'0000'
	INTO TTemplateVersionAudit(TemplateVersionId, TenantId,	TemplateId,	[Guid],	[Status],	CreatedDate,	OwnerUserId,	ApplicableToGroupId,	IncludeSubGroups,	[Definition],	ConcurrencyId, StampAction, StampDateTime, StampUser)
SELECT 	@TenantId,	TemplateId,	[Guid],	'Active',	GETDATE(),	@UserId,	NULL,	0,	'{"Steps":[]}',	1
From #templates

END TRY
	
BEGIN CATCH
	
	SET @ErrorMessage = ERROR_MESSAGE()
	RAISERROR(@ErrorMessage, 16, 1)
	WHILE(@@TRANCOUNT > 0)ROLLBACK
	RETURN
	
END CATCH

INSERT TExecutedDataScript (ScriptGUID, Comments) VALUES (@ScriptGUID, @Comments)
	
COMMIT TRANSACTION

RETURN;



