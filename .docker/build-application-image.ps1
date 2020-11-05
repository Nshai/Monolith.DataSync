#!/usr/bin/env pwsh
[CmdletBinding()]
Param (
    [ValidateNotNullOrEmpty()]
    [string]$ServiceName = 'microservice-workflow',
    [ValidateNotNullOrEmpty()]
    [string]$Tag = 'windows',
    [ValidateNotNullOrEmpty()]
    [string]$RepositoryName = 'intelliflo',
    [string]$ApplicationVersion = '1.0.0.0',
    [string]$GitHash = $null,
    [switch]$SkipUnitTestRun = $false,
    [switch]$IgnoreLocalBuildSteps = $false
)
Begin {
    Push-Location $PSScriptRoot
}
Process {
    $ErrorActionPreference = 'Stop'

    # build steps applicable only to locals builds
    if($IgnoreLocalBuildSteps -ne $true) {
        # build database in a container
        ./build-database.ps1 -ServiceName $ServiceName -IgnoreLocalBuildSteps:$IgnoreLocalBuildSteps

        Push-Location $PSScriptRoot
    }

    $containerName = "$ServiceName"
    $imageName = "$RepositoryName/$($containerName):$Tag"
    # $unitTestImageName = "$RepositoryName/$($containerName)-unittests:$Tag"

    $buildContext = Join-Path $PSScriptRoot .. -Resolve
    $dockerfilePath = Join-Path $buildContext './src/Microservice.Workflow/Dockerfile' -Resolve


    # if ($SkipUnitTestRun -ne $true) {
    #     $localOutputFolder = Join-Path $buildContext dist

    #     # remove files from previous build, or creates the folder if necessary
    #     if (Test-Path $localOutputFolder) {
    #         Write-Output "clean up previous build...`n`n"
    #         Remove-Item $localOutputFolder -Recurse -Force
    #     }

    #     # create test output folder
    #     mkdir $localOutputFolder

    #     # fully resolve local output folder
    #     $localOutputFolder = (Resolve-Path $localOutputFolder).Path

    #     Write-Output "running unit tests...`n`n"
    #     docker build -f $dockerfilePath -t $unitTestImageName --target unit-tests $buildContext
    #     $unitTestContainer = docker create $unitTestImageName
    #     docker cp "$($unitTestContainer):/results/unit-test-results.xml" $localOutputFolder

    #     # check if test output contains any files
    #     if ((Get-ChildItem $localOutputFolder -ErrorAction SilentlyContinue | Measure-Object | Select-Object -ExpandProperty Count) -lt 1) {
    #         throw "unable to copy files from test output, please check '$unitTestImageName' image..."
    #     }

    #     docker rmi $unitTestImageName

    # } else {
    #     Write-Warning "skipping unit test run...`n`n"
    # }

    if([System.String]::IsNullOrWhiteSpace($GitHash)) {

        if(Get-Command git -ErrorAction SilentlyContinue) {
            $GitHash = git rev-parse --verify HEAD
        }

        if([System.String]::IsNullOrWhiteSpace($GitHash)) {
            $GitHash = '0000000000000000000000000000000000000000'
        }
    }

    Write-Output "building application image...`n`n"
    docker build -f $dockerfilePath --force-rm --no-cache -t $imageName --target final --build-arg APPLICATION_VERSION=$ApplicationVersion --build-arg GIT_HASH=$GitHash $buildContext
}
End {
    Pop-Location
}