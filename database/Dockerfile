FROM --platform=windows intelliflo/dotnet-framework-sdk
WORKDIR /src
COPY database/Microservice.Workflow.SqlServer/ .
ENTRYPOINT [ "msbuild", "Microservice.Workflow.SqlServer.sqlproj" ]