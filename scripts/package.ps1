param(
    [Parameter(Mandatory = $true)][string]$Version,
    [string]$Configuration = 'Release',
    [string]$ArtifactsDirectory,
    [switch]$SkipInstaller
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ([string]::IsNullOrWhiteSpace($ArtifactsDirectory)) { $ArtifactsDirectory = Join-Path $repository 'artifacts' }
$artifacts = [IO.Path]::GetFullPath($ArtifactsDirectory)
if (-not $artifacts.StartsWith($repository + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'ArtifactsDirectory must be located inside the repository.'
}

$publish = Join-Path $artifacts 'publish\win-x64'
$portable = Join-Path $artifacts 'portable\GW GUI'
$portablePackageRoot = Join-Path $artifacts '.portable-package'
$portablePackage = Join-Path $portablePackageRoot 'GW GUI'
foreach ($target in @($publish, $portablePackageRoot)) {
    if (Test-Path -LiteralPath $target) { Remove-Item -LiteralPath $target -Recurse -Force }
}
New-Item -ItemType Directory -Path $publish,$portable,$portablePackage -Force | Out-Null
Get-ChildItem -LiteralPath $portable -Force -ErrorAction SilentlyContinue | Where-Object { $_.Name -ne 'Data' } | Remove-Item -Recurse -Force
Get-ChildItem -LiteralPath $artifacts -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^GW-GUI-.+-win-x64-(portable\.zip|setup\.exe)$' -or $_.Name -eq 'SHA256SUMS.txt' } | Remove-Item -Force

dotnet publish (Join-Path $repository 'src\GWGUI.App\GWGUI.App.csproj') -c $Configuration -r win-x64 --self-contained true -p:Version=$Version -p:PublishReadyToRun=true -o $publish --disable-build-servers
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed.' }
Get-ChildItem -LiteralPath $publish -Recurse -File -Filter '*.pdb' | Remove-Item -Force

# Keep satellite translation assemblies out of the application root.
$languageDirectories = @(Get-ChildItem -LiteralPath $publish -Directory | Where-Object {
    @(Get-ChildItem -LiteralPath $_.FullName -File -Filter '*.resources.dll').Count -gt 0
})
if ($languageDirectories.Count -gt 0) {
    $languages = Join-Path $publish 'Languages'
    New-Item -ItemType Directory -Path $languages -Force | Out-Null
    $dependencyManifests = @(Get-ChildItem -LiteralPath $publish -File -Filter '*.deps.json')
    foreach ($languageDirectory in $languageDirectories) {
        foreach ($resource in Get-ChildItem -LiteralPath $languageDirectory.FullName -File -Filter '*.resources.dll') {
            $publishedPath = "$($languageDirectory.Name)/$($resource.Name)"
            $packagedPath = "Languages/$publishedPath"
            foreach ($manifest in $dependencyManifests) {
                $content = [IO.File]::ReadAllText($manifest.FullName)
                $updated = $content.Replace(('"' + $publishedPath + '"'), ('"' + $packagedPath + '"'))
                if ($updated -ne $content) { [IO.File]::WriteAllText($manifest.FullName, $updated) }
            }
        }
        Move-Item -LiteralPath $languageDirectory.FullName -Destination (Join-Path $languages $languageDirectory.Name)
    }
}

Copy-Item -Path (Join-Path $publish '*') -Destination $portablePackage -Recurse -Force
Copy-Item -LiteralPath (Join-Path $repository 'LICENSE') -Destination $portablePackage
Copy-Item -LiteralPath (Join-Path $repository 'README.md') -Destination $portablePackage
New-Item -ItemType File -Path (Join-Path $portablePackage 'portable.flag') -Force | Out-Null

$zip = Join-Path $artifacts "GW-GUI-$Version-win-x64-portable.zip"
if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }
$zipCreated = $false
for ($attempt = 1; $attempt -le 5 -and -not $zipCreated; $attempt++) {
    try {
        Compress-Archive -LiteralPath $portablePackage -DestinationPath $zip -CompressionLevel Optimal -ErrorAction Stop
        $zipCreated = $true
    }
    catch {
        if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force -ErrorAction SilentlyContinue }
        if ($attempt -eq 5) { throw "Portable ZIP creation failed after 5 attempts. Close any running GW GUI instance or process using '$portable'. $($_.Exception.Message)" }
        Write-Warning "Portable ZIP attempt $attempt failed because a file is temporarily in use. Retrying..."
        Start-Sleep -Milliseconds (500 * $attempt)
    }
}

# Keep the locally runnable portable copy current without deleting its private Data folder.
Copy-Item -Path (Join-Path $portablePackage '*') -Destination $portable -Recurse -Force

if (-not $SkipInstaller) {
    $iscc = (Get-Command ISCC.exe -ErrorAction SilentlyContinue).Source
    if (-not $iscc) {
        $candidates = @((Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe'), (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'))
        $iscc = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
    }
    if (-not $iscc) { throw 'Inno Setup 6 (ISCC.exe) was not found. Use -SkipInstaller to create only the portable ZIP.' }
    & $iscc "/DMyAppVersion=$Version" "/DSourceDir=$publish" "/DOutputDir=$artifacts" (Join-Path $repository 'installer\GWGUI.iss')
    if ($LASTEXITCODE -ne 0) { throw 'Inno Setup compilation failed.' }
}

$packages = Get-ChildItem -LiteralPath $artifacts -File | Where-Object { $_.Extension -in '.zip', '.exe' }
$checksums = foreach ($package in $packages) { $hash = Get-FileHash -LiteralPath $package.FullName -Algorithm SHA256; "$($hash.Hash.ToLowerInvariant())  $($package.Name)" }
Set-Content -LiteralPath (Join-Path $artifacts 'SHA256SUMS.txt') -Value $checksums -Encoding ascii
$packages | Select-Object Name,Length
Remove-Item -LiteralPath $portablePackageRoot -Recurse -Force
