param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('Staging', 'Production', 'DevTest')]
    $env
)
$ErrorActionPreference = "Stop"
Set-strictmode -version latest

function Update-NextProjVersionInfo
{
    # compute somehow next version  number, does not matter how, its
    # not important now, just increment something

    $prevVerStr = (cat "$PSScriptRoot/VERSION.txt").trim();
    $gitCommitsCount = (git rev-list --count --all).trim();
    $gitCommitHash = (git log -n1 --pretty="%H").trim();
    $verNums = $prevVerStr.split('.');
    $nextVerStr = ('{0}.{1}.{2}' -f $verNums[0], $verNums[1], ([int]$verNums[2] + 1));
    $longVersionStr = ('{0}+{1}+{2}+{3}+{4}' -f $nextVerStr, `
            $gitCommitHash, $gitCommitsCount, (get-date -f "yyyy-MM-ddThh:mm:ss"), $env);

    set-content -path "$PSScriptRoot/VERSION.txt" -value $nextVerStr;

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
$pathBe = "$pathBuildDir\..\backend"
$pathZips = "$pathBuildDir\bin-zips";

$pathBuildDir = $PSScriptRoot;
$dateNow = (get-date).tostring('yyyymmdd-HHmmss');

$publishOutDjob = "$pathBuildDir\temp\dndocs-job-app-$($env.ToLower())-$($vi.version)-$dateNow";
$publishOutDn = "$pathBuildDir\temp\dndocs-app-$($env.ToLower())-$($vi.version)-$dateNow"
$publishOutDdocs = "$pathBuildDir\temp\dndocs-docs-app-$($env.ToLower())-$($vi.version)-$dateNow"
$publishOutDConsole = "$publishOutDjob\DNDocs.ConsoleTools"

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


write-host 'compress all build folders into .zip files'

function CompressZip ($src, $dest) {
    $j = start-job -script {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::CreateFromDirectory($using:src, $using:dest, 'Fastest', $false );
    }

    return $j;
}

Start-Sleep -seconds 2
$j2 = CompressZip "$publishOutDn" "$publishOutDn.zip";
$j3 = CompressZip "$publishOutDDocs" "$publishOutDDocs.zip";
$j4 = CompressZip "$publishOutDjob" "$publishOutDjob.zip";

wait-job @($j2, $j3, $j4)

write-host 'PUBLISHING COMPLETED'
