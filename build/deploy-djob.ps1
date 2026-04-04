param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('Staging', 'Production','DevTest')]
    $environment,
    [Parameter()]
    $zipName
)

$ErrorActionPreference = "Stop"
Set-strictmode -version latest

. "$PSScriptRoot/deploy-tools.ps1" $environment

if ([string]::isnullorwhitespace($zipName)) {
    $pathZip = Get-ChildItem "$PSScriptRoot\temp" -filter "$dndocsJobAppName-*.zip" | sort name -desc | Select-Object -first 1
    $pathZip = $pathZip.fullname;

    if (!$pathZip) { throw 'Latest zip to djob deploy not found' }
} else {
    $pathZip = (resolve-path "$PSScriptRoot\temp\$zipName");
}

$command = @"
cd /mnt/usb2/dndocs
sudo systemctl stop $dndocsJobServiceName
rm -r -f ./$dndocsJobAppName
unzip -X ./$dndocsJobAppName.zip -d ./$dndocsJobAppName;
rm ./$dndocsJobAppName.zip
cp ./$dndocsJobAppName-secrets.json ./$dndocsJobAppName/secrets.json
sudo systemctl start $dndocsJobServiceName
"@

LinuxUploadFile $pathZip "/mnt/usb2/dndocs/$dndocsJobAppName.zip";
LinuxUploadFile "$PSScriptRoot\..\..\secrets\$dndocsJobAppName.secrets.json" "/mnt/usb2/dndocs/$dndocsJobAppName-secrets.json"
PlinkCommand $command