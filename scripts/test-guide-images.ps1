param(
    [string]$PublishedDocumentationPath
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$guideDirectory = Join-Path $repository 'docs\user-guide'
$imagesDirectory = Join-Path $guideDirectory 'images'
$expected = @(
    @{ Language='fr-FR'; File='main-read-fr.png'; Guide='fr-FR.md' },
    @{ Language='en-US'; File='main-read-en.png'; Guide='en-US.md' }
)

Add-Type -AssemblyName System.Drawing
$results = foreach ($item in $expected) {
    $imagePath = Join-Path $imagesDirectory $item.File
    $guidePath = Join-Path $guideDirectory $item.Guide
    if (-not (Test-Path -LiteralPath $imagePath -PathType Leaf)) { throw "Guide image not found: $imagePath" }
    if (-not (Test-Path -LiteralPath $guidePath -PathType Leaf)) { throw "Guide not found: $guidePath" }

    $signature = [IO.File]::ReadAllBytes($imagePath)[0..7]
    $pngSignature = [byte[]](137,80,78,71,13,10,26,10)
    if ([BitConverter]::ToString($signature) -ne [BitConverter]::ToString($pngSignature)) { throw "$($item.File) is not a PNG file." }

    $image = [Drawing.Image]::FromFile($imagePath)
    try {
        if ($image.Width -lt 1900 -or $image.Height -lt 1100) { throw "$($item.File) is too small for the 150% reference capture." }
        if ([Math]::Abs($image.HorizontalResolution - 144) -gt 0.5 -or [Math]::Abs($image.VerticalResolution - 144) -gt 0.5) {
            throw "$($item.File) is $($image.HorizontalResolution)x$($image.VerticalResolution) DPI, expected Windows 150% (144 DPI)."
        }
        $guide = Get-Content -LiteralPath $guidePath -Raw
        if ($guide -notmatch [regex]::Escape("images/$($item.File)")) { throw "$($item.Guide) does not reference $($item.File)." }
        [pscustomobject]@{ Language=$item.Language; File=$item.File; Width=$image.Width; Height=$image.Height; Dpi=[Math]::Round($image.HorizontalResolution, 2); Sha256=(Get-FileHash -LiteralPath $imagePath -Algorithm SHA256).Hash.ToLowerInvariant() }
    }
    finally { $image.Dispose() }
}

if ($results[0].Sha256 -eq $results[1].Sha256) { throw 'French and English guide images are identical.' }

if ([string]::IsNullOrWhiteSpace($PublishedDocumentationPath)) {
    $candidate = Join-Path $repository 'artifacts\publish\win-x64\Documentation\user-guide'
    if (Test-Path -LiteralPath $candidate -PathType Container) { $PublishedDocumentationPath = $candidate }
}
if (-not [string]::IsNullOrWhiteSpace($PublishedDocumentationPath)) {
    $published = [IO.Path]::GetFullPath($PublishedDocumentationPath)
    foreach ($item in $expected) {
        $path = Join-Path $published "images\$($item.File)"
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Published documentation image not found: $path" }
    }
}

$results
Write-Output 'Guide image validation passed.'
