param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('Staging', 'DevTest', 'Production')]
    $environment
);

set-location "$PSScriptRoot";
$date = (get-date).tostring("yyyyMMdd_HHmmss")
$buildOut = "$PSScriptRoot/temp/tests-buildout-$date";
$logsOut = "$PSScriptRoot/temp/tests-logs-$date"
$prodTestDir = resolve-path "../backend/DNDocs.ProductionTests";

set-content "$prodTestDir/settings.json" -encoding utf8 -value (cat "$prodTestDir/settings.$environment.json")

dotnet test "$prodTestDir/DNDocs.ProductionTests.csproj" --results-directory $logsOut --output $buildOut 