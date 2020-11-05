FROM mcr.microsoft.com/dotnet/core/sdk:3.1
WORKDIR /src
COPY ["test/Microservice.Workflow.SubSystemTests/Microservice.Workflow.SubSystemTests.csproj", "test/Microservice.Workflow.SubSystemTests/"]
RUN dotnet restore "test/Microservice.Workflow.SubSystemTests/Microservice.Workflow.SubSystemTests.csproj" -s https://api.nuget.org/v3/index.json -s https://artifactory.intelliflo.io/artifactory/api/nuget/nuget-virtual
COPY ./test/Microservice.Workflow.SubSystemTests ./test/Microservice.Workflow.SubSystemTests
WORKDIR "/src/test/Microservice.Workflow.SubSystemTests"
RUN dotnet build "Microservice.Workflow.SubSystemTests.csproj" -c Debug
ENTRYPOINT ["dotnet", "test", "Microservice.Workflow.SubSystemTests.csproj", "-c", "Release"]
