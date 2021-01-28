# Workflow

Provides for hosting and management of user-defined (and system) workflows.

## Installation

Build and run the Microservice.Workflow.exe
A shortcut has also been added to the root folder to enable easy starting of the host

For running on __test__ or __uat__ or __prod__ environments we need to following the next steps:

* Create database with name 'afper'
* Add all needed users (TEST\svc_tst_07_pswrkflow etc.)
* Run sql scripts from '%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\SQL\en' in the next order:
  * SqlWorkflowInstanceStoreSchema.sql
  * SqlWorkflowInstanceStoreLogic.sql
  * SqlWorkflowInstanceStoreSchemaUpgrade.sql. 

## Usage

TODO: Write usage instructions

## History

This project was a refactoring of the initial workflow project from Platform Services.  
The new version is completely decoupled and now hosts the workflows in process rather than having a dependency on IIS.

## Unit tests

```
C:\apps\opencover\OpenCover.Console.exe -returntargetcode -target:"C:\apps\nunit271\nunit-console.exe" -targetargs:"test\Microservice.Workflow.Tests\bin\Release\Microservice.Workflow.Tests.dll --framework=4.0 --nodots --nologo --noshadow /xml:dist\UnitTestResults.xml" -output:dist\OpenCoverResults.xml -filter:"-[*Test*]* -[*]*Host* -[*]*Modules* -[*]*Properties.Settings* +[*Microservice.Workflow*]*" -excludebyattribute:"*ExcludeFromCodeCoverage*" -excludebyfile:"*.Designer.cs;*Program.cs" -hideskipped:All -register:"c:\apps\OpenCover\x64\OpenCover.Profiler.dll"
```