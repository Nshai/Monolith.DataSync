TITLE Workflow

msbuild %~dp0src\Microservice.Workflow\Microservice.Workflow.csproj /p:PostBuildEvent=

Call %~dp0src\Microservice.Workflow\bin\Debug\Microservice.Workflow.exe