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

if ([string]::isnullorwhitespace($zipName)) {
    $pathZip = Get-ChildItem "$PSScriptRoot\temp" -filter "$appName-*.zip" | sort name -desc | Select-Object -first 1
    $pathZip = $pathZip.fullname;

    if (!$pathZip) { throw 'Latest zip to ddocs deploy not found' }
} else {
    $pathZip = (resolve-path "$PSScriptRoot\temp\$zipName");
}

LinuxUploadFile $pathZip '/mnt/usb2/dndocs/ddocs-deploy.zip'
LinuxUploadFile "$PSScriptRoot\..\..\secrets\$dndocsDocsAppName.secrets.json" "/mnt/usb2/dndocs/ddocs-deploy-unzip/secrets.json"

$command = @"
sudo systemctl stop $dndocsDocsServiceName

sudo systemctl start $dndocsDocsServiceName
"@

LinuxExec "rm -r -f /mnt/usb2/dndocs/ddocs-deploy-unzip ; mkdir /mnt/usb2/dndocs/ddocs-deploy-unzip && echo STEP-OK" "remove old ddoppcs-unzip folder if existed"
LinuxExec "(unzip /mnt/usb2/dndocs/ddocs-deploy.zip -d /mnt/usb2/dndocs/ddocs-deploy-unzip) && echo STEP-OK" "unzip to temp-unzip folder"

LinuxExec "rm -r -f /mnt/usb2/dndocs/$dndocsDocsAppName; mv /mnt/usb2/dndocs/ddocs-deploy-unzip /mnt/usb2/dndocs/$appName && echo STEP-OK" "rename new temp deployed folder to valid name"
LinuxExec "sudo systemctl start $dndocsDocsServiceName ; echo STEP-OK" 'Start service again';
LinuxExec "rm -r -f /mnt/usb2/dndocs/ddocs-deploy.zip && echo STEP-OK" "cleanup zips file "