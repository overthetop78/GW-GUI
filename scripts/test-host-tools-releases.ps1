param(
    [string]$CurrentVersion = '1.23',
    [string]$PreviousVersion = '1.22',
    [string]$WorkingDirectory
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$dist = [IO.Path]::GetFullPath((Join-Path $repository 'dist'))
$distPrefix = $dist + [IO.Path]::DirectorySeparatorChar
if ([string]::IsNullOrWhiteSpace($WorkingDirectory)) {
    $WorkingDirectory = Join-Path $dist 'host-tools-release-smoke'
}
$working = [IO.Path]::GetFullPath($WorkingDirectory)
if (-not $working.StartsWith($distPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'WorkingDirectory must be located inside the repository dist directory.'
}
if (Test-Path -LiteralPath $working) { throw "Host Tools smoke-test directory already exists: $working" }

$previousEnvironment = $env:GWGUI_REAL_HOST_TOOLS
try {
    New-Item -ItemType Directory -Path $working | Out-Null
    $installations = foreach ($version in @($CurrentVersion, $PreviousVersion)) {
        $releaseUri = "https://api.github.com/repos/keirf/greaseweazle/releases/tags/v$version"
        $release = Invoke-RestMethod -Uri $releaseUri -Headers @{ 'User-Agent' = 'GW-GUI-release-validation' }
        $assetName = "greaseweazle-$version-win64.zip"
        $asset = @($release.assets) | Where-Object { $_.name -eq $assetName } | Select-Object -First 1
        if (-not $asset) { throw "The official v$version release has no $assetName asset." }

        $archivePath = Join-Path $working $assetName
        Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $archivePath -Headers @{ 'User-Agent' = 'GW-GUI-release-validation' }
        $actualHash = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($asset.digest -and $asset.digest.StartsWith('sha256:', [StringComparison]::OrdinalIgnoreCase)) {
            $expectedHash = $asset.digest.Substring(7)
            if (-not $actualHash.Equals($expectedHash, [StringComparison]::OrdinalIgnoreCase)) {
                throw "Checksum mismatch for $assetName."
            }
        }

        $extractDirectory = Join-Path $working $version
        Expand-Archive -LiteralPath $archivePath -DestinationPath $extractDirectory
        $gw = Get-ChildItem -LiteralPath $extractDirectory -Recurse -File -Filter 'gw.exe' | Select-Object -First 1
        if (-not $gw) { throw "gw.exe is missing from $assetName." }
        foreach ($arguments in @(@('read', '--help'), @('write', '--help'), @('convert', '--help'))) {
            $savedPreference = $ErrorActionPreference
            try {
                $ErrorActionPreference = 'Continue'
                $output = & $gw.FullName @arguments 2>&1
            }
            finally { $ErrorActionPreference = $savedPreference }
            if ($LASTEXITCODE -ne 0) { throw "gw.exe $($arguments -join ' ') failed for Host Tools $version." }
            if ([string]::Join("`n", @($output)) -notmatch '--format') {
                throw "gw.exe $($arguments -join ' ') did not expose format options for Host Tools $version."
            }
        }

        [pscustomobject]@{
            Version = $version
            ArchivePath = $archivePath
            ExecutablePath = $gw.FullName
            Sha256 = $actualHash
            PublishedDigestChecked = [bool]$asset.digest
        }
    }

    $env:GWGUI_REAL_HOST_TOOLS = [string]::Join(';', @($installations | ForEach-Object { "$($_.Version)|$($_.ArchivePath)" }))
    dotnet test (Join-Path $repository 'GWGUI.sln') -c Release --no-restore --disable-build-servers --filter 'FullyQualifiedName~RealHostToolsInstallationsAreDetectedAndExposeFormatCapabilitiesWhenRequested'
    if ($LASTEXITCODE -ne 0) { throw 'The real Host Tools integration test failed.' }
    $installations | Select-Object Version,Sha256,PublishedDigestChecked
}
finally {
    $env:GWGUI_REAL_HOST_TOOLS = $previousEnvironment
    if (Test-Path -LiteralPath $working) { Remove-Item -LiteralPath $working -Recurse -Force }
}

Write-Output 'Real Host Tools release validation passed and the isolated downloads were removed.'
