#!/usr/bin/env pwsh
[CmdletBinding()]
Param (
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]$ServiceName,
    [ValidateNotNullOrEmpty()]
    [string]$Tag = 'latest',
    [ValidateNotNullOrEmpty()]
    [string]$RepositoryName = 'intelliflo',
    [switch]$IgnoreLocalBuildSteps = $false
)
Begin {
    Push-Location (Join-Path $PSScriptRoot .. -Resolve)
}
Process {
    $ErrorActionPreference = 'Stop'

    # build steps applicable only to locals builds
    if($IgnoreLocalBuildSteps -ne $true) {
        # use Windows containers
        ./.docker/switch-docker-daemon.ps1 -TargetDaemon 'windows'
    }

    $containerName = "$ServiceName-database"
    $imageName = "$RepositoryName/$($containerName):$Tag"

    $databaseFolder = './database/'
    $dacpacFolder = 'dacpac'
    $dacpacFullPath = Join-Path $databaseFolder $dacpacFolder

    # remove files from previous build, or creates the folder if necessary
    if (Test-Path $dacpacFullPath) {
        Write-Output "clean up previous build..."
        Remove-Item $dacpacFullPath -Recurse -Force
    }

    # create database build output folder
    mkdir $dacpacFullPath

    # remove containers from previous build if necessary
    if ((docker container list --all --filter name=$containerName --format '{{.Names}}' | Measure-Object | Select-Object -ExpandProperty Count) -gt 0) {
        docker rm $containerName
    }

    # build can run container
    docker build --no-cache --rm -f "$(Join-Path $databaseFolder 'Dockerfile')" -t $imageName .
    docker run --name $containerName $imageName

    # ensure the container is stopped before copying build output
    $maxCheckTries = 6
    $containerCheckTries = 0
    do {
        Start-Sleep -Milliseconds 500
        $containerCheckTries++
        Write-Output "checking container status, try $containerCheckTries out of $maxCheckTries ..."
    } while (((docker ps -a -f name=$containerName --format "{{.Status}}") -notmatch '^Exited \(0\)') -and ($containerCheckTries -lt $maxCheckTries))

    # handle container hanging or unexpected exit state
    if ((docker ps -a -f name=$containerName --format "{{.Status}}") -notmatch '^Exited \(0\)') {
        throw "container hanging or database project not built correctly, please check '$containerName' container status..."
    }

    Write-Output "copying output of database build from container..."
    docker cp "$($containerName):c:\\$($dacpacFolder)\\" $databaseFolder

    # check if build output contains any files
    if ((Get-ChildItem $dacpacFullPath -ErrorAction SilentlyContinue | Measure-Object | Select-Object -ExpandProperty Count) -lt 1) {
        throw "unable to copy files from build output, please check build container..."
    }

    # remove build container
    docker rm $containerName

    Write-Output "database project was compiled successfully, please check build output at: '$dacpacFullPath'"
}
End {
    Pop-Location
}