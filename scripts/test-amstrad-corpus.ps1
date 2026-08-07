param(
    [string]$CorpusRoot = (Join-Path (Split-Path $PSScriptRoot -Parent) 'image_test')
)

$ErrorActionPreference = 'Stop'
$repository = Split-Path $PSScriptRoot -Parent
$resolved = (Resolve-Path -LiteralPath $CorpusRoot).Path
$env:GWGUI_AMSTRAD_CORPUS = $resolved
try {
    dotnet test (Join-Path $repository 'GWGUI.sln') -c Release --no-restore --filter 'FullyQualifiedName~AmstradDiskImageTests'
    if ($LASTEXITCODE -ne 0) { throw "Amstrad corpus tests failed with exit code $LASTEXITCODE." }
}
finally {
    Remove-Item Env:GWGUI_AMSTRAD_CORPUS -ErrorAction SilentlyContinue
}
