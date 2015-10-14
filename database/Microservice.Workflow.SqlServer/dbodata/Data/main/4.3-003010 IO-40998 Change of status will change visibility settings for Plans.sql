USE [workflow]
GO

DECLARE @ScriptGUID UNIQUEIDENTIFIER
      , @Comments VARCHAR(255)
      , @ErrorMessage VARCHAR(MAX)

--Use the line below to generate a GUID.
--Please DO NOT make it part of the script. You should only generate the GUID once.
--SELECT NEWID()

SELECT @ScriptGUID = 'C59F1644-8399-4B78-AF3D-E559EEAE7E01'
      , @Comments = '4.3-003010 IO-40998 Change of status will change visibility settings for Plans'
      
IF EXISTS (SELECT 1 FROM TExecutedDataScript WHERE ScriptGUID = @ScriptGUID)
	RETURN; 

BEGIN TRANSACTION

	BEGIN TRY
		-- BEGIN DATA INSERT/UPDATE

		insert WF_TTemplateDefinition(Id, Name, TenantId, Definition, DateUtc)
		select 'dafb179f-fc00-4e82-984f-8dc0d7304705', 'Update Plan Visibility', 0, '<WorkflowService xmlns="http://schemas.microsoft.com/netfx/2009/xaml/servicemodel" xmlns:ipwv="clr-namespace:IntelliFlo.Platform.Workflow.v3;assembly=IntelliFlo.Services.Contracts" xmlns:ipwva="clr-namespace:IntelliFlo.Platform.Workflow.v3.Activities;assembly=IntelliFlo.Services.IO" xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" xmlns:p="http://schemas.microsoft.com/netfx/2009/xaml/activities" xmlns:s="clr-namespace:System;assembly=mscorlib" xmlns:sads="http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger" xmlns:sap="http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation" xmlns:sap2010="http://schemas.microsoft.com/netfx/2010/xaml/activities/presentation" xmlns:scg="clr-namespace:System.Collections.Generic;assembly=mscorlib" xmlns:sco="clr-namespace:System.Collections.ObjectModel;assembly=mscorlib" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" mc:Ignorable="sap sap2010 sads" ConfigurationName="DynamicWorkflow" Name="UpdatePlanVisibility"><p:Sequence sap2010:WorkflowViewState.IdRef="Sequence_1"><p:TextExpression.Namespaces><sco:Collection x:TypeArguments="x:String"><x:String>System</x:String><x:String>System.Collections.Generic</x:String><x:String>System.Data</x:String><x:String>System.Linq</x:String><x:String>System.Text</x:String><x:String>Microsoft.Activities</x:String></sco:Collection></p:TextExpression.Namespaces><p:TextExpression.References><sco:Collection x:TypeArguments="p:AssemblyReference"><p:AssemblyReference>Microsoft.Activities</p:AssemblyReference><p:AssemblyReference>Microsoft.Activities.Design</p:AssemblyReference><p:AssemblyReference>Newtonsoft.Json</p:AssemblyReference><p:AssemblyReference>PresentationCore</p:AssemblyReference><p:AssemblyReference>PresentationFramework</p:AssemblyReference><p:AssemblyReference>System</p:AssemblyReference><p:AssemblyReference>System.Activities</p:AssemblyReference><p:AssemblyReference>System.Activities.Presentation</p:AssemblyReference><p:AssemblyReference>System.ComponentModel.DataAnnotations</p:AssemblyReference><p:AssemblyReference>System.Configuration</p:AssemblyReference><p:AssemblyReference>System.Core</p:AssemblyReference><p:AssemblyReference>System.Drawing</p:AssemblyReference><p:AssemblyReference>System.ServiceModel</p:AssemblyReference><p:AssemblyReference>System.ServiceModel.Activities</p:AssemblyReference><p:AssemblyReference>System.ServiceModel.Channels</p:AssemblyReference><p:AssemblyReference>System.Web</p:AssemblyReference><p:AssemblyReference>System.Workflow.ComponentModel</p:AssemblyReference><p:AssemblyReference>System.Xaml</p:AssemblyReference><p:AssemblyReference>System.Xml.Linq</p:AssemblyReference><p:AssemblyReference>System.Data.DataSetExtensions</p:AssemblyReference><p:AssemblyReference>Microsoft.CSharp</p:AssemblyReference><p:AssemblyReference>System.Data</p:AssemblyReference><p:AssemblyReference>System.Xml</p:AssemblyReference><p:AssemblyReference>WindowsBase</p:AssemblyReference><p:AssemblyReference>IntelliFlo.Commons</p:AssemblyReference><p:AssemblyReference>mscorlib</p:AssemblyReference><p:AssemblyReference>IntelliFlo.Services.IO</p:AssemblyReference><p:AssemblyReference>IntelliFlo.Services.Client</p:AssemblyReference><p:AssemblyReference>IntelliFlo.Services.Contracts</p:AssemblyReference></sco:Collection></p:TextExpression.References><p:Sequence.Variables><x:Reference>__ReferenceID0</x:Reference><x:Reference>__ReferenceID1</x:Reference></p:Sequence.Variables><ipwva:Create sap2010:ExpressionActivityEditor.ExpressionActivityEditor="C#" TemplateId="dafb179f-fc00-4e82-984f-8dc0d7304705"><ipwva:Create.Context><p:OutArgument x:TypeArguments="ipwv:WorkflowContext"><p:VariableReference x:TypeArguments="ipwv:WorkflowContext"><p:VariableReference.Variable><p:Variable x:TypeArguments="ipwv:WorkflowContext" x:Name="__ReferenceID0" Name="context" /></p:VariableReference.Variable></p:VariableReference></p:OutArgument></ipwva:Create.Context><ipwva:Create.InstanceId><p:OutArgument x:TypeArguments="s:Guid"><p:VariableReference x:TypeArguments="s:Guid"><p:VariableReference.Variable><p:Variable x:TypeArguments="s:Guid" x:Name="__ReferenceID1" Name="instanceId" /></p:VariableReference.Variable></p:VariableReference></p:OutArgument></ipwva:Create.InstanceId><ipwva:Create.TemplateType><p:InArgument x:TypeArguments="s:Nullable(ipwv:EntityType)"><p:Literal x:TypeArguments="s:Nullable(ipwv:EntityType)" Value="Plan" /></p:InArgument></ipwva:Create.TemplateType></ipwva:Create><ipwva:WorkflowStep Step="{x:Null}" StepDetail="{x:Null}" StepId="{x:Null}" EnableLogging="False"><ipwva:WorkflowStep.Context><p:InArgument x:TypeArguments="ipwv:WorkflowContext"><p:VariableValue x:TypeArguments="ipwv:WorkflowContext" Variable="{x:Reference __ReferenceID0}" /></p:InArgument></ipwva:WorkflowStep.Context><ipwva:UpdatePlanVisibility /></ipwva:WorkflowStep></p:Sequence></WorkflowService>', getutcdate()

		
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
