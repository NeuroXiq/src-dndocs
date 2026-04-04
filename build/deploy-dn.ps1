param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('Staging', 'Production', 'DevTest')]
    $environment,
    [Parameter()]
    $dnAppZip
)

. "$PSScriptRoot/deploy-tools.ps1" $environment

if ([string]::isnullorwhitespace($dnAppZip)) {
    $dnAppZip = (Get-ChildItem "$PSScriptRoot\temp" -filter "$dndocsAppName-*.zip" | sort name -desc | Select-Object -first 1).fullname
}

if (![system.io.file]::Exists($dnAppZip)) { throw 'backend not exists' }

# must run as sudo
#unzip: -X ignores additional file attribues (e.g. not needed when using exFAT)

$command = @"
cd /mnt/usb2/dndocs
sudo systemctl stop $dndocsServiceName
rm -r -f ./$dndocsAppName
unzip -X ./$dndocsAppName.zip -d ./$dndocsAppName;
rm ./$dndocsAppName.zip
cp ./$dndocsAppName-secrets.json ./$dndocsAppName/secrets.json
sudo systemctl start $dndocsServiceName
"@

LinuxUploadFile $dnAppZip "/mnt/usb2/dndocs/$dndocsAppName.zip";
LinuxUploadFile "$PSScriptRoot\..\..\secrets\$dndocsAppName.secrets.json" "/mnt/usb2/dndocs/$dndocsAppName-secrets.json"
PlinkCommand $command