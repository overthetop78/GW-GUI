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
foreach ($target in @($publish, $portable)) {
    if (Test-Path -LiteralPath $target) { Remove-Item -LiteralPath $target -Recurse -Force }
}
New-Item -ItemType Directory -Path $publish,$portable -Force | Out-Null
Get-ChildItem -LiteralPath $artifacts -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^GW-GUI-.+-win-x64-(portable\.zip|setup\.exe)$' -or $_.Name -eq 'SHA256SUMS.txt' } | Remove-Item -Force

dotnet publish (Join-Path $repository 'src\GWGUI.App\GWGUI.App.csproj') -c $Configuration -r win-x64 --self-contained true -p:Version=$Version -p:PublishReadyToRun=true -o $publish
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed.' }
Get-ChildItem -LiteralPath $publish -Recurse -File -Filter '*.pdb' | Remove-Item -Force

Copy-Item -Path (Join-Path $publish '*') -Destination $portable -Recurse -Force
Copy-Item -LiteralPath (Join-Path $repository 'LICENSE') -Destination $portable
Copy-Item -LiteralPath (Join-Path $repository 'README.md') -Destination $portable
New-Item -ItemType File -Path (Join-Path $portable 'portable.flag') -Force | Out-Null

$zip = Join-Path $artifacts "GW-GUI-$Version-win-x64-portable.zip"
if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }
Compress-Archive -LiteralPath $portable -DestinationPath $zip -CompressionLevel Optimal

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
