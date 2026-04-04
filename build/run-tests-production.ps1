param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('Staging', 'DevTest', 'Production')]
    $environment
);

$date = (get-date).tostring("yyyyMMdd_HHmmss")
$buildOut = "$PSScriptRoot/temp/tests-buildout-$date";
$logsOut = "$PSScriptRoot/temp/tests-logs-$date"

set-location "../backend/DNDocs.ProductionTests"
set-content "./settings.json" -encoding utf8 -value (cat "./settings.$environment.json")

dotnet test './DNDocs.ProductionTests.csproj' --results-directory $logsOut --output $buildOut 

cd "../../build"