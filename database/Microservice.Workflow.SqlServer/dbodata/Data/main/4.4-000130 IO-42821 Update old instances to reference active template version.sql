USE [workflow]
GO

DECLARE @ScriptGUID UNIQUEIDENTIFIER
      , @Comments VARCHAR(255)
      , @ErrorMessage VARCHAR(MAX)

--Use the line below to generate a GUID.
--Please DO NOT make it part of the script. You should only generate the GUID once.
--SELECT NEWID()

SELECT @ScriptGUID = 'BE1ACB85-4BBC-4A46-8B7E-3DD6F125F4CE'
      , @Comments = '4.4-000130 IO-42821 Update old instances to reference active template version'
      
IF EXISTS (SELECT 1 FROM TExecutedDataScript WHERE ScriptGUID = @ScriptGUID)
	RETURN; 

BEGIN TRANSACTION

	BEGIN TRY
		-- BEGIN DATA INSERT/UPDATE
		update i
		set TemplateId = resolvedTemplate.[Guid], UniqueId = newid()
		output deleted.Id, deleted.TemplateId, deleted.CorrelationId, deleted.UserId, deleted.TenantId, deleted.EntityId, deleted.EntityType, deleted.RelatedEntityId, deleted.ParentEntityType, deleted.ParentEntityId, deleted.CreatedUtc, deleted.[Status], deleted.UniqueId, 'EAF71BC3-8279-49D1-8CA8-E71CA80E3F3F'
		into WF_Migration_TInstance(Id, TemplateId, CorrelationId, UserId, TenantId, EntityId, EntityType, RelatedEntityId, ParentEntityType, ParentEntityId, CreatedUtc, Status, UniqueId, NewInstanceId)
		from 
		WF_TInstance i
		inner join WF_TTemplateDefinition d on i.TemplateId = d.Id
		inner join TTemplateVersionAudit a on d.Id = a.[Guid] and a.StampAction = 'D'
		inner join TTemplateVersion resolvedTemplate on a.TemplateId = resolvedTemplate.TemplateId
		where d.Version = 0 and d.Id != resolvedTemplate.[Guid] and i.Status = 'In Progress'
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
