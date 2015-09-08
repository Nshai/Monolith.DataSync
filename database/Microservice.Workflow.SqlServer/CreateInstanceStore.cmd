@echo off

:createStore
SET INSTANCESTORE=afper
if not .%1 == . SET INSTANCESTORE=%1
Echo Dropping instance store %INSTANCESTORE%
sqlcmd.exe -S "." -Q "drop database %INSTANCESTORE%"
sqlcmd.exe -S "." -Q "create database %INSTANCESTORE%"
Echo Creating instance store %INSTANCESTORE%
sqlcmd.exe -S "." -d %INSTANCESTORE% -i "%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\SQL\en\SqlWorkflowInstanceStoreSchema.sql"
sqlcmd.exe -S "." -d %INSTANCESTORE% -i "%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\SQL\en\SqlWorkflowInstanceStoreLogic.sql"
echo Workflow Persistence store %INSTANCESTORE% successfully created
:end

