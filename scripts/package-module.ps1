param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Amiga', 'Atari')]
    [string]$Module,
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [string]$DistDirectory
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ([string]::IsNullOrWhiteSpace($DistDirectory)) { $DistDirectory = Join-Path $repository 'dist' }
$dist = [IO.Path]::GetFullPath($DistDirectory)
if (-not $dist.StartsWith($repository + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw 'DistDirectory must be located inside the repository.'
}

$definitions = @{
    Amiga = @{ Project = 'src\GWGUI.Emulation.Amiga\GWGUI.Emulation.Amiga.csproj'; Assembly = 'gwgui.emulation.amiga' }
    Atari = @{ Project = 'src\GWGUI.Emulation.Atari\GWGUI.Emulation.Atari.csproj'; Assembly = 'gwgui.emulation.atari' }
}
$definition = $definitions[$Module]
$project = Join-Path $repository $definition.Project
$projectDirectory = Split-Path -Parent $project
$manifestPath = Join-Path $projectDirectory 'module.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
$manifestIsInvalid = $manifest.schemaVersion -ne 1 -or $manifest.id -ine $Module `
    -or $manifest.entryAssembly -ine ($definition.Assembly + '.dll') `
    -or $manifest.moduleVersion -notmatch '^\d+\.\d+\.\d+$' `
    -or $manifest.hostApiMinimum -notmatch '^\d+\.\d+$' `
    -or $manifest.hostApiMaximum -notmatch '^\d+\.\d+$'
if ($manifestIsInvalid) {
    throw "Invalid official module manifest: $manifestPath"
}

$workRoot = Join-Path $dist ".module-package-$($manifest.id.ToLowerInvariant())"
$publish = Join-Path $workRoot 'publish'
$packageRoot = Join-Path $workRoot 'package'
$moduleDirectory = Join-Path $packageRoot "Modules\$($manifest.id)"
if (Test-Path -LiteralPath $workRoot) { Remove-Item -LiteralPath $workRoot -Recurse -Force }
New-Item -ItemType Directory -Path $dist,$publish,$moduleDirectory -Force | Out-Null

try {
    dotnet publish $project -c $Configuration -r win-x64 --self-contained false `
        -p:Version=$($manifest.moduleVersion) -o $publish --disable-build-servers
    if ($LASTEXITCODE -ne 0) { throw "$Module module publish failed." }

    $entryAssembly = Join-Path $publish $manifest.entryAssembly
    $dependencyManifest = Join-Path $publish ($definition.Assembly + '.deps.json')
    foreach ($required in @((Join-Path $publish 'module.json'), $entryAssembly, $dependencyManifest)) {
        if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
            throw "Required module package file was not produced: $required"
        }
    }

    foreach ($source in Get-ChildItem -LiteralPath $publish -Recurse -File) {
        $isShared = $source.Name -imatch '^gwgui\.(emulation|mediaengine)\.(dll|pdb)$' `
            -or $source.Name -imatch '^(DiscUtils|LTRData)\..*\.dll$'
        if ($isShared) { continue }
        if ($Configuration -eq 'Release' -and $source.Extension -ieq '.pdb') { continue }
        $relativePath = $source.FullName.Substring($publish.Length).TrimStart('\', '/')
        $target = Join-Path $moduleDirectory $relativePath
        $targetDirectory = Split-Path -Parent $target
        if (-not (Test-Path -LiteralPath $targetDirectory -PathType Container)) {
            New-Item -ItemType Directory -Path $targetDirectory -Force | Out-Null
        }
        Copy-Item -LiteralPath $source.FullName -Destination $target -Force
    }

    $archiveName = "GW-GUI-Module-$($manifest.id)-$($manifest.moduleVersion)-win-x64.zip"
    $archive = Join-Path $dist $archiveName
    $checksum = $archive + '.sha256'
    if (Test-Path -LiteralPath $archive) { Remove-Item -LiteralPath $archive -Force }
    if (Test-Path -LiteralPath $checksum) { Remove-Item -LiteralPath $checksum -Force }
    Compress-Archive -LiteralPath (Join-Path $packageRoot 'Modules') -DestinationPath $archive -CompressionLevel Optimal
    $hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
    Set-Content -LiteralPath $checksum -Value "$hash  $archiveName" -Encoding ascii

    [pscustomobject]@{
        Module = $manifest.id
        Version = $manifest.moduleVersion
        Archive = $archive
        Checksum = $checksum
    }
}
finally {
    if (Test-Path -LiteralPath $workRoot) { Remove-Item -LiteralPath $workRoot -Recurse -Force }
}
