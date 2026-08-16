param(
    [Parameter(Mandatory = $true)][string]$PublishedUserGuidePath
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$source = Join-Path $repository 'docs\user-guide'
$published = [IO.Path]::GetFullPath($PublishedUserGuidePath)

if (-not (Test-Path -LiteralPath $published -PathType Container)) {
    throw "Published user-guide directory not found: $published"
}

$sourcePdfs = @(Get-ChildItem -LiteralPath $source -File -Filter '*.pdf' | Sort-Object Name)
if ($sourcePdfs.Count -eq 0) { throw 'No source user-guide PDF was found.' }

$publishedFiles = @(Get-ChildItem -LiteralPath $published -Recurse -File)
$publishedPdfs = @($publishedFiles | Where-Object Extension -eq '.pdf' | Sort-Object Name)
$unexpected = @($publishedFiles | Where-Object Extension -ne '.pdf')
if ($unexpected.Count -gt 0) {
    throw "Non-PDF user-guide files were published: $($unexpected.FullName -join ', ')"
}

$sourceNames = @($sourcePdfs.Name)
$publishedNames = @($publishedPdfs.Name)
if (Compare-Object -ReferenceObject $sourceNames -DifferenceObject $publishedNames) {
    throw "Published user-guide PDFs do not match the source PDFs. Source: $($sourceNames -join ', '); published: $($publishedNames -join ', ')"
}

$publishedPdfs | Select-Object Name, Length
Write-Output 'Published user-guide contains every source PDF and no Markdown or image file.'
