# ----------------------------------------------------------------------------
# Installs PFX certificates before running another command
# ----------------------------------------------------------------------------
# Assumptions:
#  1. C:\vault-init\certs may or may not exist, if they do not,
#     this script will Finish having taking no action
#  2. Subdirectories of C:\vault-init\certs must be named with valid
#     Windows Certificate Store Locations (CurrentUser, LocalMachine, etc.)
#  3. Subdirectories of C:\vault-init\certs/LOCATION (see 2) must be named with
#     valid Store Names (to form eg. CurrentUser/My, LocalMachine/Root, etc.)
#  4. Should C:\vault-init\secrets exist, it will be checked for files with the
#     same name as PFX certificates found under C:\vault-init\certs, in the
#     format "NAME.passphrase" (to match eg. NAME.pfx). If such a file exists,
#     it will be assumed this contains the passphrase for the corresponding
#     certificate.
# ----------------------------------------------------------------------------
$usage = '
Usage:
  ./entrypoint.ps1 [COMMAND ARGS...]
'
$full_usage = $usage + '
COMMAND and ARGS can be any valid Windows CMD command, eg. for Powershell:
  ./entrypoint.ps1 powershell -c "Write-Output Hello, world!"
'

# Script Setup
$ErrorActionPreference = "Stop"
Set-strictmode -version latest
$original_cmd = $args

# Functions
function New-Directory-Recurse {
  New-Item -Path $args[0] -ItemType "directory" -ErrorAction SilentlyContinue | Out-Null
}

function Finish {
  Write-Output "*** Cert Install End ***"
  Write-Output ""
  If ( $original_cmd.count -gt 0 ) {
    Write-Output "Running original program..."
    Write-Output ""
    # Run original command and args
    $cmd, $cmdargs = $original_cmd
    & $cmd $cmdargs
    # Check LastExitCode to avoid Strict treating it as a failure if $cmd above is a Powershell command
    If ( Test-Path variable:LastExitCode ) {
      exit $LastExitCode
    } Else {
      exit 0
    }
  } Else {
    Write-Output "No command specified, exciting..."
    exit 0
  }
}

# "Constants"
$mount_dir   = 'C:\vault-init'
$certs_dir   = "$mount_dir\certs"
$secrets_dir = "$mount_dir\secrets"

Write-Output "*** Cert Install Start ***"

# Check for certs dir
$certs_dir_present = Test-Path "$certs_dir"
If ( -Not $certs_dir_present -eq "True" ) {
  Write-Output "No cert dir found at C:\vault-init\certs"
  Finish
}

# Crawl certs dir for subdirs
$certs_crawl = @(Get-ChildItem -Path $certs_dir -Recurse -Depth 2 -File) # @() forces the output to be an array, even if it contains 0 or 1 items
$deletions = New-Object System.Collections.Generic.List[System.Object] # Track files to delete on successfull run
If ( $certs_crawl.count -eq 0 ) {
  Write-Output "No certificates found to install."
  Finish
} ElseIf ( $certs_crawl.count -eq 1 ) {
  Write-Output "Found '1' certificate to install:"
} Else {
  Write-Output "Found '$($certs_crawl.count)' certificates to install:"
}
ForEach ($cert in $certs_crawl) {
  # Break out cert details
  $cert_path      = $cert.FullName
  $cert_name      = $cert.Name
  $cert_basename  = $cert.BaseName # No extension
  $store_location = $cert.Directory.Parent
  $store_name     = $cert.Directory.BaseName
  $store          = "$store_location\$store_name"
  $store_path     = "Cert:\$store_location\$store_name"
  $cert_temp_path = New-TemporaryFile

  # Check for passphrase
  $passphrase_path    = "$secrets_dir\$cert_basename.passphrase"
  $passphrase_present = Test-Path $passphrase_path

  # Decode Base64 Certificate - forced as the temp file exists, but is empty
  CertUtil -decode -f "$cert_path" $cert_temp_path.FullName

  If ($passphrase_present) {
    # $cert_pass = Get-Content -Path $passphrase_path | ConvertTo-SecureString -AsPlainText -Force
    # 'Import-PfxCertificate' DOES NOT work
    # Import-PfxCertificate -FilePath "$cert_temp_path" -CertStoreLocation "$store_path" -Password $cert_pass

    $cert_pass = Get-Content -Path $passphrase_path
    certutil -f -p $cert_pass -ImportPfx "$((Get-Item $store_path).Name)" "$cert_temp_path"
    $deletions += "$passphrase_path"
  } Else {
    Import-PfxCertificate -FilePath "$cert_temp_path" -CertStoreLocation "$store_path"
  }

  $deletions += "$cert_path"
  $deletions += "$cert_temp_path"

  # Success
  Write-Output "** Installed '$cert_basename' to '$store' **"
}

# Clean up installed files and passphrase(s)
$deletions = $deletions | Select-Object -Unique
# (temporary ignore file deletion)
# ForEach ($path in $deletions) {
#   Remove-Item "$path"
# }

Finish
