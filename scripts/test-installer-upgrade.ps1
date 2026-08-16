param(
    [string]$CurrentSetupPath,
    [string]$CurrentVersion = '0.1.0',
    [string]$PreviousVersion = '0.0.0',
    [string]$InstallDirectory
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$artifacts = [IO.Path]::GetFullPath((Join-Path $repository 'artifacts'))
$artifactsPrefix = $artifacts + [IO.Path]::DirectorySeparatorChar
if ([string]::IsNullOrWhiteSpace($CurrentSetupPath)) {
    $CurrentSetupPath = Join-Path $artifacts "GW-GUI-$CurrentVersion-win-x64-setup.exe"
}
if ([string]::IsNullOrWhiteSpace($InstallDirectory)) {
    $InstallDirectory = Join-Path $artifacts 'installer-upgrade-smoke'
}

$currentSetup = [IO.Path]::GetFullPath($CurrentSetupPath)
$destination = [IO.Path]::GetFullPath($InstallDirectory)
$fixtureDirectory = Join-Path $artifacts 'installer-upgrade-fixture'
$publishDirectory = Join-Path $artifacts 'publish\win-x64'
$uninstallRegistryPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{7B909A70-92B3-48E5-82CB-51A584ECE231}_is1'

if (-not $destination.StartsWith($artifactsPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'InstallDirectory must be located inside the repository artifacts directory.'
}
if (-not (Test-Path -LiteralPath $currentSetup -PathType Leaf)) { throw "Current installer not found: $currentSetup" }
if (-not (Test-Path -LiteralPath (Join-Path $publishDirectory 'gwgui.exe') -PathType Leaf)) {
    throw "Published application not found: $publishDirectory"
}
if (Test-Path -LiteralPath $destination) { throw "Upgrade-test destination already exists: $destination" }
if (Test-Path -LiteralPath $fixtureDirectory) { throw "Upgrade-test fixture directory already exists: $fixtureDirectory" }
if (Test-Path -LiteralPath $uninstallRegistryPath) {
    throw 'An installed GW GUI registration already exists. The upgrade smoke test refuses to replace it.'
}

$iscc = (Get-Command ISCC.exe -ErrorAction SilentlyContinue).Source
if (-not $iscc) {
    $candidates = @(
        (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe'),
        (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe')
    )
    $iscc = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}
if (-not $iscc) { throw 'Inno Setup 6 (ISCC.exe) was not found.' }

$uninstaller = Join-Path $destination 'unins000.exe'
try {
    New-Item -ItemType Directory -Path $fixtureDirectory | Out-Null
    & $iscc "/DMyAppVersion=$PreviousVersion" "/DSourceDir=$publishDirectory" "/DOutputDir=$fixtureDirectory" (Join-Path $repository 'installer\GWGUI.iss') | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Previous-version fixture compilation failed.' }
    $previousSetup = Join-Path $fixtureDirectory "GW-GUI-$PreviousVersion-win-x64-setup.exe"
    if (-not (Test-Path -LiteralPath $previousSetup -PathType Leaf)) { throw 'Previous-version fixture was not produced.' }

    $installArguments = @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/NOICONS', "/DIR=`"$destination`"")
    $previousInstall = Start-Process -FilePath $previousSetup -ArgumentList $installArguments -Wait -PassThru
    if ($previousInstall.ExitCode -ne 0) { throw "Previous installer exited with code $($previousInstall.ExitCode)." }
    $previousRegistration = Get-ItemProperty -LiteralPath $uninstallRegistryPath
    if ($previousRegistration.DisplayVersion -ne $PreviousVersion) {
        throw "Previous installer registered version $($previousRegistration.DisplayVersion), expected $PreviousVersion."
    }

    # Reproduce an upgrade from the former self-contained package. These files
    # make apphost prefer an application-local .NET runtime and cause the false
    # "install Microsoft .NET" prompt when that old runtime is incomplete.
    $obsoleteRuntimeFiles = @('hostfxr.dll', 'hostpolicy.dll', 'coreclr.dll', 'GW GUI.dll')
    foreach ($obsoleteRuntimeFile in $obsoleteRuntimeFiles) {
        Set-Content -LiteralPath (Join-Path $destination $obsoleteRuntimeFile) -Value 'obsolete runtime fixture'
    }

    $currentInstall = Start-Process -FilePath $currentSetup -ArgumentList $installArguments -Wait -PassThru
    if ($currentInstall.ExitCode -ne 0) { throw "Current installer exited with code $($currentInstall.ExitCode)." }
    $currentRegistration = Get-ItemProperty -LiteralPath $uninstallRegistryPath
    if ($currentRegistration.DisplayVersion -ne $CurrentVersion) {
        throw "Upgrade registered version $($currentRegistration.DisplayVersion), expected $CurrentVersion."
    }
    $productVersion = (Get-Item -LiteralPath (Join-Path $destination 'gwgui.exe')).VersionInfo.ProductVersion
    if (-not $productVersion.StartsWith($CurrentVersion, [StringComparison]::Ordinal)) {
        throw "Upgraded executable version is $productVersion, expected $CurrentVersion."
    }
    foreach ($obsoleteRuntimeFile in $obsoleteRuntimeFiles) {
        if (Test-Path -LiteralPath (Join-Path $destination $obsoleteRuntimeFile)) {
            throw "Upgrade left obsolete application-local runtime file: $obsoleteRuntimeFile"
        }
    }

    [pscustomobject]@{
        InstallDirectory = $destination
        PreviousVersion = $PreviousVersion
        CurrentVersion = $currentRegistration.DisplayVersion
        ProductVersion = $productVersion
    }
}
finally {
    if (Test-Path -LiteralPath $uninstaller -PathType Leaf) {
        $uninstall = Start-Process -FilePath $uninstaller -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART') -Wait -PassThru
        if ($uninstall.ExitCode -ne 0) { throw "Uninstaller exited with code $($uninstall.ExitCode)." }
    }
    if (Test-Path -LiteralPath $uninstallRegistryPath) { throw 'The uninstall registration remained after cleanup.' }
    if (Test-Path -LiteralPath $destination) {
        $remaining = @(Get-ChildItem -LiteralPath $destination -Force)
        if ($remaining.Count -ne 0) { throw "Uninstaller left $($remaining.Count) item(s) in $destination." }
        Remove-Item -LiteralPath $destination -Force
    }
    if (Test-Path -LiteralPath $fixtureDirectory) {
        Remove-Item -LiteralPath $fixtureDirectory -Recurse -Force
    }
}

Write-Output 'Installer upgrade smoke test passed and all isolated state was removed.'
