FROM intelliflo/dotnet-framework-sdk

ENV PIPELINE_S3_BUCKET=sub-ie-01-test-results.sub.intelliflo.systems

WORKDIR /tools
# Install netcore-sdk
RUN powershell Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
RUN choco install dotnetcore-sdk --version=3.1.404 -y
RUN choco install awscli -y

WORKDIR /results
WORKDIR /test
COPY ["test/Microservice.Workflow.SubSystemTests/Microservice.Workflow.SubSystemTests.csproj", "test/"]
RUN dotnet restore "test/Microservice.Workflow.SubSystemTests.csproj" -s https://api.nuget.org/v3/index.json -s https://artifactory.intelliflo.io/artifactory/api/nuget/nuget-virtual
COPY ./test/Microservice.Workflow.SubSystemTests/ .
RUN dotnet build "Microservice.Workflow.SubSystemTests.csproj" --nologo -c Release

COPY ./.docker/subsys/entrypoint.ps1 .
CMD powershell .\entrypoint.ps1
