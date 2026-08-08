param(
    [string]$CorpusPath = (Join-Path $PSScriptRoot '..\image_test'),
    [switch]$Detailed
)

$ErrorActionPreference = 'Stop'
$resolved = (Resolve-Path -LiteralPath $CorpusPath).Path
$env:GWGUI_APPLE_CORPUS = $resolved
$arguments = @(
    'test', (Join-Path $PSScriptRoot '..\tests\GWGUI.Tests\GWGUI.Tests.csproj'),
    '--no-restore',
    '--filter', 'FullyQualifiedName~AppleDiskImageTests'
)
if ($Detailed) { $arguments += @('--logger', 'console;verbosity=detailed') }
& dotnet @arguments
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
