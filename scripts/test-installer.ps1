param(
    [string]$SetupPath,
    [string]$InstallDirectory,
    [string]$ExpectedVersion = '0.1.0',
    [ValidateSet(
        'english', 'french', 'german', 'italian', 'spanish', 'polish', 'russian', 'japanese',
        'chinesesimplified', 'chinesetraditional', 'portuguese', 'brazilianportuguese', 'greek',
        'korean', 'dutch', 'czech', 'hungarian', 'turkish', 'swedish', 'danish', 'norwegian',
        'finnish', 'romanian', 'ukrainian', 'arabic', 'hebrew', 'thai', 'indonesian', 'vietnamese'
    )]
    [string]$InstallerLanguage = 'english'
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ([string]::IsNullOrWhiteSpace($SetupPath)) { $SetupPath = Join-Path $repository 'artifacts\GW-GUI-0.1.0-win-x64-setup.exe' }
if ([string]::IsNullOrWhiteSpace($InstallDirectory)) { $InstallDirectory = Join-Path $repository 'artifacts\installer-smoke' }
$setup = [IO.Path]::GetFullPath($SetupPath)
$destination = [IO.Path]::GetFullPath($InstallDirectory)
$artifactsRoot = [IO.Path]::GetFullPath((Join-Path $repository 'artifacts')) + [IO.Path]::DirectorySeparatorChar
$uninstallRegistryPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{7B909A70-92B3-48E5-82CB-51A584ECE231}_is1'
if (-not $destination.StartsWith($artifactsRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'InstallDirectory must be located inside the repository artifacts directory.'
}
if (-not (Test-Path -LiteralPath $setup -PathType Leaf)) { throw "Installer not found: $setup" }
if (Test-Path -LiteralPath $destination) { throw "Smoke-test destination already exists: $destination" }
if (Test-Path -LiteralPath $uninstallRegistryPath) {
    throw 'An installed GW GUI registration already exists. The installer smoke test refuses to replace it.'
}

$uninstaller = Join-Path $destination 'unins000.exe'
try {
    $install = Start-Process -FilePath $setup -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/NOICONS', "/LANG=$InstallerLanguage", "/DIR=`"$destination`"") -Wait -PassThru
    if ($install.ExitCode -ne 0) { throw "Installer exited with code $($install.ExitCode)." }

    $sourceGuideDirectory = Join-Path $repository 'docs\user-guide'
    $guidePdfs = @(Get-ChildItem -LiteralPath $sourceGuideDirectory -File -Filter '*.pdf')
    $required = @('gwgui.exe', 'unins000.exe') + @($guidePdfs | ForEach-Object { "Documentation\user-guide\$($_.Name)" })
    foreach ($relative in $required) {
        $path = Join-Path $destination $relative
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Installed file missing: $relative" }
    }
    $installedGuideFiles = @(Get-ChildItem -LiteralPath (Join-Path $destination 'Documentation\user-guide') -Recurse -File)
    if ($installedGuideFiles | Where-Object Extension -ne '.pdf') { throw 'The installer included Markdown or image files in the user guide.' }
    if (Get-ChildItem -LiteralPath $destination -Recurse -File -Filter '*.pdb') { throw 'Debug symbols were installed.' }

    $version = (Get-Item -LiteralPath (Join-Path $destination 'gwgui.exe')).VersionInfo.ProductVersion
    if (-not $version.StartsWith($ExpectedVersion, [StringComparison]::Ordinal)) {
        throw "Unexpected installed product version: $version (expected $ExpectedVersion)."
    }
    $registration = Get-ItemProperty -LiteralPath $uninstallRegistryPath
    $registeredLanguage = $registration.'Inno Setup: Language'
    if ($registeredLanguage -ne $InstallerLanguage) {
        throw "Installer registered language $registeredLanguage, expected $InstallerLanguage."
    }
    [pscustomobject]@{ InstallDirectory = $destination; ProductVersion = $version; InstallerLanguage = $registeredLanguage; RequiredFiles = $required.Count }
}
finally {
    if (Test-Path -LiteralPath $uninstaller -PathType Leaf) {
        $uninstall = Start-Process -FilePath $uninstaller -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART') -Wait -PassThru
        if ($uninstall.ExitCode -ne 0) { throw "Uninstaller exited with code $($uninstall.ExitCode)." }
    }
}

if (Test-Path -LiteralPath $uninstallRegistryPath) { throw 'The uninstall registration remained after cleanup.' }

if (Test-Path -LiteralPath $destination) {
    $remaining = @(Get-ChildItem -LiteralPath $destination -Force)
    if ($remaining.Count -ne 0) { throw "Uninstaller left $($remaining.Count) item(s) in $destination." }
    Remove-Item -LiteralPath $destination -Force
}
Write-Output 'Installer smoke test passed and the isolated installation was removed.'
