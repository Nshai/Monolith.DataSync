#!/usr/bin/env pwsh
[CmdletBinding()]
Param ()
Begin {
    Push-Location $PSScriptRoot
}
Process {
    # use Windows containers
    ../../switch-docker-daemon.ps1 -TargetDaemon 'windows'

    # based of https://raw.githubusercontent.com/microsoft/dotnet-framework-docker/master/4.8/sdk/windowsservercore-ltsc2019/Dockerfile
    docker build -t intelliflo/dotnet-framework-sdk .
}
End {
    Pop-Location
}