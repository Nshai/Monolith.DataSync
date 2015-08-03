USE [workflow]
GO

DECLARE @ScriptGUID UNIQUEIDENTIFIER
      , @Comments VARCHAR(255)
      , @ErrorMessage VARCHAR(MAX)

--Use the line below to generate a GUID.
--Please DO NOT make it part of the script. You should only generate the GUID once.
--SELECT NEWID()

SELECT @ScriptGUID = 'F936525C-704C-458B-9D07-6C67F33E0D47'
      , @Comments = '4.4-000127 IO-42821 Alter instance history serialization to use names instead of full type name'
      
IF EXISTS (SELECT 1 FROM TExecutedDataScript WHERE ScriptGUID = @ScriptGUID)
	RETURN; 

BEGIN TRANSACTION

	BEGIN TRY
		-- BEGIN DATA INSERT/UPDATE

		update tv set Definition = replace(Definition, '$type":"IntelliFlo.Platform.Services.Workflow.DelayStep, IntelliFlo.Platform.Services.Workflow"', '$key":"DelayStep"')
		OUTPUT deleted.TemplateVersionId, deleted.TemplateId, deleted.TenantId, deleted.Guid, deleted.Status, deleted.CreatedDate, deleted.OwnerUserId, deleted.ApplicableToGroupId, deleted.IncludeSubGroups, deleted.Definition, deleted.ConcurrencyId, deleted.Notes, deleted.TriggerDefinition, 'C', GETDATE(), 0
		INTO TTemplateVersionAudit (TemplateVersionId, TemplateId, TenantId, Guid, Status, CreatedDate, OwnerUserId, ApplicableToGroupId, IncludeSubGroups, Definition, ConcurrencyId, Notes, TriggerDefinition, StampAction, StampDateTime, StampUser)
		FROM TTemplateVersion tv 
		where tv.Definition like '%$type":"IntelliFlo.Platform.Services.Workflow.DelayStep, IntelliFlo.Platform.Services.Workflow"%'

		update tv set Definition = replace(Definition, '$type":"IntelliFlo.Platform.Services.Workflow.Domain.DelayStep, IntelliFlo.Platform.Services.Workflow"', '$key":"DelayStep"')
		OUTPUT deleted.TemplateVersionId, deleted.TemplateId, deleted.TenantId, deleted.Guid, deleted.Status, deleted.CreatedDate, deleted.OwnerUserId, deleted.ApplicableToGroupId, deleted.IncludeSubGroups, deleted.Definition, deleted.ConcurrencyId, deleted.Notes, deleted.TriggerDefinition, 'C', GETDATE(), 0
		INTO TTemplateVersionAudit (TemplateVersionId, TemplateId, TenantId, Guid, Status, CreatedDate, OwnerUserId, ApplicableToGroupId, IncludeSubGroups, Definition, ConcurrencyId, Notes, TriggerDefinition, StampAction, StampDateTime, StampUser)
		FROM TTemplateVersion tv 
		where tv.Definition like '%$type":"IntelliFlo.Platform.Services.Workflow.Domain.DelayStep, IntelliFlo.Platform.Services.Workflow"%'
		
		
		-- END DATA INSERT/UPDATE
	END TRY
	BEGIN CATCH
	
		SET @ErrorMessage = ERROR_MESSAGE()
		RAISERROR(@ErrorMessage, 16, 1)
		WHILE(@@TRANCOUNT > 0)ROLLBACK
		RETURN
	
	END CATCH

	INSERT TExecutedDataScript (ScriptGUID, Comments) VALUES (@ScriptGUID, @Comments)
	
COMMIT TRANSACTION

-- Check for ANY open transactions
-- This applies not only to THIS script but will rollback any open transactions in any scripts that have been run before this one.
IF @@TRANCOUNT > 0
BEGIN
       ROLLBACK
       RETURN
       PRINT 'Open transaction found, aborting'
END

RETURN;
