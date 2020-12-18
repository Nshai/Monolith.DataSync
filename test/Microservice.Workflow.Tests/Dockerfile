FROM intelliflo/dotnet-framework-sdk
WORKDIR /src
COPY ./src/Microservice.Workflow/Microservice.Workflow.csproj ./Microservice.Workflow/
RUN nuget sources Add -Name 'Ifl Artifactory (virtual)' -Source 'https://artifactory.intelliflo.io/artifactory/api/nuget/nuget-virtual'
WORKDIR /test
COPY test/Microservice.Workflow.Tests/Microservice.Workflow.Tests.csproj ./Microservice.Workflow.Tests/
RUN dotnet restore "Microservice.Workflow.Tests/Microservice.Workflow.Tests.csproj" -s https://api.nuget.org/v3/index.json -s https://artifactory.intelliflo.io/artifactory/api/nuget/nuget-virtual
COPY ./test/Microservice.Workflow.Tests/ ./Microservice.Workflow.Tests/
COPY ./src/ /src/
WORKDIR /src/Microservice.Workflow
RUN nuget restore "Microservice.Workflow.csproj"
WORKDIR /test/Microservice.Workflow.Tests
RUN msbuild "Microservice.Workflow.Tests.csproj" --nologo -c Debug
ENTRYPOINT ["mstest", "Microservice.Workflow.Tests.csproj"]
