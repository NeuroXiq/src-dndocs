Param([String]$environment);
$ErrorActionPreference = "Stop"
Set-strictmode -version latest

. "./linux-env.ps1" -environment $environment;

#deployed folder must have chmod 777 -r ...
#must have valid: .net runtime, unzip, nginx

$published_zips = ( gci -filter *$environment*.zip $Path_AppFiles_PublishedBin | sort name -desc | select -first 1);

if (($published_zips | measure-object).Count -ne 1) { 
    throw 'Did not found any zip in published folder.'
    return;
}

$path_zip_toupload = $published_zips[0].fullname;
$zip_name = $published_zips[0].name;

echo ('Publishing zip name: '+ $zip_name);

$cmd = ('sudo systemctl stop {0}' -f $linuxBackendServiceName);
$cmd += (' && sudo systemctl stop {0}' -f $linuxFrontendServiceName);
$cmd += ' && echo STEP-OK';
LinuxExec $cmd 'Stopping services';

# create linux folder
$cmd = '';
$cmd += "cd " + $linuxEnvFolder;
#$cmd += " && pwd";
$cmd += (" && (rm -r {0} ; mkdir {0})" -f $linuxTempDeployingFolder);
# #######  $cmd += " && chmod 777 -R " + $linux_tempdeploying_folder;
$cmd += " && echo STEP-OK";

LinuxExec $cmd 'Create temp deploying folder';

# upload zip to linux
echo 'uploading rar to linux'

LinuxUploadFile $path_zip_toupload $linuxTempDeployingFolder

# unzip zip on linux
# todo: remove .zip after completed
$cmd = '';
$cmd = ('cd {0}; unzip {0}/{1}' -f $linuxTempDeployingFolder, $zip_name);
#(plink -batch -l $linux_username -pw $linux_password $linux_server $cmd);

LinuxExec $cmd 'unzip on linux'

# rename current deployed instance on linux
echo 'removing old instance and making new deploy current'
$cmd = '';
$cmd += 'cd ' + $linuxEnvFolder;
$cmd += '; rm -r ' + $linuxAppFolder
$cmd += ('; mv {0} {1}' -f $linuxTempDeployingFolder, $linuxAppFolder);
$cmd += (' && chmod 777 -R {0}' -f $linuxAppFolder);
$cmd += ' && echo STEP-OK';

LinuxExec $cmd 'remove old instance and make unzipped current (rename tempdeploying dir to be app dir)'

# Reset systemd services
$cmd = ('sudo systemctl stop {0}' -f $linuxBackendServiceName);
$cmd += (' && sudo systemctl stop {0}' -f $linuxFrontendServiceName);
$cmd += (' && sudo systemctl start {0}' -f $linuxBackendServiceName);
$cmd += (' && sudo systemctl start {0}' -f $linuxFrontendServiceName);
$cmd += (' && echo STEP-OK');

LinuxExec $cmd 'Restart service on linux'
