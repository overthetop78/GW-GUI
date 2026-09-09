param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Application', 'Module')]
    [string]$Scope,
    [string]$Version,
    [string]$ApplicationTag,
    [string]$Module,
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$')]
    [string]$Repository,
    [string]$ModuleTag,
    [string]$ExistingCatalog,
    [string]$DistDirectory,
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
. (Join-Path $PSScriptRoot 'emulation-modules.ps1')
if ([string]::IsNullOrWhiteSpace($DistDirectory)) { $DistDirectory = Join-Path $repositoryRoot 'dist' }
$dist = [IO.Path]::GetFullPath($DistDirectory)
if ([string]::IsNullOrWhiteSpace($OutputPath)) { $OutputPath = Join-Path $dist 'update-catalog.json' }
$output = [IO.Path]::GetFullPath($OutputPath)

function Read-ExistingCatalog([string]$Path, [string]$ExpectedKind) {
    if ([string]::IsNullOrWhiteSpace($Path) -or -not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return [ordered]@{
            schemaVersion = 2
            kind = $ExpectedKind
            generatedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
            components = @()
        }
    }
    $catalog = Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json
    if ($catalog.schemaVersion -ne 2 -or $catalog.kind -ine $ExpectedKind -or $null -eq $catalog.components) {
        throw "Invalid existing $ExpectedKind update catalog: $Path"
    }
    return $catalog
}

function Read-Sha256([string]$PackagePath) {
    if (-not (Test-Path -LiteralPath $PackagePath -PathType Leaf)) { throw "Package is missing: $PackagePath" }
    $sidecar = $PackagePath + '.sha256'
    if (Test-Path -LiteralPath $sidecar -PathType Leaf) {
        $value = ((Get-Content -LiteralPath $sidecar -Raw -Encoding ascii).Trim() -split '\s+')[0]
    } else {
        $checksumFile = Join-Path $dist 'SHA256SUMS.txt'
        if (-not (Test-Path -LiteralPath $checksumFile -PathType Leaf)) { throw "Checksum file is missing: $checksumFile" }
        $name = [IO.Path]::GetFileName($PackagePath)
        $line = Get-Content -LiteralPath $checksumFile -Encoding ascii | Where-Object { $_ -match "^[0-9a-fA-F]{64}\s+$([regex]::Escape($name))$" } | Select-Object -First 1
        if (-not $line) { throw "Checksum for '$name' is missing from $checksumFile" }
        $value = ($line -split '\s+')[0]
    }
    if ($value -notmatch '^[0-9a-fA-F]{64}$') { throw "Invalid checksum for $PackagePath" }
    $actual = (Get-FileHash -LiteralPath $PackagePath -Algorithm SHA256).Hash
    if ($actual -ine $value) { throw "Checksum does not match package: $PackagePath" }
    return $actual.ToLowerInvariant()
}

function Set-Release($Catalog, [string]$Id, [string]$Kind, $Release) {
    $component = @($Catalog.components | Where-Object { $_.id -ieq $Id }) | Select-Object -First 1
    if (-not $component) {
        $component = [ordered]@{ id = $Id; kind = $Kind; releases = @() }
        $Catalog.components = @($Catalog.components) + $component
    } elseif ($component.kind -ine $Kind) {
        throw "Component '$Id' has kind '$($component.kind)' instead of '$Kind'."
    }
    $component.releases = @($component.releases | Where-Object { $_.version -ne $Release.version }) + $Release
    $component.releases = @($component.releases | Sort-Object { [Version]$_.version } -Descending)
}

$catalogKind = $Scope.ToLowerInvariant()
$catalog = Read-ExistingCatalog $ExistingCatalog $catalogKind
$catalog.generatedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
$repositoryParts = $Repository.Split('/', 2)
$owner = $repositoryParts[0]
$repo = $repositoryParts[1]

if ($Scope -eq 'Application') {
    $catalog.components = @($catalog.components | Where-Object {
        $_.id -ieq 'gwgui' -and $_.kind -ieq 'application'
    })
    if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw 'Version is required for the application and must use X.Y.Z.' }
    if ([string]::IsNullOrWhiteSpace($ApplicationTag)) { $ApplicationTag = "v$Version" }
    if ($ApplicationTag -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]*$') { throw 'ApplicationTag contains unsupported characters.' }
    $packageName = "GW-GUI-$Version-win-x64-portable.zip"
    $packagePath = Join-Path $dist $packageName
    $release = [ordered]@{
        version = $Version
        packageUrl = "https://github.com/$owner/$repo/releases/download/$ApplicationTag/$packageName"
        sha256 = Read-Sha256 $packagePath
        notesUrl = "https://github.com/$owner/$repo/releases/tag/$ApplicationTag"
        hostApiVersion = '1.0'
    }
    Set-Release $catalog 'gwgui' 'application' $release
}

if ($Scope -eq 'Module') {
    $availableModules = @(Get-GwGuiEmulationModules -RepositoryRoot $repositoryRoot)
    if ([string]::IsNullOrWhiteSpace($Module)) {
        throw 'Module is required when Scope is Module.'
    }
    if ([string]::IsNullOrWhiteSpace($ModuleTag) `
        -or $ModuleTag -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]*$') {
        throw 'ModuleTag is required for a module and contains unsupported characters.'
    }
    $moduleDefinition = Resolve-GwGuiEmulationModule -Modules $availableModules -Module $Module
    $manifest = $moduleDefinition.Manifest
    $catalog.components = @($catalog.components | Where-Object {
        $_.id -ieq $manifest.id -and $_.kind -ieq 'module'
    })
    $packageName = "GW-GUI-Module-$($manifest.id)-$($manifest.moduleVersion)-win-x64.zip"
    $packagePath = Join-Path $dist $packageName
    $release = [ordered]@{
        version = $manifest.moduleVersion
        packageUrl = "https://github.com/$owner/$repo/releases/download/$ModuleTag/$packageName"
        sha256 = Read-Sha256 $packagePath
        notesUrl = "https://github.com/$owner/$repo/releases/tag/$ModuleTag"
        hostApiMinimum = $manifest.hostApiMinimum
        hostApiMaximum = $manifest.hostApiMaximum
    }
    Set-Release $catalog $manifest.id 'module' $release
}

$outputDirectory = Split-Path -Parent $output
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
$catalog | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $output -Encoding utf8
Get-Item -LiteralPath $output
