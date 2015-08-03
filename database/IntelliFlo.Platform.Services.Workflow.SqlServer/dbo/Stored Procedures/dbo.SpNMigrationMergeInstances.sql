create procedure [dbo].[SpNMigrationMergeInstances]
@InstanceId uniqueidentifier,
@NewInstanceId uniqueidentifier
AS
declare @ErrorMessage VARCHAR(MAX)

BEGIN TRANSACTION
BEGIN TRY
	insert WF_Migration_TInstanceHistory (Id, InstanceId, StepId, Step, Data, IsComplete, TimestampUtc)
	select i.Id, i.InstanceId, i.StepId, i.Step, i.Data, i.IsComplete, i.TimestampUtc
	from WF_TInstanceHistory i where InstanceId = @InstanceId
		
	insert WF_Migration_TInstance (Id, TemplateId, CorrelationId, UserId, TenantId, EntityId, EntityType, RelatedEntityId, ParentEntityType, ParentEntityId, CreatedUtc, Status, UniqueId, NewInstanceId)
	select i.Id, i.TemplateId, i.CorrelationId, i.UserId, i.TenantId, i.EntityId, i.EntityType, i.RelatedEntityId, i.ParentEntityType, i.ParentEntityId, i.CreatedUtc, i.Status, i.UniqueId, @NewInstanceId
	from WF_TInstance i where Id = @InstanceId

	update WF_TInstanceHistory set InstanceId = @NewInstanceId where InstanceId = @InstanceId

	update WF_TInstance set CreatedUtc = (select CreatedUtc from WF_TInstance where Id = @InstanceId) where Id = @NewInstanceId

	delete WF_TInstance where Id = @InstanceId

END TRY
	
BEGIN CATCH

	SET @ErrorMessage = ERROR_MESSAGE()
	RAISERROR(@ErrorMessage, 16, 1)
	WHILE(@@TRANCOUNT > 0)ROLLBACK
	RETURN

END CATCH

COMMIT TRANSACTION

GO