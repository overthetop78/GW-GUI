param([string]$Corpus = (Join-Path $PSScriptRoot '..\image_test'))

$ErrorActionPreference = 'Stop'
$env:GWGUI_IBM_CORPUS = (Resolve-Path -LiteralPath $Corpus).Path
dotnet test (Join-Path $PSScriptRoot '..\tests\GWGUI.Tests\GWGUI.Tests.csproj') --no-restore --disable-build-servers --filter 'FullyQualifiedName~IbmPcDiskImageTests' --verbosity minimal
exit $LASTEXITCODE
