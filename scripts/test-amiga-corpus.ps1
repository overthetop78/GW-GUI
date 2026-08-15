param(
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$originals = Join-Path $repository 'image_test\Amiga'
$generated = Join-Path $repository 'image_test\_generated\Amiga'
$tests = Join-Path $repository 'tests\GWGUI.Tests\GWGUI.Tests.csproj'

if (-not (Test-Path -LiteralPath $originals)) { throw "Amiga test-image folder not found: $originals" }
if (-not (Test-Path -LiteralPath $generated)) { throw "Generated Amiga SCP folder not found: $generated" }

$adfFiles = @(Get-ChildItem -LiteralPath $originals -File -Filter '*.adf' | Sort-Object Name)
if ($adfFiles.Count -eq 0) { throw 'No Amiga ADF test images were found.' }

$failures = @()
foreach ($adf in $adfFiles) {
    $scp = Join-Path $generated ($adf.BaseName + ' [test].scp')
    if (-not (Test-Path -LiteralPath $scp)) {
        $failures += "$($adf.Name): matching SCP is missing"
        continue
    }

    [Environment]::SetEnvironmentVariable('GWGUI_REAL_AMIGA_ADF', $adf.FullName, 'Process')
    [Environment]::SetEnvironmentVariable('GWGUI_REAL_AMIGA_SCP', [IO.Path]::GetFullPath($scp), 'Process')
    try {
        dotnet test $tests -c $Configuration --no-restore --disable-build-servers --verbosity minimal `
            --filter 'FullyQualifiedName~RealAmigaAdfAndScp|FullyQualifiedName~RealAmigaAdfRoundTripsThroughTheInternalEncoder'
        if ($LASTEXITCODE -ne 0) { $failures += $adf.Name }
    }
    finally {
        [Environment]::SetEnvironmentVariable('GWGUI_REAL_AMIGA_ADF', $null, 'Process')
        [Environment]::SetEnvironmentVariable('GWGUI_REAL_AMIGA_SCP', $null, 'Process')
    }
}

if ($failures.Count -ne 0) { throw "Amiga corpus validation failed: $($failures -join '; ')" }
Write-Output "Amiga corpus validated: $($adfFiles.Count) ADF/SCP pairs (DD, HD, OFS and FFS)."
