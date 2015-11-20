USE [workflow]
GO

DECLARE @ScriptGUID UNIQUEIDENTIFIER
      , @Comments VARCHAR(255)
      , @ErrorMessage VARCHAR(MAX)

--Use the line below to generate a GUID.
--Please DO NOT make it part of the script. You should only generate the GUID once.
--SELECT NEWID()

SELECT @ScriptGUID = '7A139DB0-1A7D-4CB0-A958-E1E6C58A854A'
      , @Comments = '4.5-000010 IO-46251 Author system workflows'
      
IF EXISTS (SELECT 1 FROM TExecutedDataScript WHERE ScriptGUID = @ScriptGUID)
	RETURN; 

BEGIN TRANSACTION

	BEGIN TRY
		-- BEGIN DATA INSERT/UPDATE
		if not exists(select 1 from WF_TTemplateDefinition where Id = 'bad13106-1e2d-4f8a-bcbe-27bcfdead878')
		begin
			insert workflow..WF_TTemplateDefinition(Id, Name, TenantId, Definition, DateUtc, Version)
			values ('bad13106-1e2d-4f8a-bcbe-27bcfdead878', 'Document Review', 0, '<WorkflowService mc:Ignorable="sap sap2010 sads" ConfigurationName="DynamicWorkflow" sap2010:WorkflowViewState.IdRef="WorkflowService_1" Name="DocumentReview"
			 xmlns="http://schemas.microsoft.com/netfx/2009/xaml/servicemodel"
			 xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
			 xmlns:mwv="clr-namespace:Microservice.Workflow.v1;assembly=Microservice.Workflow"
			 xmlns:mwva="clr-namespace:Microservice.Workflow.v1.Activities;assembly=Microservice.Workflow"
			 xmlns:p="http://schemas.microsoft.com/netfx/2009/xaml/activities"
			 xmlns:s="clr-namespace:System;assembly=mscorlib"
			 xmlns:sads="http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger"
			 xmlns:sap="http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"
			 xmlns:sap2010="http://schemas.microsoft.com/netfx/2010/xaml/activities/presentation"
			 xmlns:scg="clr-namespace:System.Collections.Generic;assembly=mscorlib"
			 xmlns:sco="clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"
			 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
			  <p:Sequence sap2010:WorkflowViewState.IdRef="Sequence_1">
				<p:TextExpression.Namespaces>
				  <sco:Collection x:TypeArguments="x:String">
					<x:String>System</x:String>
					<x:String>System.Collections.Generic</x:String>
					<x:String>System.Data</x:String>
					<x:String>System.Linq</x:String>
					<x:String>System.Text</x:String>
					<x:String>Microsoft.Activities</x:String>
				  </sco:Collection>
				</p:TextExpression.Namespaces>
				<p:TextExpression.References>
				  <sco:Collection x:TypeArguments="p:AssemblyReference">
					<p:AssemblyReference>Microsoft.Activities</p:AssemblyReference>
					<p:AssemblyReference>Microsoft.Activities.Design</p:AssemblyReference>
					<p:AssemblyReference>Newtonsoft.Json</p:AssemblyReference>
					<p:AssemblyReference>PresentationCore</p:AssemblyReference>
					<p:AssemblyReference>PresentationFramework</p:AssemblyReference>
					<p:AssemblyReference>System</p:AssemblyReference>
					<p:AssemblyReference>System.Activities</p:AssemblyReference>
					<p:AssemblyReference>System.Activities.Presentation</p:AssemblyReference>
					<p:AssemblyReference>System.ComponentModel.DataAnnotations</p:AssemblyReference>
					<p:AssemblyReference>System.Configuration</p:AssemblyReference>
					<p:AssemblyReference>System.Core</p:AssemblyReference>
					<p:AssemblyReference>System.Drawing</p:AssemblyReference>
					<p:AssemblyReference>System.ServiceModel</p:AssemblyReference>
					<p:AssemblyReference>System.ServiceModel.Activities</p:AssemblyReference>
					<p:AssemblyReference>System.ServiceModel.Channels</p:AssemblyReference>
					<p:AssemblyReference>System.Web</p:AssemblyReference>
					<p:AssemblyReference>System.Workflow.ComponentModel</p:AssemblyReference>
					<p:AssemblyReference>System.Xaml</p:AssemblyReference>
					<p:AssemblyReference>System.Xml.Linq</p:AssemblyReference>
					<p:AssemblyReference>System.Data.DataSetExtensions</p:AssemblyReference>
					<p:AssemblyReference>Microsoft.CSharp</p:AssemblyReference>
					<p:AssemblyReference>System.Data</p:AssemblyReference>
					<p:AssemblyReference>System.Xml</p:AssemblyReference>
					<p:AssemblyReference>WindowsBase</p:AssemblyReference>
					<p:AssemblyReference>IntelliFlo.Commons</p:AssemblyReference>
					<p:AssemblyReference>mscorlib</p:AssemblyReference>
					<p:AssemblyReference>Microservice.Workflow</p:AssemblyReference>
				  </sco:Collection>
				</p:TextExpression.References>
				<p:Sequence.Variables>
				  <x:Reference>__ReferenceID0</x:Reference>
				  <x:Reference>__ReferenceID1</x:Reference>
				</p:Sequence.Variables>
				<mwva:Create TemplateId="bad13106-1e2d-4f8a-bcbe-27bcfdead878" TemplateType="Author">
				  <mwva:Create.Context>
					<p:OutArgument x:TypeArguments="mwv:WorkflowContext">
					  <p:VariableReference x:TypeArguments="mwv:WorkflowContext">
						<p:VariableReference.Variable>
						  <p:Variable x:TypeArguments="mwv:WorkflowContext" x:Name="__ReferenceID0" Name="context" />
						</p:VariableReference.Variable>
					  </p:VariableReference>
					</p:OutArgument>
				  </mwva:Create.Context>
				  <mwva:Create.InstanceId>
					<p:OutArgument x:TypeArguments="s:Guid">
					  <p:VariableReference x:TypeArguments="s:Guid">
						<p:VariableReference.Variable>
						  <p:Variable x:TypeArguments="s:Guid" x:Name="__ReferenceID1" Name="instanceId" />
						</p:VariableReference.Variable>
					  </p:VariableReference>
					</p:OutArgument>
				  </mwva:Create.InstanceId>
				</mwva:Create>
				<mwva:WorkflowStep Step="{x:Null}" StepDetail="{x:Null}" StepId="{x:Null}" StepIndex="{x:Null}" EnableLogging="False" >
				  <mwva:WorkflowStep.Context>
					<p:InArgument x:TypeArguments="mwv:WorkflowContext">
					  <p:VariableValue x:TypeArguments="mwv:WorkflowContext" Variable="{x:Reference __ReferenceID0}" />
					</p:InArgument>
				  </mwva:WorkflowStep.Context>
				  <mwva:PostToService Body="{}{ ''OutputId'': {EntityId} }" sap2010:WorkflowViewState.IdRef="PostToService_1" ServiceName="author" Uri="/v1/reviews" />
				</mwva:WorkflowStep>
			  </p:Sequence>
			</WorkflowService>', getdate(), 1)

			insert TTemplateRegistration(TenantId, Identifier, TemplateId, ConcurrencyId)
			values (0, 'documentreview', 'bad13106-1e2d-4f8a-bcbe-27bcfdead878', 1)

		end

		if not exists(select 1 from WF_TTemplateDefinition where Id = '6e2b7947-adab-49b5-b45a-271cf15540e0')
		begin
			insert workflow..WF_TTemplateDefinition(Id, Name, TenantId, Definition, DateUtc, Version)
			values ('6e2b7947-adab-49b5-b45a-271cf15540e0', 'Document Save To Entity', 0, '<WorkflowService mc:Ignorable="sap sap2010 sads" ConfigurationName="DynamicWorkflow" sap2010:WorkflowViewState.IdRef="WorkflowService_1" Name="DocumentSaveToEntity"
			 xmlns="http://schemas.microsoft.com/netfx/2009/xaml/servicemodel"
			 xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
			 xmlns:mwv="clr-namespace:Microservice.Workflow.v1;assembly=Microservice.Workflow"
			 xmlns:mwva="clr-namespace:Microservice.Workflow.v1.Activities;assembly=Microservice.Workflow"
			 xmlns:p="http://schemas.microsoft.com/netfx/2009/xaml/activities"
			 xmlns:s="clr-namespace:System;assembly=mscorlib"
			 xmlns:sads="http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger"
			 xmlns:sap="http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"
			 xmlns:sap2010="http://schemas.microsoft.com/netfx/2010/xaml/activities/presentation"
			 xmlns:scg="clr-namespace:System.Collections.Generic;assembly=mscorlib"
			 xmlns:sco="clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"
			 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
			  <p:Sequence sap2010:WorkflowViewState.IdRef="Sequence_1">
				<p:TextExpression.Namespaces>
				  <sco:Collection x:TypeArguments="x:String">
					<x:String>System</x:String>
					<x:String>System.Collections.Generic</x:String>
					<x:String>System.Data</x:String>
					<x:String>System.Linq</x:String>
					<x:String>System.Text</x:String>
					<x:String>Microsoft.Activities</x:String>
				  </sco:Collection>
				</p:TextExpression.Namespaces>
				<p:TextExpression.References>
				  <sco:Collection x:TypeArguments="p:AssemblyReference">
					<p:AssemblyReference>Microsoft.Activities</p:AssemblyReference>
					<p:AssemblyReference>Microsoft.Activities.Design</p:AssemblyReference>
					<p:AssemblyReference>Newtonsoft.Json</p:AssemblyReference>
					<p:AssemblyReference>PresentationCore</p:AssemblyReference>
					<p:AssemblyReference>PresentationFramework</p:AssemblyReference>
					<p:AssemblyReference>System</p:AssemblyReference>
					<p:AssemblyReference>System.Activities</p:AssemblyReference>
					<p:AssemblyReference>System.Activities.Presentation</p:AssemblyReference>
					<p:AssemblyReference>System.ComponentModel.DataAnnotations</p:AssemblyReference>
					<p:AssemblyReference>System.Configuration</p:AssemblyReference>
					<p:AssemblyReference>System.Core</p:AssemblyReference>
					<p:AssemblyReference>System.Drawing</p:AssemblyReference>
					<p:AssemblyReference>System.ServiceModel</p:AssemblyReference>
					<p:AssemblyReference>System.ServiceModel.Activities</p:AssemblyReference>
					<p:AssemblyReference>System.ServiceModel.Channels</p:AssemblyReference>
					<p:AssemblyReference>System.Web</p:AssemblyReference>
					<p:AssemblyReference>System.Workflow.ComponentModel</p:AssemblyReference>
					<p:AssemblyReference>System.Xaml</p:AssemblyReference>
					<p:AssemblyReference>System.Xml.Linq</p:AssemblyReference>
					<p:AssemblyReference>System.Data.DataSetExtensions</p:AssemblyReference>
					<p:AssemblyReference>Microsoft.CSharp</p:AssemblyReference>
					<p:AssemblyReference>System.Data</p:AssemblyReference>
					<p:AssemblyReference>System.Xml</p:AssemblyReference>
					<p:AssemblyReference>WindowsBase</p:AssemblyReference>
					<p:AssemblyReference>IntelliFlo.Commons</p:AssemblyReference>
					<p:AssemblyReference>mscorlib</p:AssemblyReference>
					<p:AssemblyReference>Microservice.Workflow</p:AssemblyReference>
				  </sco:Collection>
				</p:TextExpression.References>
				<p:Sequence.Variables>
				  <x:Reference>__ReferenceID0</x:Reference>
				  <x:Reference>__ReferenceID1</x:Reference>
				</p:Sequence.Variables>
				<mwva:Create TemplateId="6e2b7947-adab-49b5-b45a-271cf15540e0" TemplateType="Author">
				  <mwva:Create.Context>
					<p:OutArgument x:TypeArguments="mwv:WorkflowContext">
					  <p:VariableReference x:TypeArguments="mwv:WorkflowContext">
						<p:VariableReference.Variable>
						  <p:Variable x:TypeArguments="mwv:WorkflowContext" x:Name="__ReferenceID0" Name="context" />
						</p:VariableReference.Variable>
					  </p:VariableReference>
					</p:OutArgument>
				  </mwva:Create.Context>
				  <mwva:Create.InstanceId>
					<p:OutArgument x:TypeArguments="s:Guid">
					  <p:VariableReference x:TypeArguments="s:Guid">
						<p:VariableReference.Variable>
						  <p:Variable x:TypeArguments="s:Guid" x:Name="__ReferenceID1" Name="instanceId" />
						</p:VariableReference.Variable>
					  </p:VariableReference>
					</p:OutArgument>
				  </mwva:Create.InstanceId>
				</mwva:Create>
				<mwva:WorkflowStep Step="{x:Null}" StepDetail="{x:Null}" StepId="{x:Null}" StepIndex="{x:Null}" EnableLogging="False" >
				  <mwva:WorkflowStep.Context>
					<p:InArgument x:TypeArguments="mwv:WorkflowContext">
					  <p:VariableValue x:TypeArguments="mwv:WorkflowContext" Variable="{x:Reference __ReferenceID0}" />
					</p:InArgument>
				  </mwva:WorkflowStep.Context>
				  <mwva:PostToService Body="{x:Null}" sap2010:WorkflowViewState.IdRef="PostToService_1" ServiceName="nio" Uri="[&quot;author/savetoclient?outputId=&quot; + context.EntityId.ToString()]" />
				</mwva:WorkflowStep>
			  </p:Sequence>
			</WorkflowService>', getdate(), 1)

			insert TTemplateRegistration(TenantId, Identifier, TemplateId, ConcurrencyId)
			values (0, 'documentsavetoentity', '6e2b7947-adab-49b5-b45a-271cf15540e0', 1)

		end

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