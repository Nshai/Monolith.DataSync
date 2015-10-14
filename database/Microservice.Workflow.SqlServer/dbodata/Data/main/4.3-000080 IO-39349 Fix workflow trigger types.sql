USE workflow
GO
--select newid()
DECLARE @ScriptGUID UNIQUEIDENTIFIER
      , @Comments VARCHAR(255)
      , @ErrorMessage VARCHAR(MAX)
      , @ProductGroupGetNewQuoteName VARCHAR(255)
      , @StampAction CHAR(1)
	  ,	@StampDateTime DATETIME
	  ,	@StampUser INT


SELECT @ScriptGUID = 'D3C4B81B-69F5-4A8E-B20A-CDCA6669A0D1'
      , @Comments = '4.3-000080 IO-39349 Fix workflow trigger types'
      , @StampAction= 'U'
	  ,	@StampDateTime = getdate()
	  ,	@StampUser = 0
	  
 
IF EXISTS (SELECT 1 FROM TExecutedDataScript WHERE ScriptGUID = @ScriptGUID)
	RETURN; 


BEGIN TRANSACTION
	
	BEGIN TRY

	IF OBJECT_ID('tempdb..#TriggerSets') IS NOT NULL
		DROP TABLE #TriggerSets	


	IF OBJECT_ID('tempdb..#TriggerTypes') IS NOT NULL
		DROP TABLE #TriggerTypes

	SELECT
	'OnCreate' TriggerType,
	'Client' Datasource,
	'OnClientCreation' NewTriggerType
	INTO #TriggerTypes
	UNION ALL
	SELECT
	'OnDelete',
	'Client',
	'OnClientDeletion'
	UNION ALL
	SELECT
	'OnUpdate',
	'Client',
	'OnClientStatusUpdate'

	UNION ALL
	SELECT
	'OnCreate',
	'Lead',
	'OnLeadCreation'
	UNION ALL
	SELECT
	'OnDelete',
	'Lead',
	'OnLeadDeletion'
	UNION ALL
	SELECT
	'OnUpdate',
	'Lead',
	'OnLeadStatusUpdate'

	UNION ALL
	SELECT
	'OnCreate',
	'ServiceCase',
	'OnServiceCaseCreation'
	UNION ALL
	SELECT
	'OnDelete',
	'ServiceCase',
	'OnServiceCaseDeletion'
	UNION ALL
	SELECT
	'OnUpdate',
	'ServiceCase',
	'OnServiceCaseStatusUpdate'

	UNION ALL
	SELECT
	'OnCreate',
	'Plan',
	'OnPlanCreation'
	UNION ALL
	SELECT
	'OnUpdate',
	'Plan',
	'OnPlanStatusUpdate'

	UNION ALL
	SELECT
	'OnCreate',
	'Adviser',
	'None'
	UNION ALL
	SELECT
	'OnDelete',
	'Adviser',
	'None'
	UNION ALL
	SELECT
	'OnUpdate',
	'Adviser',
	'None'

	-- 
	SELECT 
	s.TemplateTriggerSetId TemplateTriggerSetId,
	s.TriggerType TriggerType,
	t.RelatedTo Datasource
	INTO #TriggerSets
	FROM workflow..TTemplateTriggerSet s
	JOIN workflow..TTemplateVersion v ON s.TemplateVersionId = v.TemplateVersionId
	JOIN workflow..TTemplate t ON t.TemplateId = v.TemplateId

	-- audit existing ..
	INSERT INTO workflow..TTemplateTriggerSetAudit( TemplateTriggerSetId, TriggerType, TemplateVersionId, TenantId, ConcurrencyId, EventSubscriptionId, TriggerList,
							StampAction, StampDateTime, StampUser) 

	SELECT s.TemplateTriggerSetId, s.TriggerType, TemplateVersionId, TenantId, ConcurrencyId, EventSubscriptionId, TriggerList,
							@StampAction, @StampDateTime, @StampUser
	FROM workflow..TTemplateTriggerSet s
	JOIN #TriggerSets ts ON ts.TemplateTriggerSetId = s.TemplateTriggerSetId
	JOIN #TriggerTypes tt ON tt.TriggerType = ts.TriggerType AND tt.Datasource = ts.Datasource


	-- update to new trigger types
	UPDATE s
	SET s.TriggerType = tt.NewTriggerType
	FROM workflow..TTemplateTriggerSet s
	JOIN #TriggerSets ts ON ts.TemplateTriggerSetId = s.TemplateTriggerSetId
	JOIN #TriggerTypes tt ON tt.TriggerType = ts.TriggerType AND tt.Datasource = ts.Datasource

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

--SELECT * FROM #TriggerSets
--SELECT * FROM #TriggerTypes
--SELECT * FROM workflow..TTemplateTriggerSet s