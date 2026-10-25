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
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
if ([string]::IsNullOrWhiteSpace($SetupPath)) { $SetupPath = Join-Path $repository 'dist\GW-GUI-0.1.0-win-x64-setup.exe' }
if ([string]::IsNullOrWhiteSpace($InstallDirectory)) { $InstallDirectory = Join-Path $repository 'dist\installer-smoke' }
$setup = [IO.Path]::GetFullPath($SetupPath)
$destination = [IO.Path]::GetFullPath($InstallDirectory)
$distRoot = [IO.Path]::GetFullPath((Join-Path $repository 'dist')) + [IO.Path]::DirectorySeparatorChar
$uninstallRegistryPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{7B909A70-92B3-48E5-82CB-51A584ECE231}_is1'
if (-not $destination.StartsWith($distRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'InstallDirectory must be located inside the repository dist directory.'
}
if (-not (Test-Path -LiteralPath $setup -PathType Leaf)) { throw "Installer not found: $setup" }
if (Test-Path -LiteralPath $destination) { throw "Smoke-test destination already exists: $destination" }
if (Test-Path -LiteralPath $uninstallRegistryPath) {
    throw 'An installed GW GUI registration already exists. The installer smoke test refuses to replace it.'
}

$uninstaller = Join-Path $destination 'unins000.exe'
$install = $null
$uninstall = $null
try {
    $install = Start-Process -FilePath $setup -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/NOICONS', "/LANG=$InstallerLanguage", "/DIR=`"$destination`"") -PassThru
    $install.WaitForExit()
    if ($install.ExitCode -ne 0) { throw "Installer exited with code $($install.ExitCode)." }

    $required = @('gwgui.exe', 'unins000.exe', 'Modules\example\module.json',
        'Modules\example\gwgui.emulation.example.dll', 'Modules\example\gwgui.emulation.example.deps.json')
    foreach ($relative in $required) {
        $path = Join-Path $destination $relative
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Installed file missing: $relative" }
    }
    if (Test-Path -LiteralPath (Join-Path $destination 'Documentation\user-guide')) { throw 'The installer included the obsolete offline user guide.' }
    if (Get-ChildItem -LiteralPath $destination -Recurse -File -Filter '*.pdb') { throw 'Debug symbols were installed.' }
    $modulesDirectory = Join-Path $destination 'Modules'
    if (-not (Test-Path -LiteralPath $modulesDirectory -PathType Container)) {
        throw 'A clean GW GUI installation is missing its Modules directory.'
    }
    $bundledModules = @(Get-ChildItem -LiteralPath $modulesDirectory -Force)
    if ($bundledModules.Count -ne 1 -or $bundledModules[0].Name -ine 'example' -or -not $bundledModules[0].PSIsContainer) {
        throw 'A clean GW GUI installation must contain only the Example module.'
    }
    $exampleManifestPath = Join-Path $modulesDirectory 'example\module.json'
    $manifests = @(Get-ChildItem -LiteralPath $destination -Recurse -File -Filter 'module.json')
    if ($manifests.Count -ne 1 -or $manifests[0].FullName -ine $exampleManifestPath) {
        throw 'A clean GW GUI installation must contain only the Example module manifest.'
    }
    $exampleManifest = Get-Content -LiteralPath $exampleManifestPath -Raw | ConvertFrom-Json
    if ($exampleManifest.id -ine 'example' -or $exampleManifest.entryAssembly -ine 'gwgui.emulation.example.dll') {
        throw 'The bundled Example module manifest does not match its entry assembly.'
    }

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
    try {
        if ($null -ne $install -and -not $install.HasExited) {
            $install.Kill($true)
            $install.WaitForExit()
        }
        if (Test-Path -LiteralPath $uninstaller -PathType Leaf) {
            # Attendre aussi le processus enfant qui termine la suppression du désinstalleur.
            $uninstall = Start-Process -FilePath $uninstaller -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART') -WindowStyle Hidden -Wait -PassThru
            $uninstall.WaitForExit()
            if ($uninstall.ExitCode -ne 0) { throw "Uninstaller exited with code $($uninstall.ExitCode)." }
        }
    }
    finally {
        if ($null -ne $uninstall) { $uninstall.Dispose() }
        if ($null -ne $install) { $install.Dispose() }
    }
}

if (Test-Path -LiteralPath $uninstallRegistryPath) { throw 'The uninstall registration remained after cleanup.' }

if (Test-Path -LiteralPath $destination) {
    $remainingFiles = @(Get-ChildItem -LiteralPath $destination -Recurse -File -Force)
    if ($remainingFiles.Count -ne 0) {
        throw "Uninstaller left files in ${destination}: $($remainingFiles.FullName -join ', ')."
    }
    # Les fichiers doivent avoir disparu ; les répertoires vides du test sont
    # supprimés sans suppression récursive, afin de conserver ce contrôle strict.
    Get-ChildItem -LiteralPath $destination -Recurse -Directory -Force |
        Sort-Object { $_.FullName.Length } -Descending |
        ForEach-Object { Remove-Item -LiteralPath $_.FullName -Force }
    Remove-Item -LiteralPath $destination -Force
}
Write-Output 'Installer smoke test passed and the isolated installation was removed.'
