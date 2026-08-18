param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('Staging', 'Production', 'DevTest')]
    $environment,
    [Parameter()]
    $zipName
)

$ErrorActionPreference = "Stop"
Set-strictmode -version latest

. "$PSScriptRoot/deploy-tools.ps1" $environment

if ([string]::isnullorwhitespace($zipName)) {
    $pathZip = Get-ChildItem "$PSScriptRoot\temp" -filter "$dndocsDocsAppName-*.zip" | sort name -desc | Select-Object -first 1
    $pathZip = $pathZip.fullname;

    if (!$pathZip) { throw 'Latest zip to ddocs deploy not found' }
} else {
    $pathZip = (resolve-path "$PSScriptRoot\temp\$zipName");
}

$command = @"
cd /var/www/dndocs
sudo systemctl stop $dndocsDocsServiceName
rm -r -f ./$dndocsDocsAppName
unzip -X ./$dndocsDocsAppName.zip -d ./$dndocsDocsAppName;
rm ./$dndocsDocsAppName.zip
cp ./$dndocsDocsAppName-secrets.json ./$dndocsDocsAppName/secrets.json
sudo systemctl start $dndocsDocsServiceName
"@

LinuxUploadFile $pathZip "/var/www/dndocs/$dndocsDocsAppName.zip";
LinuxUploadFile "$PSScriptRoot\..\..\secrets\$dndocsDocsAppName.secrets.json" "/var/www/dndocs/$dndocsDocsAppName-secrets.json"
PlinkCommand $command
