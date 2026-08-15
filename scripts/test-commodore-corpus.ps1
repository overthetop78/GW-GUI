param(
    [string]$CorpusPath = (Join-Path $PSScriptRoot '..\image_test')
)

$ErrorActionPreference = 'Stop'
$resolved = (Resolve-Path -LiteralPath $CorpusPath).Path
$env:GWGUI_COMMODORE_CORPUS = $resolved
try {
    & dotnet test (Join-Path $PSScriptRoot '..\tests\GWGUI.Tests\GWGUI.Tests.csproj') --no-restore --disable-build-servers --filter 'FullyQualifiedName~CommodoreDiskImageTests'
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
finally {
    Remove-Item Env:GWGUI_COMMODORE_CORPUS -ErrorAction SilentlyContinue
}
