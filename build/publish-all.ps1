param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('Staging', 'Production')]
    $env
)
$ErrorActionPreference = "Stop"
Set-strictmode -version latest

if ($env -ne 'Staging' -and $env -ne 'Production')
{
    throw 'environment not set';
}

$pathBuildDir = $PSScriptRoot;
$pathBe = "$pathBuildDir\..\backend"
$pathFe = "$pathBuildDir\..\frontend";
$pathZips = "$pathBuildDir\bin-zips";

function Update-NextProjVersionInfo
{
    # compute somehow next version  number, does not matter how, its
    # not important now, just increment something

    $VersionFile = (resolve-path './').path + '/VERSION.txt';
    $prevVerStr = (cat $VersionFile).trim();
    $gitCommitsCount = (git rev-list --count --all).trim();
    $gitCommitHash = (git log -n1 --pretty="%H").trim();
    $verNums = $prevVerStr.split('.');
    $nextVerStr = ('{0}.{1}.{2}.{3}' -f $verNums[0], $verNums[1], ([int]$verNums[2] + 1), $gitCommitsCount);
    $longVersionStr = ('{0}+{1}+{2}+{3}+{4}' -f $nextVerStr, `
            $gitCommitHash, $gitCommitsCount, (get-date -f "yyyy-MM-ddThh:mm:ss"), $env);

    if (!(Test-Path $VersionFile -PathType Leaf))
    {
        throw 'VERSION.txt file does not exists. Create VERSION.txt with string in it like: 1.0.0.0'
        return;
    }
    set-content -path $VersionFile -value $nextVerStr;

    $r = [ordered]@{
        PackageId            = $nextVerStr;
        Title                = 'DNDocs - Assembly';
        Version              = $nextVerStr;
        Author               = 'NeuroXiq';
        Company              = 'DNDocs';
        Product              = 'DNDocs';
        InformationalVersion = $longVersionStr;
        Description          = '.NET Core API Explorer';
        Copyright            = ('Copyright {0}' -f (get-date).tostring('yyyy'));
        PackageProjectUrl    = 'https://dndocs.com/';
        AssemblyVersion      = $nextVerStr;
        FileVersion          = $nextVerStr;
        LongVersion          = $longVersionStr;
    };

    return $r;
}

write-host '# # # #'
write-host 'START PUBLISH'
write-host '# # # #'
$vi = Update-NextProjVersionInfo

$pathBuildDir = $PSScriptRoot;
$dateNow = (get-date).tostring('yyyymmdd-HHmmss');

write-output ""
#FRONTEND END

#BACKEND START
write-host 'BACKEND START'

$vsprops = @(
    '--configuration=Release',
    # "-p:dn_packageid=$($vi.PackageId)",
    "-p:dn_title=`"$($vi.Title)`"",
    "-p:dn_version=`"$($vi.Version)`"",
    "-p:dn_informationalversion=`"$($vi.InformationalVersion)`"",
    "-p:dn_author=`"$($vi.Author)`"",
    "-p:dn_company=`"$($vi.Company)`"",
    "-p:dn_product=`"$($vi.Product)`"",
    "-p:dn_description=`"$($vi.Description)`"",
    "-p:dn_copyright=`"$($vi.Copyright)`"",
    "-p:dn_packageprojecturl=`"$($vi.PackageProjectUrl)`"",
    "-p:dn_assemblyversion=`"$($vi.AssemblyVersion)`"",
    "-p:dn_fileversion=`"$($vi.FileVersion)`"");

$vsprops

$publishOutDjob = "$pathBuildDir\temp\djob-$env-$dateNow";
$publishOutDn = "$pathBuildDir\temp\dn-$env-$dateNow";
$publishOutDdocs = "$pathBuildDir\temp\ddocs-$env-$dateNow";
$publishOutDConsole = "$publishOutDjob\DNDocs.ConsoleTools"

$publishPaths1 = "$PathBe\DNDocs.Job.Web\DNDocs.Job.Web.csproj", "--output", $publishOutDjob;
$publishPaths2 = "$PathBe\DNDocs.App.Web\DNDocs.App.Web.csproj", "--output", $publishOutDn;
$publishPaths3 = "$PathBe\DNDocs.Docs.Web\DNDocs.Docs.Web.csproj", "--output", $publishOutDdocs;
$publishPaths4 = "$PathBe\DNDocs.ConsoleTools\DNDocs.ConsoleTools.csproj", "--output", $publishOutDConsole;
$publishParams1 = $publishPaths1 + $vsprops;
$publishParams2 = $publishPaths2 + $vsprops;
$publishParams3 = $publishPaths3 + $vsprops;
$publishParams4 = $publishPaths4 + $vsprops;

function dotnetPublish($p) {
    dotnet publish $p
    if ($LASTEXITCODE -ne 0) { throw 'failed to dotnet publish' }
}

dotnet publish $publishParams1
if ($LASTEXITCODE -ne 0) { throw 'failed to dotnet publish' }
dotnet publish $publishParams2
if ($LASTEXITCODE -ne 0) { throw 'failed to dotnet publish' }
dotnet publish $publishParams3
if ($LASTEXITCODE -ne 0) { throw 'failed to dotnet publish' }
dotnet publish $publishParams4
if ($LASTEXITCODE -ne 0) { throw 'failed to dotnet publish' }

# FRONTEND START
# Build frontend and copy result to 'wwwroot'

write-host 'FRONTEND START'
# Set env for VITE to compile with valid configuration
$Env:VITE_APPINFO_ENV = $env;
$Env:VITE_APPINFO_VERSION = $vi.LongVersion;

write-host 'build frontend with "npm run build"'
npm --prefix $pathFe run build;

if ($LASTEXITCODE -ne 0) { throw 'Frontend build failed: last exit code != 0' }

write-host 'copy frontend build files into wwwroot of DNDocs.Web project'
copy-item -path "$pathFe/dist/*" -destination "$publishOutDn/wwwroot" -recurse

$Env:VITE_APPINFO_ENV = '';
$Env:VITE_APPINFO_VERSION = '';

write-host 'compress all build folders into .zip files'

function CompressZip ($src, $dest) {
    $j = start-job -script {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::CreateFromDirectory($using:src, $using:dest, 'Fastest', $false );
    }

    return $j;
}

Start-Sleep -seconds 2
$j2 = CompressZip "$publishOutDn" "$PathZips\dn-$env-$($vi.version)-$dateNow.zip";
$j3 = CompressZip "$publishOutDDocs" "$PathZips\ddocs-$env-$($vi.version)-$dateNow.zip";
$j4 = CompressZip "$publishOutDjob" "$PathZips\djob-$env-$($vi.version)-$dateNow.zip";

wait-job @($j2, $j3, $j4)

write-host 'PUBLISHING COMPLETED'
