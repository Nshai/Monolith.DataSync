#!/usr/bin/env pwsh
[CmdletBinding()]
Param (
    [ValidateNotNullOrEmpty()]
    [string] $ImageName = 'intelliflo/microservice-workflow-apitests:linux',
    [ValidateNotNullOrEmpty()]
    [string] $Instance = [System.Environment]::MachineName,
    [ValidateNotNullOrEmpty()]
    [string] $ServiceBaseAddress = 'http://host.docker.internal:40008',
    [ValidateNotNullOrEmpty()]
    [string] $WiremockBaseAddress = 'http://host.docker.internal:19257',
    [ValidateNotNullOrEmpty()]
    [string] $AwsProfile = 'dev',
    [switch] $UseAssumedAwsCredentials,
    [switch] $UploadPipelineTestResults
)
Begin {
    Push-Location $PSScriptRoot
}
Process {
    $ErrorActionPreference = 'Stop'

    # remove "env.list" if exists
    $tmp_envListPath = Join-Path $PSScriptRoot ../env.list
    if (Test-Path $tmp_envListPath) { Remove-Item $tmp_envListPath -Force }

    # set "env.list" values
    $envVarSettings = ''
    $envVarSettings += "REASSURE__BUS__INSTANCE=$Instance"
    $envVarSettings += "`nREASSURE__SERVICEBASEADDRESS=$ServiceBaseAddress"
    $envVarSettings += "`nREASSURE__WIREMOCKBASEADDRESS=$WiremockBaseAddress"

    # save environment variables
    Set-Content -Path $tmp_envListPath -Value $envVarSettings -Encoding UTF8
    # need to convert CRLF to LF line endings
    ((Get-Content $tmp_envListPath) -join "`n") + "`n" | Set-Content -NoNewline $tmp_envListPath

    $envListPath = Join-Path $PSScriptRoot ../env.list -Resolve

    # when using '$UseAssumedAwsCredentials', the caller will be responsible to provide AWS credentials to the container
    if(-not $UseAssumedAwsCredentials) {

        # gather current user AWS credentials
        $isProfileName = $false
        $inDevProfile = $false

        # extract credentials from matched profile
        $parsedAwsCreds = Get-Content (Join-path $env:USERPROFILE '.aws/credentials' -Resolve) | ForEach-Object {
            $isProfileName = $_ -match "\[([\w-\d]+)\]"
            $matchDevProfile = $Matches[0] -match $AwsProfile

            if(($matchDevProfile) -and ($isProfileName)) { $inDevProfile = $true }

            if((-not $matchDevProfile) -and $isProfileName) { $inDevProfile = $false }

            if((-not $isProfileName) -and $inDevProfile) { return $_ }
        }

        # add credentials to "env.list"
        if (-not ((Get-Content $envListPath) -match 'aws_access_key_id')) {
            Add-Content $envListPath ($parsedAwsCreds |
                Where-Object { $_ -match "aws_access_key_id" } |
                Select-Object -First 1 |
                ForEach-Object { ($_ -replace "\s", "") -replace "^([\w_]+)", "AWS_ACCESS_KEY_ID" })
        }
        if (-not ((Get-Content $envListPath) -match 'aws_secret_access_key')) {
            Add-Content $envListPath ($parsedAwsCreds |
                Where-Object { $_ -match "aws_secret_access_key" } |
                Select-Object -First 1 |
                ForEach-Object { ($_ -replace "\s", "") -replace "^([\w_]+)", "AWS_SECRET_ACCESS_KEY" })
        }

    }

    # handle argument that skips upload of test results
    $skipUploadOfTestResults = '--skip-results-upload'
    if ($UploadPipelineTestResults) { $skipUploadOfTestResults = '' }

    # run API tests in Docker
    docker run --rm --env-file $envListPath -h $Instance $ImageName $skipUploadOfTestResults

}
End {
    Pop-Location
}
