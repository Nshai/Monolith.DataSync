#!/usr/bin/env pwsh
Begin {
    Push-Location $PSScriptRoot
}
Process {

    Write-Output "`ngenerating config...`n"
    .\SlowCheetah.Xdt.exe .\Microservice.Workflow.exe.config .\App.$env:env_name.config .\Microservice.Workflow.exe.config

    Write-Output "`nfix AWS authentication token path...`n"
    $env:AWS_WEB_IDENTITY_TOKEN_FILE = "C:$($env:AWS_WEB_IDENTITY_TOKEN_FILE -replace '/','\')"

    Write-Output "`nrunning the app...`n"
    .\Microservice.Workflow.exe

}
End {
    Pop-Location
}