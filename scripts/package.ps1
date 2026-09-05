param(
    [Parameter(Mandatory = $true)][string]$Version,
    [string]$Configuration = 'Release',
    [string]$DistDirectory,
    [switch]$SkipInstaller
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ([string]::IsNullOrWhiteSpace($DistDirectory)) { $DistDirectory = Join-Path $repository 'dist' }
$dist = [IO.Path]::GetFullPath($DistDirectory)
if (-not $dist.StartsWith($repository + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'DistDirectory must be located inside the repository.'
}

$publish = Join-Path $dist 'publish\win-x64'
$applicationPublish = Join-Path $dist '.application-publish'
$portable = Join-Path $dist 'portable\GW GUI'
$portablePackageRoot = Join-Path $dist '.portable-package'
$portablePackage = Join-Path $portablePackageRoot 'GW GUI'
foreach ($target in @($publish, $applicationPublish, $portablePackageRoot)) {
    if (Test-Path -LiteralPath $target) { Remove-Item -LiteralPath $target -Recurse -Force }
}
New-Item -ItemType Directory -Path $publish,$portable,$portablePackage -Force | Out-Null
Get-ChildItem -LiteralPath $portable -Force -ErrorAction SilentlyContinue | Where-Object { $_.Name -ne 'Data' } | Remove-Item -Recurse -Force
Get-ChildItem -LiteralPath $dist -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^GW-GUI-.+-win-x64-(portable\.zip|setup\.exe)$' -or $_.Name -eq 'SHA256SUMS.txt' } | Remove-Item -Force

dotnet publish (Join-Path $repository 'src\GWGUI.App\GWGUI.App.csproj') -c $Configuration -r win-x64 --self-contained false -p:Version=$Version -o $applicationPublish --disable-build-servers
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed.' }
dotnet publish (Join-Path $repository 'src\GWGUI.Launcher\GWGUI.Launcher.csproj') -c $Configuration -r win-x64 --self-contained false -p:Version=$Version -o $publish --disable-build-servers
if ($LASTEXITCODE -ne 0) { throw 'GW GUI bootstrap publish failed.' }
Copy-Item -Path (Join-Path $applicationPublish '*') -Destination $publish -Recurse -Force
Remove-Item -LiteralPath (Join-Path $publish 'gwgui.app.exe'),(Join-Path $publish 'gwgui.app.runtimeconfig.json') -Force
& (Join-Path $repository 'scripts\organize-application-output.ps1') -OutputDirectory $publish
Get-ChildItem -LiteralPath $publish -Recurse -File -Filter '*.pdb' | Remove-Item -Force

Copy-Item -Path (Join-Path $publish '*') -Destination $portablePackage -Recurse -Force
Copy-Item -LiteralPath (Join-Path $repository 'LICENSE') -Destination $portablePackage
Copy-Item -LiteralPath (Join-Path $repository 'README.md') -Destination $portablePackage
Copy-Item -LiteralPath (Join-Path $repository 'installer\Prerequisites') -Destination $portablePackage -Recurse -Force
New-Item -ItemType File -Path (Join-Path $portablePackage 'portable.flag') -Force | Out-Null

$zip = Join-Path $dist "GW-GUI-$Version-win-x64-portable.zip"
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
    & $iscc "/DMyAppVersion=$Version" "/DSourceDir=$publish" "/DOutputDir=$dist" (Join-Path $repository 'installer\GWGUI.iss')
    if ($LASTEXITCODE -ne 0) { throw 'Inno Setup compilation failed.' }
}

$packages = Get-ChildItem -LiteralPath $dist -File | Where-Object { $_.Extension -in '.zip', '.exe' }
$checksums = foreach ($package in $packages) { $hash = Get-FileHash -LiteralPath $package.FullName -Algorithm SHA256; "$($hash.Hash.ToLowerInvariant())  $($package.Name)" }
Set-Content -LiteralPath (Join-Path $dist 'SHA256SUMS.txt') -Value $checksums -Encoding ascii
$packages | Select-Object Name,Length
Remove-Item -LiteralPath $portablePackageRoot -Recurse -Force
Remove-Item -LiteralPath $applicationPublish -Recurse -Force
