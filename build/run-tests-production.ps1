param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('Staging', 'DevTest', 'Production')]
    $environment
);

set-location "$PSScriptRoot";
$date = (get-date).tostring("yyyyMMdd_HHmmss")
$prodTestDir = resolve-path "../backend/DNDocs.ProductionTests";
set-content "$prodTestDir/settings.json" -encoding utf8 -value (get-content "$prodTestDir/settings.$environment.json" -raw)
dotnet test "$prodTestDir/DNDocs.ProductionTests.csproj" --results-directory "$PSScriptRoot/temp/tests-prod-logs-$date" --output "$PSScriptRoot/temp/tests-prod-buildout-$date";