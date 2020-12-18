# escape=`

FROM mcr.microsoft.com/dotnet/framework/runtime:4.7.2-windowsservercore-ltsc2019 AS base
# install additional requirement: SqlCmd
WORKDIR /tools
RUN curl.exe -L https://aka.ms/vs/15/release/vc_redist.x64.exe -o vc_redist.x64.exe
RUN curl.exe -L https://go.microsoft.com/fwlink/?linkid=2120137 -o msodbcsql.msi
RUN curl.exe -L https://go.microsoft.com/fwlink/?linkid=2082790 -o sqlcmd.msi
RUN .\vc_redist.x64.exe /install /passive /norestart
RUN msiexec.exe /i msodbcsql.msi /qn /L*V msodbcsql.log IACCEPTMSODBCSQLLICENSETERMS=YES
RUN msiexec.exe /i sqlcmd.msi /qn /L*V sqlcmd.log IACCEPTMSSQLCMDLNUTILSLICENSETERMS=YES
RUN set PATH=%PATH%;C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\
EXPOSE 80

FROM mcr.microsoft.com/dotnet/framework/sdk:4.7.2-windowsservercore-ltsc2019 AS tools
ARG CONSUL_TMPL_VERSION=0.25.1
WORKDIR /tools
WORKDIR /temp
RUN curl.exe https://releases.hashicorp.com/consul-template/$($Env:CONSUL_TMPL_VERSION)/consul-template_$($Env:CONSUL_TMPL_VERSION)_windows_amd64.zip -L -o consul-template.zip -s; `
    tar -xf consul-template.zip -C /tools
RUN curl.exe https://www.nuget.org/api/v2/package/Microsoft.Web.Xdt/2.1.1 -L -o xdt.zip -s; `
    tar -xf xdt.zip; `
    cp ./lib/net40/Microsoft.Web.XmlTransform.dll /tools
RUN curl.exe https://www.nuget.org/api/v2/package/SlowCheetah.Xdt/1.1.7-beta -L -o slowcheetah.zip -s; `
    tar -xf slowcheetah.zip; `
    cp ./lib/SlowCheetah.Xdt.dll /tools; `
    cp ./tools/SlowCheetah.Xdt.exe /tools
COPY ./.docker/windows/wincore /tools

FROM intelliflo/dotnet-framework-sdk AS build
ARG APPLICATION_VERSION=0.0.0.1
ARG GIT_HASH=0000000000000000000000000000000000000000
RUN cmd /S /C echo "setting application version '%APPLICATION_VERSION%' and associating git commit '%GIT_HASH%'"
WORKDIR /src
COPY src/Microservice.Workflow/Microservice.Workflow.csproj ./Microservice.Workflow/
RUN nuget sources Add -Name 'Ifl Artifactory (virtual)' -Source 'https://artifactory.intelliflo.io/artifactory/api/nuget/nuget-virtual'
COPY ./src/ ./
WORKDIR /src/Microservice.Workflow
RUN nuget restore "Microservice.Workflow.csproj"
RUN msbuild "Microservice.Workflow.csproj" `
    /p:Configuration=Release `
    /p:TargetFrameworkVersion=v4.7.2 `
    /p:OutputPath=../publish `
    /p:AssemblyVersion=$($Env:APPLICATION_VERSION) `
    /p:AssemblyTrademark=$($Env:GIT_HASH)


FROM build AS unit-tests
WORKDIR /results
WORKDIR /test
COPY test/Microservice.Workflow.Tests/Microservice.Workflow.Tests.csproj ./Microservice.Workflow.Tests/
RUN nuget restore "Microservice.Workflow.Tests/Microservice.Workflow.Tests.csproj"
COPY ./test/Microservice.Workflow.Tests/ ./Microservice.Workflow.Tests/
WORKDIR /test/Microservice.Workflow.Tests
RUN curl.exe https://github.com/nunit/nunit-console/releases/download/v3.11.1/NUnit.Console-3.11.1.zip -L -o NUnit.Console-3.11.1.zip -s; `
    tar -xf NUnit.Console-3.11.1.zip
RUN msbuild "Microservice.Workflow.Tests.csproj" `
    /p:Configuration=Release `
    /p:TargetFrameworkVersion=v4.7.2 `
    /p:AssemblyVersion=$($Env:APPLICATION_VERSION) `
    /p:AssemblyTrademark=$($Env:GIT_HASH)
RUN ./bin/net35/nunit3-console.exe ./bin/Release/Microservice.Workflow.Tests.dll --result unit-test-results.xml --work ./../../results


FROM base AS final
WORKDIR /database
COPY ./database/Microservice.Workflow.SqlServer/dbo .
COPY ./database/Microservice.Workflow.SqlServer/dbodata/data ./data
WORKDIR /app
COPY --from=build /src/publish .
COPY --from=tools /tools .
ADD https://artifactory.intelliflo.services/artifactory/generic-local/cert-load/certs-1.2.0.ps1 /certs.ps1
HEALTHCHECK --interval=15s --timeout=3s --retries=3 `
    CMD curl --fail http://localhost/health || exit 1
ENTRYPOINT ["powershell", "-command", "/certs.ps1", "./consul-template.exe", "-config='consul-template.hcl'", "-once"]
