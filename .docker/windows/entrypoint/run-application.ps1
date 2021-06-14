#!/usr/bin/env pwsh
Begin {
    Push-Location $PSScriptRoot
}
Process {

    Write-Output "`ngenerating config...`n"
    .\SlowCheetah.Xdt.exe .\Monolith.DataSync.exe.config .\App.$env:env_name.config .\Monolith.DataSync.exe.config

    Write-Output "`nrunning the app...`n"
    .\Monolith.DataSync.exe

}
End {
    Pop-Location
}