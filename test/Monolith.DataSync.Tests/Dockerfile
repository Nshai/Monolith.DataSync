FROM intelliflo/dotnet-framework-sdk
WORKDIR /src
COPY ./src/Monolith.DataSync/Monolith.DataSync.csproj ./Monolith.DataSync/
RUN nuget sources Add -Name 'Ifl Artifactory (virtual)' -Source 'https://artifactory.intelliflo.io/artifactory/api/nuget/nuget-virtual'
WORKDIR /test
COPY test/Monolith.DataSync.Tests/Monolith.DataSync.Tests.csproj ./Monolith.DataSync.Tests/
RUN dotnet restore "Monolith.DataSync.Tests/Monolith.DataSync.Tests.csproj" -s https://api.nuget.org/v3/index.json -s https://artifactory.intelliflo.io/artifactory/api/nuget/nuget-virtual
COPY ./test/Monolith.DataSync.Tests/ ./Monolith.DataSync.Tests/
COPY ./src/ /src/
WORKDIR /src/Monolith.DataSync
RUN nuget restore "Monolith.DataSync.csproj"
WORKDIR /test/Monolith.DataSync.Tests
RUN msbuild "Monolith.DataSync.Tests.csproj" --nologo -c Debug
ENTRYPOINT ["mstest", "Monolith.DataSync.Tests.csproj"]
