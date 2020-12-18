#!/usr/bin/env pwsh
Begin {
    Push-Location $PSScriptRoot
}
Process {

    Write-Output "`ngenerating config...`n"
    .\SlowCheetah.Xdt.exe .\Microservice.Workflow.exe.config .\App.$env:env_name.config .\Microservice.Workflow.exe.config

    Write-Output "`nrunning the app...`n"
    .\Microservice.Workflow.exe

}
End {
    Pop-Location
}