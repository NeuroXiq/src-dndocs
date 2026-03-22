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

. "$PSScriptRoot/../../secrets/dndocs-deploy-secret.ps1" $environment 'DNDocs'
. "$PSScriptRoot/deploy-tools.ps1"

if ([string]::isnullorwhitespace($dnAppZip)) {
    $dnAppZip = (Get-ChildItem "$PSScriptRoot\bin-zips" -filter "*-$environment-dn.zip" | sort name -desc | Select-Object -first 1).fullname
}

if (![system.io.file]::Exists($dnAppZip)) { throw 'backend not exists' }

# must run as sudo

LinuxExec "sudo systemctl stop dn-$env.service; exit 0" "stop dn service"
LinuxExec "rm -r -f /var/www/dndocs/deploy-dn-unzip;" "remove old unzip if exists"
LinuxExec "mkdir /var/www/dndocs/deploy-dn-unzip; " "create dn-unzip for unzipped app"
LinuxUploadFile $dnAppZip "/var/www/dndocs/deploy-dn.zip";
LinuxExec "unzip /var/www/dndocs/deploy-dn.zip -d /var/www/dndocs/deploy-dn-unzip;" "unzip data"
LinuxUploadFile "$PSScriptRoot\..\..\secrets\appsettings.dn.$environment.json" "/var/www/dndocs/deploy-dn-unzip/appsettings.$environment.json"
LinuxExec "rm -r -f /var/www/dndocs/dn-$env" "remove old app files"
LinuxExec "mv /var/www/dndocs/deploy-dn-unzip /var/www/dndocs/dn-$env " "rename temp unzip folder to valid service folders"
LinuxExec "sudo systemctl start dn-$env" "start service"
LinuxExec "rm /var/www/dndocs/deploy-dn.zip;" "cleanup"
