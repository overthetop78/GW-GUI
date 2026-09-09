param(
    [string]$RegistryDirectory,
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ([string]::IsNullOrWhiteSpace($RegistryDirectory)) {
    $RegistryDirectory = Join-Path $repository 'module-registry'
}
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repository 'dist\module-directory.json'
}
$registry = [IO.Path]::GetFullPath($RegistryDirectory)
$output = [IO.Path]::GetFullPath($OutputPath)
if (-not (Test-Path -LiteralPath $registry -PathType Container)) {
    throw "Module registry directory is missing: $registry"
}

$modules = @()
$identifiers = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$catalogUrls = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($file in @(Get-ChildItem -LiteralPath $registry -File -Filter '*.json' | Sort-Object Name)) {
    $entry = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8 | ConvertFrom-Json
    if ($entry.PSObject.Properties.Name.Count -ne 3 `
        -or @($entry.PSObject.Properties.Name | Where-Object { $_ -notin @('id', 'displayName', 'catalogUrl') }).Count -ne 0) {
        throw "Registry entry must contain only id, displayName and catalogUrl: $($file.FullName)"
    }
    if ([string]::IsNullOrWhiteSpace($entry.id) -or $entry.id -notmatch '^[A-Za-z0-9_-]+$') {
        throw "Registry entry has an invalid id: $($file.FullName)"
    }
    if (-not $identifiers.Add([string]$entry.id)) {
        throw "Duplicate module id '$($entry.id)' in registry."
    }
    if ([string]::IsNullOrWhiteSpace($entry.displayName) -or $entry.displayName -ne $entry.displayName.Trim()) {
        throw "Registry entry '$($entry.id)' has an invalid displayName."
    }
    $catalogUri = $null
    if (-not [Uri]::TryCreate([string]$entry.catalogUrl, [UriKind]::Absolute, [ref]$catalogUri) `
        -or $catalogUri.Scheme -ne [Uri]::UriSchemeHttps `
        -or -not $catalogUri.AbsolutePath.EndsWith('.json', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Registry entry '$($entry.id)' must use a direct HTTPS catalog URL ending in .json."
    }
    if (-not $catalogUrls.Add($catalogUri.AbsoluteUri)) {
        throw "Duplicate module catalog URL '$($entry.catalogUrl)' in registry."
    }
    $modules += [ordered]@{
        id = [string]$entry.id
        displayName = [string]$entry.displayName
        catalogUrl = $catalogUri.AbsoluteUri
    }
}

$directory = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
    modules = @($modules | Sort-Object { $_['displayName'] })
}
$outputDirectory = Split-Path -Parent $output
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
$directory | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $output -Encoding utf8
Get-Item -LiteralPath $output
