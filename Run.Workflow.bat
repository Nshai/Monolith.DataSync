TITLE Workflow

set microservice=Microservice.Workflow

nuget restore %microservice%.sln
msbuild %~dp0src\%microservice%\%microservice%.csproj /p:PostBuildEvent=

Call %~dp0src\%microservice%\bin\Debug\%microservice%.exe -dbprofile:dev