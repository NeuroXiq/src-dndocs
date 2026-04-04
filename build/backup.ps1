param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('Staging', 'Production', 'DevTest')]
    $environment,
    [switch]
    $showCurrentStatus
)

. "$PSScriptRoot/deploy-tools" $environment

cd $PSScriptRoot

if ($showCurrentStatus) {
    PlinkCommand 'tail -n500 /var/www/dndocs/logs/backup-logs'
    exit;
}

$bakDate = Get-Date -Format 'yyyy-MM-dd_HH-mm-ss'
$shTemplate = Get-Content "./backup-template.sh" -Raw
$shTemplate = $shTemplate.`
    Replace('$environment',$environment.tolower()).`
    Replace('$dndocsDataPath',$dndocsDataPath).`
    Replace('$dndocsDocsDataPath',$dndocsDocsDataPath).`
    Replace('$bakDate', $bakDate);

$shTemplate | out-file -filePath './temp-backup.sh' -encoding ascii

Write-Host 'content of temp bash file: '
echo '# # # # # # # # # # # # # # # # # # # #'
cat './temp-backup.sh'
echo '# # # # # # # # # # # # # # # # # # # #'

LinuxUploadFile "./temp-backup.sh" "/var/www/dndocs/backups/backup-$env.sh";
#PlinkCommand "/bin/bash /var/www/dndocs/backups/backup-$env.sh"
PlinkCommand "/bin/bash /var/www/dndocs/backups/backup-$env.sh > /var/www/dndocs/logs/backup-logs 2>&1 & exit"



#systemctl stop 
#rsync -av --progress --log-file=$logFile $src $dest/dndocs-data-bak-$bakDate
#rsync -av --progress --log-file=$logFile $src $dest/dndocs-docs-data-bak-$bakDate

# -batch: forces not interactive mode,
#  without this after start we need to click spacebar to continue

