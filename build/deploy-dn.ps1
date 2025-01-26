param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('Staging', 'Production')]
    $environment,
    [Parameter()]
    $dnAppZip
)

if ($environment -eq 'Staging') { $env = 'stag' } else { $env = 'prod' }

$ErrorActionPreference = "Stop"
Set-strictmode -version latest

. "$PSScriptRoot/../../dndocs-secret/deploy-secret.ps1" $environment 'DNDocs'
. "$PSScriptRoot/deploy-tools.ps1"

if ([string]::isnullorwhitespace($dnAppZip)) {
    $dnAppZip = (Get-ChildItem "$PSScriptRoot\bin-zips" -filter "dn-$environment-*" | sort name -desc | Select-Object -first 1).fullname
}

if (![system.io.file]::Exists($dnAppZip)) { throw 'backend not exists' }

# upload front

LinuxExec "sudo systemctl stop dn-$env.service; exit 0" "stop dn service"
LinuxExec "rm -r -f /var/www/deploy-dn-unzip;" "remove old unzip if exists"
LinuxExec "mkdir /var/www/deploy-dn-unzip; " "create dn-unzip for unzipped app"
LinuxUploadFile $dnAppZip "/var/www/deploy-dn.zip";
LinuxExec "unzip /var/www/deploy-dn.zip -d /var/www/deploy-dn-unzip;" "unzip data"
LinuxUploadFile "$PSScriptRoot\..\..\dndocs-secret\appsettings.dn.$environment.json" "/var/www/deploy-dn-unzip/appsettings.$environment.json"
LinuxExec "rm -r -f /var/www/dn-$env" "remove old app files"
LinuxExec "mv /var/www/deploy-dn-unzip /var/www/dn-$env " "rename temp unzip folder to valid service folders"
LinuxExec "sudo systemctl start dn-$env" "start service"
LinuxExec "rm /var/www/deploy-dn.zip;" "cleanup"
