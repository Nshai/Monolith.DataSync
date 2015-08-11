TITLE Workflow

msbuild %~dp0src\IntelliFlo.Platform.Services.Workflow\IntelliFlo.Platform.Services.Workflow.csproj /p:PostBuildEvent=

Call %~dp0src\IntelliFlo.Platform.Services.Workflow\bin\Debug\Microservice.Workflow.exe