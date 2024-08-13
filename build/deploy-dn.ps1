param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('Staging', 'Production')]
    $environment,
    [Parameter()]
    $backendZip,
    [Parameter()]
    $frontendZip
)

if ($environment -eq 'Staging') { $env = 'stag' } else { $env = 'prod' }

$ErrorActionPreference = "Stop"
Set-strictmode -version latest

. "$PSScriptRoot/../../dndocs-secret/deploy-secret.ps1" $environment 'DNDocs'
. "$PSScriptRoot/deploy-tools.ps1"

if ([string]::isnullorwhitespace($backendZip)) {
    $backendZip = (Get-ChildItem "$PSScriptRoot\bin-zips" -filter "dn-$environment-*" | sort name -desc | Select-Object -first 1).fullname
}

if ([string]::isnullorwhitespace($frontendZip)) {
    $frontendZip = (Get-ChildItem "$PSScriptRoot\bin-zips" -filter "front-$environment-*" | sort name -desc | Select-Object -first 1).fullname
}

if (![system.io.file]::Exists($backendZip)) { throw 'backend not exists' }
if (![system.io.file]::Exists($frontendZip)) { throw 'backend not exists' }

# upload front



LinuxExec "sudo systemctl stop dnfe-$env.service; sudo systemctl stop dnbe-$env.service; exit 0" "stop frontend services"
LinuxExec "rm -r -f /var/www/deploy-fe-unzip;rm -r -f /var/www/deploy-be-unzip " "remove old unzips if exists"
LinuxExec "mkdir /var/www/deploy-fe-unzip; mkdir /var/www/deploy-be-unzip" "create dirs for unzipped"
LinuxUploadFile $frontendZip "/var/www/deploy-fe.zip";
LinuxUploadFile $backendZip "/var/www/deploy-be.zip";
LinuxExec "unzip /var/www/deploy-fe.zip -d /var/www/deploy-fe-unzip ; unzip /var/www/deploy-be.zip -d /var/www/deploy-be-unzip" "unzip data"
LinuxUploadFile "$PSScriptRoot\..\..\dndocs-secret\appsettings.dn.$environment.json" "/var/www/deploy-be-unzip/appsettings.$environment.json"
LinuxExec "rm -r -f /var/www/dnfe-$env; rm -r -f /var/www/dnbe-$env" "remove old app files"
LinuxExec "mv /var/www/deploy-fe-unzip /var/www/dnfe-$env && mv /var/www/deploy-be-unzip /var/www/dnbe-$env" "rename temp unzip folders to valid service folders"
LinuxExec "sudo systemctl start dnfe-$env && sudo systemctl start dnbe-$env" "start services"
LinuxExec "rm /var/www/deploy-be.zip; rm /var/www/deploy-fe.zip" "cleanup"
