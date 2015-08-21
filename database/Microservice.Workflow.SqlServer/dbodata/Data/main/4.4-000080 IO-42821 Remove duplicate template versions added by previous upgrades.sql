USE [workflow]
GO

DECLARE @ScriptGUID UNIQUEIDENTIFIER
      , @Comments VARCHAR(255)
      , @ErrorMessage VARCHAR(MAX)

--Use the line below to generate a GUID.
--Please DO NOT make it part of the script. You should only generate the GUID once.
--SELECT NEWID()

SELECT @ScriptGUID = '0A98733A-5FDA-40B6-AD7A-F310FF17F87E'
      , @Comments = '4.4-000080 IO-42821 Remove duplicate template versions added by previous upgrades'
      
IF EXISTS (SELECT 1 FROM TExecutedDataScript WHERE ScriptGUID = @ScriptGUID)
	RETURN; 

BEGIN TRANSACTION

	BEGIN TRY
		-- BEGIN DATA INSERT/UPDATE

		;WITH cte AS (
		  SELECT TemplateVersionId, TenantId, TemplateId, Guid, Status, CreatedDate, OwnerUserId, ApplicableToGroupId, IncludeSubGroups, Definition, ConcurrencyId, Notes, TriggerDefinition, row_number() OVER(PARTITION BY TemplateId ORDER BY CreatedDate desc) AS [rn]
		  FROM workflow..TTemplateVersion
		)
		delete cte 
		output deleted.TemplateVersionId, deleted.TemplateId, deleted.TenantId, deleted.Guid, deleted.Status, deleted.CreatedDate, deleted.OwnerUserId, deleted.ApplicableToGroupId, deleted.IncludeSubGroups, deleted.Definition, deleted.ConcurrencyId, deleted.Notes, deleted.TriggerDefinition, 'D', getdate(), 0
		into TTemplateVersionAudit (TemplateVersionId, TemplateId, TenantId, Guid, Status, CreatedDate, OwnerUserId, ApplicableToGroupId, IncludeSubGroups, Definition, ConcurrencyId, Notes, TriggerDefinition, StampAction, StampDateTime, StampUser)
		where rn > 1

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




