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

. "$PSScriptRoot/../../dndocs-secret/deploy-secret.ps1" $environment 'DNDocsJob'
. "$PSScriptRoot/deploy-tools.ps1"

if ([string]::isnullorwhitespace($zipName)) {
    $pathZip = Get-ChildItem "$PSScriptRoot\bin-zips" -filter "djob-$environment-*" | sort name -desc | Select-Object -first 1
    $pathZip = $pathZip.fullname;

    if (!$pathZip) { throw 'Latest zip to djob deploy not found' }
} else {
    $pathZip = (resolve-path "$PSScriptRoot\bin-zips\$zipName");
}

LinuxExec "sudo systemctl stop djob-$env.service ; echo STEP-OK" 'Stopping services';
LinuxUploadFile $pathZip '/var/www/djob-deploy.zip'
LinuxExec "rm -r -f /var/www/djob-deploy-unzip ; mkdir /var/www/djob-deploy-unzip && echo STEP-OK" "remove old djobs-unzip folder if existed"
LinuxExec "(unzip /var/www/djob-deploy.zip -d /var/www/djob-deploy-unzip) && echo STEP-OK" "unzip to temp-unzip folder"
LinuxUploadFile "$PSScriptRoot\..\..\dndocs-secret\appsettings.djob.$environment.json" "/var/www/djob-deploy-unzip/appsettings.$Environment.json"
LinuxExec "rm -r -f /var/www/djob-$env; mv /var/www/djob-deploy-unzip /var/www/djob-$env && echo STEP-OK" "rename new temp deployed folder to valid name"
LinuxExec "sudo systemctl start djob-$env.service ; echo STEP-OK" 'Start service again';
LinuxExec "rm -r -f /var/www/djob-deploy.zip && echo STEP-OK" "cleanup zips file "