param(
    [Parameter(Mandator=$true)]
    [ValidateSet('DevTest', 'Development', 'Staging')]
    $environment
)

set-location "$PSScriptRoot";
$date = (get-date).tostring("yyyyMMdd_HHmmss")
$itProjectDir = resolve-path "../backend/DNDocs.IntegrationTests";
$originalSettings = get-content "$itProjectDir/settings.json" -raw;
Set-Content "$itProjectDir/settings.json" -encoding utf8 -value (get-content "$itProjectDir/settings.$environment.json")
dotnet test "$itProjectDir/DNDocs.IntegrationTests.csproj" --results-directory "$PSScriptRoot/temp/tests-it-logs-$date" --output "$PSScriptRoot/temp/tests-it-buildout-$date";
set-content "$itProjectDir/settings.json" -encoding utf8 -value $originalSettings