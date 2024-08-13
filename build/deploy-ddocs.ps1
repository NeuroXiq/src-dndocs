param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('Staging', 'Production')]
    $environment,
    [Parameter()]
    $zipName
)


# for now very simple deployment
# upload zip to linux temp folder, remove current app folder, rename folder and start systemd service
#

if ($environment -eq 'Staging') { $env = 'stag' } else { $env = 'prod' }

$ErrorActionPreference = "Stop"
Set-strictmode -version latest

. "$PSScriptRoot/../../dndocs-secret/deploy-secret.ps1" $environment 'DNDocsDocs'
. "$PSScriptRoot/deploy-tools.ps1"

if ([string]::isnullorwhitespace($zipName)) {
    $pathZip = Get-ChildItem "$PSScriptRoot\bin-zips" -filter "ddocs-$environment-*" | sort name -desc | Select-Object -first 1
    $pathZip = $pathZip.fullname;

    if (!$pathZip) { throw 'Latest zip to ddocs deploy not found' }
} else {
    $pathZip = (resolve-path "$PSScriptRoot\bin-zips\$zipName");
}

LinuxExec "sudo systemctl stop ddocs-$env.service ; echo STEP-OK" 'Stopping services';
LinuxUploadFile $pathZip '/var/www/ddocs-deploy.zip'
LinuxExec "rm -r -f /var/www/ddocs-deploy-unzip ; mkdir /var/www/ddocs-deploy-unzip && echo STEP-OK" "remove old ddoppcs-unzip folder if existed"
LinuxExec "(unzip /var/www/ddocs-deploy.zip -d /var/www/ddocs-deploy-unzip) && echo STEP-OK" "unzip to temp-unzip folder"
LinuxUploadFile "$PSScriptRoot\..\..\dndocs-secret\appsettings.ddocs.$environment.json" "/var/www/ddocs-deploy-unzip/appsettings.$Environment.json"
LinuxExec "rm -r -f /var/www/ddocs-$env; mv /var/www/ddocs-deploy-unzip /var/www/ddocs-$env && echo STEP-OK" "rename new temp deployed folder to valid name"
LinuxExec "sudo systemctl start ddocs-$env.service ; echo STEP-OK" 'Start service again';
LinuxExec "rm -r -f /var/www/ddocs-deploy.zip && echo STEP-OK" "cleanup zips file "