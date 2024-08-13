Param([String]$environment, [String]$bakname)
$ErrorActionPreference = "Stop"
Set-strictmode -version latest

. ./linux-env.ps1 $environment

if (!$bakname) {
    throw "'bakname' parameter is null or empty. Provide backup file name from backups folder."
}

$WindowsZipBakFullPath = "$Path_AppFiles_Backups\$bakname";
$SPath_UploadBakInto = "$LPath_TempBakRestoreDir/$bakname";
$SPath_OldInfrDir = "$SPath_InfrastructureDir-restorebakold";

if (!(test-path $WindowsZipBakFullPath)) {
    throw "'$bakname' does not exists in backups folder ($WindowsZipBakFullPath)";
}

$cmd = "";
$cmd += "(rm -r $LPath_TempBakRestoreDir;mkdir $LPath_TempBakRestoreDir) && echo STEP-OK";

LinuxExec $cmd 'Creating temp folder on linux where zip will be downloaded'

LinuxUploadFile $WindowsZipBakFullPath $SPath_UploadBakInto

$cmd = "";
$cmd += "mv $SPath_InfrastructureDir $SPath_OldInfrDir ";
$cmd += "&& echo STEP-OK"

LinuxExec $cmd 'Rename current infrastructure dir to indicate it is old'

$cmd = '';
$cmd += "(cd $LPath_TempBakRestoreDir && unzip $bakname -d $linuxEnvFolder) "
$cmd += '&& echo STEP-OK'

LinuxExec $cmd 'Unzip backup to be new infrastructure folder';

$cmd = '';
$cmd = "rm -r $LPath_TempBakRestoreDir && "
$cmd += "rm -r $SPath_OldInfrDir &&"
$cmd += "echo STEP-OK"

LinuxExec $cmd 'Removing temp-back folder and old infrastructure folder'
