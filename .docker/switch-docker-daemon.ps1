#!/usr/bin/env pwsh
[CmdletBinding()]
Param (
    [Parameter(Mandatory = $true)]
    [ValidateSet("windows", "linux")]
    [string]$TargetDaemon
)
Begin { }
Process {

    $dockerver = docker version -f '{{.Server.Os}}'

    if ($dockerver -match $TargetDaemon) {
        Write-Output "Docker daemon already set to: $TargetDaemon"
    }
    else {
        Write-Output "Switching Docker daemon to use '$TargetDaemon' containers...";

        & $Env:ProgramFiles\Docker\Docker\DockerCli.exe -SwitchDaemon

        Write-Output "Docker daemon switched!"
    }

}
End { }