USE [workflow]
GO

DECLARE @ScriptGUID UNIQUEIDENTIFIER
      , @Comments VARCHAR(255)
      , @ErrorMessage VARCHAR(MAX)

--Use the line below to generate a GUID.
--Please DO NOT make it part of the script. You should only generate the GUID once.
--SELECT NEWID()

SELECT @ScriptGUID = 'E9B62B86-E44C-472C-9BC3-10086337FF25'
      , @Comments = '4.4-000060 IO-42821 Alter instance history serialization to use names instead of full type name'
      
IF EXISTS (SELECT 1 FROM TExecutedDataScript WHERE ScriptGUID = @ScriptGUID)
	RETURN; 

BEGIN TRANSACTION

	BEGIN TRY
		-- BEGIN DATA INSERT/UPDATE

		update WF_TInstanceHistory set Data = replace(Data, 'IntelliFlo.Platform.Workflow.InstanceCreatedLog, IntelliFlo.Services.Contracts', 'InstanceCreatedLog')
		update WF_TInstanceHistory set Data = replace(Data, 'IntelliFlo.Platform.Workflow.AbortRequestLog, IntelliFlo.Services.Contracts', 'AbortRequestLog')
		update WF_TInstanceHistory set Data = replace(Data, 'IntelliFlo.Platform.Workflow.CreateTaskLog, IntelliFlo.Services.Contracts', 'CreateTaskLog')
		update WF_TInstanceHistory set Data = replace(Data, 'IntelliFlo.Platform.Workflow.DelayLog, IntelliFlo.Services.Contracts', 'DelayLog')
		update WF_TInstanceHistory set Data = replace(Data, 'IntelliFlo.Platform.Workflow.ExceptionLog, IntelliFlo.Services.Contracts', 'ExceptionLog')
		update WF_TInstanceHistory set Data = replace(Data, '$type":', '$key":')

		update TTemplateVersion set Definition = replace(Definition, '$type":"IntelliFlo.IO.Core.Domain.Workflow', '$type":"IntelliFlo.Platform.Services.Workflow')
		update TTemplateVersion set Definition = replace(Definition, ', IntelliFlo.IO.Core"', ', IntelliFlo.Platform.Services.Workflow"')
		

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
