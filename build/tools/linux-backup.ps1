param([String]$environment)

. "./linux-env.ps1" $environment

#creates zip but zip contains full patch starting from '/var/www'
#for now ignoring this

echo '## Start Linux Backup ##'

$cmd = ''
$cmd = 'cd {0} && ' -f $linuxEnvFolder;
$cmd += ('zip -dd -3 -r {0} {1}' -f $linuxFullPathTempBackupZipFile, $folderNameRobiniaInfrastructureFiles);
$cmd += ' && echo STEP-OK';

LinuxExec $cmd 'Backup: Generating zip backup file on linux';
LinuxDownloadFile $linuxFullPathTempBackupZipFile $WindowsDownloadBackupFullPath;
$cmd = ('rm {0} && echo STEP-OK' -f $linuxFullPathTempBackupZipFile);
LinuxExec $cmd 'Backup: removing .zip on linux'
