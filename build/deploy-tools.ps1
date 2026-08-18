param(
	[Parameter(Mandatory=$true)]
	[ValidateSet('Production', 'Staging', 'DevTest')]
	$environment
)

$ErrorActionPreference = "Stop"
Set-strictmode -version latest

. "$PSScriptRoot/../../secrets/dndocs-deploy-secret.ps1" $environment 'DNDocs'

$linux_username = $luser;
$linux_password = $lpass;
$linux_server = $lsrv

$env = $environment.ToLower();
$dndocsAppName = "dndocs-app-$env";
$dndocsDocsAppName = "dndocs-docs-app-$env";
$dndocsJobAppName = "dndocs-job-app-$env";

$dndocsServiceName = "$dndocsAppName.service";
$dndocsDocsServiceName = "$dndocsDocsAppName.service";
$dndocsJobServiceName = "$dndocsJobAppName.service";

$dndocsDataPath = "/mnt/usb2/dndocs/dndocs-data-$env";
$dndocsDocsDataPath = "/mnt/usb2/dndocs/dndocs-docs-data-$env";
$dndocsJobDataPath = "/mnt/usb2/dndocs/dndocs-job-data-$env";

function PlinkCommand($commands) {
	$commands = $commands.Replace("`r`n", "`n");
	write-output 'starting plink with commands:'
	write-output $commands
	plink.exe "$linux_username@$linux_server" -batch -pw $linux_password $commands
}

function LinuxExec($command, $name) {
	if ([string]::IsNullOrEmpty($command) -or [string]::IsNullOrWhiteSpace($name)) {
		throw 'Linux exec empty command or empty name';
		return;
	}

    $command = "($command) && echo STEP-OK"
	echo 'linuxExec'
	echo ('command: {0} ' -f $command);
	echo '';
	$plargs = '-batch', '-l', $linux_username, '-pw', $linux_password, $linux_server, $command;
	$result = & "plink.exe" $plargs

	if ([string]::IsNullOrEmpty($result)) { $result = ''; }

	$result = $result.TrimEnd("`n")
	$result;

	if ($result.EndsWith('STEP-OK')) {

	}
 else {
		throw ('Linux step did not return STEP-OK. Aborting, name: {0}' -f $name);
	}
}
function LinuxUploadFile($windowsPath, $linuxPath) {
	if ([string]::IsNullOrEmpty($windowsPath) -or [string]::IsNullOrEmpty($linuxPath)) { throw 'Linux or windows path empty' }

	echo ('LinuxUploadFile linuxpath: {0}, windowpath: {1} ' -f $linuxPath, $windowsPath)
	pscp -l $linux_username -pw $linux_password $windowsPath ($linux_server + ':' + $linuxPath);
	if ($LastExitCode -ne 0) { throw 'Last exist code != 0' }
}

function LinuxDownloadFile($linuxPath, $windowsPath) {
	if ([string]::IsNullOrEmpty($linuxPath) -or [string]::IsNullOrEmpty($windowsPath)) { throw 'Linux downloadfile empty windows or linux path'; return; }

	echo ('LinuxDownloadFile linuxpath: {0}, windowpath: {1} ' -f $linuxPath, $windowsPath)
	pscp -l $linux_username -pw $linux_password ($linux_server + ':' + $linuxPath) $windowsPath;
	if ($LastExitCode -ne 0) { throw 'Last exist code != 0' }

	if ((gci $windowsPath | measure-object).Count -ne 1) {
		throw 'File not exists after downloading or more than 1 files';
	}
}
