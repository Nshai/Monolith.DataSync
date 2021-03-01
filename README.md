# Microservice.Workflow

Provides for hosting and management of user-defined (and system) workflows. Based on Windows Workflow Foundation.

## Database management instructions

Microservice.Workflow uses two databases:

- `workflow`: used to coorelate tenants and workflows created by them (managed by us)
- `afper`: Windows Workflow Foundation internal database used to persist workflow states (managed by .NET Framework scripts)

### workflow database

Schema and data scripts are stored in `./database/` folder and deployed usion RedGate Minion (included in Intelliflo.Platform library)

### afper database

Schema scripts can be located in this folder `%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\SQL\en`. In App.config `SqlAfper` section contains references to all schema creation and update SQL scripts.

To simply database creation and schema update operation, there are two `appSettings` settings that can be leveraged:

- `CreateAfperDatabase`: creates database and deploys schema
- `UpgradeAfperDatabase`: updates database schema

**NOTE** to avoid potential data loss `CreatePersistenceDataStoreTask.cs` have a safety check. If database contains any tables, the creation script will be skipped.

## History

This project was a refactoring of the initial workflow project from Platform Services.  
The new version is completely decoupled and now hosts the workflows in process rather than having a dependency on IIS.

## Unit tests

```
C:\apps\opencover\OpenCover.Console.exe -returntargetcode -target:"C:\apps\nunit271\nunit-console.exe" -targetargs:"test\Microservice.Workflow.Tests\bin\Release\Microservice.Workflow.Tests.dll --framework=4.0 --nodots --nologo --noshadow /xml:dist\UnitTestResults.xml" -output:dist\OpenCoverResults.xml -filter:"-[*Test*]* -[*]*Host* -[*]*Modules* -[*]*Properties.Settings* +[*Microservice.Workflow*]*" -excludebyattribute:"*ExcludeFromCodeCoverage*" -excludebyfile:"*.Designer.cs;*Program.cs" -hideskipped:All -register:"c:\apps\OpenCover\x64\OpenCover.Profiler.dll"
```