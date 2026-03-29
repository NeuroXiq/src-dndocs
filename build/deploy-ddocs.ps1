param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('Staging', 'Production', 'DevTest')]
    $environment,
    [Parameter()]
    $zipName
)

$ErrorActionPreference = "Stop"
Set-strictmode -version latest

. "$PSScriptRoot/../../secrets/dndocs-deploy-secret.ps1" $environment 'DNDocsDocs'
. "$PSScriptRoot/deploy-tools.ps1"

$appName = "dndocs-docs-app-$($environment.ToLower())";

if ([string]::isnullorwhitespace($zipName)) {
    $pathZip = Get-ChildItem "$PSScriptRoot\temp" -filter "$appName-*.zip" | sort name -desc | Select-Object -first 1
    $pathZip = $pathZip.fullname;

    if (!$pathZip) { throw 'Latest zip to ddocs deploy not found' }
} else {
    $pathZip = (resolve-path "$PSScriptRoot\temp\$zipName");
}

LinuxExec "sudo systemctl stop $appName.service ; echo STEP-OK" 'Stopping services';
LinuxUploadFile $pathZip '/var/www/dndocs/ddocs-deploy.zip'
LinuxExec "rm -r -f /var/www/dndocs/ddocs-deploy-unzip ; mkdir /var/www/dndocs/ddocs-deploy-unzip && echo STEP-OK" "remove old ddoppcs-unzip folder if existed"
LinuxExec "(unzip /var/www/dndocs/ddocs-deploy.zip -d /var/www/dndocs/ddocs-deploy-unzip) && echo STEP-OK" "unzip to temp-unzip folder"
LinuxUploadFile "$PSScriptRoot\..\..\secrets\$appName.secrets.json" "/var/www/dndocs/ddocs-deploy-unzip/secrets.json"
LinuxExec "rm -r -f /var/www/dndocs/$appName; mv /var/www/dndocs/ddocs-deploy-unzip /var/www/dndocs/$appName && echo STEP-OK" "rename new temp deployed folder to valid name"
LinuxExec "sudo systemctl start $appName.service ; echo STEP-OK" 'Start service again';
LinuxExec "rm -r -f /var/www/dndocs/ddocs-deploy.zip && echo STEP-OK" "cleanup zips file "