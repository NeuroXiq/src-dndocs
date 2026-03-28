param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('Staging', 'Production', 'DevTest')]
    $environment,
    [Parameter()]
    $dnAppZip
)

$ErrorActionPreference = "Stop"
Set-strictmode -version latest

. "$PSScriptRoot/../../secrets/dndocs-deploy-secret.ps1" $environment 'DNDocs'
. "$PSScriptRoot/deploy-tools.ps1"

$appName = "dndocs-app-$($environment.ToLower())";

if ([string]::isnullorwhitespace($dnAppZip)) {
    $dnAppZip = (Get-ChildItem "$PSScriptRoot\temp" -filter "$appName-*.zip" | sort name -desc | Select-Object -first 1).fullname
}

if (![system.io.file]::Exists($dnAppZip)) { throw 'backend not exists' }

# must run as sudo

LinuxExec "sudo systemctl stop $appName.service; exit 0" "stop dn service"
LinuxExec "rm -r -f /var/www/dndocs/$appName-unzip;" "remove old unzip if exists"
LinuxExec "mkdir /var/www/dndocs/$appName-unzip; " "create unzip for unzipped app"
LinuxUploadFile $dnAppZip "/var/www/dndocs/$appName.zip";
LinuxExec "unzip /var/www/dndocs/$appName.zip -d /var/www/dndocs/$appName-unzip;" "unzip data"
LinuxUploadFile "$PSScriptRoot\..\..\secrets\$appName.secrets.json" "/var/www/dndocs/$appName-unzip/secrets.json"
LinuxExec "rm -r -f /var/www/dndocs/$appName" "remove old app files"
LinuxExec "mv /var/www/dndocs/$appName-unzip /var/www/dndocs/$appName" "rename temp unzip folder to valid service folders"
LinuxExec "sudo systemctl start $appName.service" "start service"
LinuxExec "rm /var/www/dndocs/$appName.zip;" "cleanup"