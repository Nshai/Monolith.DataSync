#!/usr/bin/env pwsh
Param(
    [Parameter(Position = 0, ValueFromPipeline = $true)]
    [string]$template
)
Begin {
    Push-Location $PSScriptRoot
}
Process {

    Write-Output "`nfixing container hosts...`n"
    .\fix-container-hosts.ps1

    Write-Output "`ncalling consul-template...`n"
    .\consul-template.exe -config='consul-template.hcl' $template -once

}
End {
    Pop-Location
}