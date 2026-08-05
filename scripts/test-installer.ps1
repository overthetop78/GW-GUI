param(
    [string]$SetupPath,
    [string]$InstallDirectory,
    [string]$ExpectedVersion = '0.1.0'
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ([string]::IsNullOrWhiteSpace($SetupPath)) { $SetupPath = Join-Path $repository 'artifacts\GW-GUI-0.1.0-win-x64-setup.exe' }
if ([string]::IsNullOrWhiteSpace($InstallDirectory)) { $InstallDirectory = Join-Path $repository 'artifacts\installer-smoke' }
$setup = [IO.Path]::GetFullPath($SetupPath)
$destination = [IO.Path]::GetFullPath($InstallDirectory)
$artifactsRoot = [IO.Path]::GetFullPath((Join-Path $repository 'artifacts')) + [IO.Path]::DirectorySeparatorChar
if (-not $destination.StartsWith($artifactsRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'InstallDirectory must be located inside the repository artifacts directory.'
}
if (-not (Test-Path -LiteralPath $setup -PathType Leaf)) { throw "Installer not found: $setup" }
if (Test-Path -LiteralPath $destination) { throw "Smoke-test destination already exists: $destination" }

$uninstaller = Join-Path $destination 'unins000.exe'
try {
    $install = Start-Process -FilePath $setup -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/NOICONS', "/DIR=`"$destination`"") -Wait -PassThru
    if ($install.ExitCode -ne 0) { throw "Installer exited with code $($install.ExitCode)." }

    $required = @(
        'GW GUI.exe',
        'Documentation\user-guide.fr.md',
        'Documentation\user-guide.en.md',
        'Documentation\images\main-read-fr.png',
        'Documentation\images\main-read-en.png',
        'unins000.exe'
    )
    foreach ($relative in $required) {
        $path = Join-Path $destination $relative
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Installed file missing: $relative" }
    }
    if (Get-ChildItem -LiteralPath $destination -Recurse -File -Filter '*.pdb') { throw 'Debug symbols were installed.' }

    $version = (Get-Item -LiteralPath (Join-Path $destination 'GW GUI.exe')).VersionInfo.ProductVersion
    if (-not $version.StartsWith($ExpectedVersion, [StringComparison]::Ordinal)) {
        throw "Unexpected installed product version: $version (expected $ExpectedVersion)."
    }
    [pscustomobject]@{ InstallDirectory = $destination; ProductVersion = $version; RequiredFiles = $required.Count }
}
finally {
    if (Test-Path -LiteralPath $uninstaller -PathType Leaf) {
        $uninstall = Start-Process -FilePath $uninstaller -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART') -Wait -PassThru
        if ($uninstall.ExitCode -ne 0) { throw "Uninstaller exited with code $($uninstall.ExitCode)." }
    }
}

if (Test-Path -LiteralPath $destination) {
    $remaining = @(Get-ChildItem -LiteralPath $destination -Force)
    if ($remaining.Count -ne 0) { throw "Uninstaller left $($remaining.Count) item(s) in $destination." }
    Remove-Item -LiteralPath $destination -Force
}
Write-Output 'Installer smoke test passed and the isolated installation was removed.'
