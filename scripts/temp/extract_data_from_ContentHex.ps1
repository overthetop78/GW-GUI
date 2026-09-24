param(
    [Parameter(Mandatory = $true)]
    [string]$ReportPath,

    [string]$OutputPath,

    [string[]]$EntryName,

    [ValidateRange(1, 4096)]
    [int]$SignatureBytes = 256,

    [ValidateRange(1, 10000)]
    [int]$MaximumStrings = 500
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$resolvedReportPath = (Resolve-Path -LiteralPath $ReportPath).Path
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path (Split-Path -Parent $resolvedReportPath) 'content-analysis.json'
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath = Join-Path (Get-Location) $OutputPath
}

function Get-MediaEntries {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Entries,

        [string]$ParentPath = ''
    )

    foreach ($entry in $Entries) {
        $logicalPath = if ([string]::IsNullOrEmpty($ParentPath)) {
            [string]$entry.Name
        }
        else {
            "$ParentPath/$($entry.Name)"
        }

        [pscustomobject]@{
            Entry = $entry
            LogicalPath = $logicalPath
        }

        if ($null -ne $entry.Children -and @($entry.Children).Count -gt 0) {
            Get-MediaEntries -Entries @($entry.Children) -ParentPath $logicalPath
        }
    }
}

function Get-PrintableRuns {
    param(
        [Parameter(Mandatory = $true)]
        [byte[]]$Bytes,

        [Parameter(Mandatory = $true)]
        [int]$Limit
    )

    $runs = [System.Collections.Generic.List[string]]::new()
    $builder = [System.Text.StringBuilder]::new()

    foreach ($value in $Bytes) {
        $character = $value -band 0x7f
        if ($character -ge 0x20 -and $character -le 0x7e) {
            [void]$builder.Append([char]$character)
            continue
        }

        if ($builder.Length -ge 4) {
            $runs.Add($builder.ToString())
            if ($runs.Count -ge $Limit) { break }
        }
        [void]$builder.Clear()
    }

    if ($runs.Count -lt $Limit -and $builder.Length -ge 4) {
        $runs.Add($builder.ToString())
    }

    return $runs.ToArray()
}

function Get-LongestRun {
    param(
        [Parameter(Mandatory = $true)]
        [byte[]]$Bytes,

        [Parameter(Mandatory = $true)]
        [byte]$Value
    )

    $longest = 0
    $current = 0
    foreach ($item in $Bytes) {
        if ($item -eq $Value) {
            $current++
            if ($current -gt $longest) { $longest = $current }
        }
        else {
            $current = 0
        }
    }
    return $longest
}

$report = Get-Content -LiteralPath $resolvedReportPath -Raw | ConvertFrom-Json
$requestedNames = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
foreach ($name in @($EntryName)) {
    if (-not [string]::IsNullOrWhiteSpace($name)) { [void]$requestedNames.Add($name) }
}

$flatEntries = foreach ($volume in @($report.Volumes)) {
    if ($null -ne $volume.Entries) {
        Get-MediaEntries -Entries @($volume.Entries)
    }
}

$selectedEntries = @($flatEntries | Where-Object {
    $entry = $_.Entry
    $contentHexProperty = $entry.PSObject.Properties['ContentHex']
    $null -ne $contentHexProperty -and
        -not [string]::IsNullOrWhiteSpace([string]$contentHexProperty.Value) -and
        ($requestedNames.Count -eq 0 -or $requestedNames.Contains([string]$entry.Name))
})

if ($selectedEntries.Count -eq 0) {
    throw "No matching entries with ContentHex were found in '$resolvedReportPath'."
}

$analysis = foreach ($selected in $selectedEntries) {
    $entry = $selected.Entry
    $bytes = [Convert]::FromHexString([string]$entry.ContentHex)
    $frequency = [long[]]::new(256)
    $deltaTotal = 0L

    for ($index = 0; $index -lt $bytes.Length; $index++) {
        $frequency[$bytes[$index]]++
        if ($index -gt 0) {
            $deltaTotal += [Math]::Abs([int]$bytes[$index] - [int]$bytes[$index - 1])
        }
    }

    $entropy = 0.0
    if ($bytes.Length -gt 0) {
        foreach ($count in $frequency) {
            if ($count -eq 0) { continue }
            $probability = $count / [double]$bytes.Length
            $entropy -= $probability * [Math]::Log($probability, 2)
        }
    }

    $headCount = [Math]::Min($SignatureBytes, $bytes.Length)
    $tailCount = [Math]::Min($SignatureBytes, $bytes.Length)
    $histogram = for ($value = 0; $value -lt 256; $value++) {
        if ($frequency[$value] -gt 0) {
            [ordered]@{
                valueHex = $value.ToString('X2')
                count = $frequency[$value]
            }
        }
    }

    [ordered]@{
        name = [string]$entry.Name
        logicalPath = $selected.LogicalPath
        extension = [System.IO.Path]::GetExtension([string]$entry.Name).ToLowerInvariant()
        length = $bytes.Length
        category = [string]$entry.Category
        contentFormat = [string]$entry.ContentFormat
        textEncoding = [string]$entry.TextEncoding
        preview = [string]$entry.Preview
        metadata = $entry.Metadata
        sha256 = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
        headerHex = if ($headCount -eq 0) { '' } else { [Convert]::ToHexString($bytes[0..($headCount - 1)]) }
        trailerHex = if ($tailCount -eq 0) { '' } else { [Convert]::ToHexString($bytes[($bytes.Length - $tailCount)..($bytes.Length - 1)]) }
        distinctByteCount = @($frequency | Where-Object { $_ -gt 0 }).Count
        zeroByteCount = $frequency[0]
        longestZeroRun = Get-LongestRun -Bytes $bytes -Value 0
        averageByteValue = if ($bytes.Length -eq 0) { 0 } else { ($bytes | Measure-Object -Average).Average }
        averageAdjacentDelta = if ($bytes.Length -le 1) { 0 } else { $deltaTotal / [double]($bytes.Length - 1) }
        entropyBitsPerByte = $entropy
        byteHistogram = @($histogram)
        printableRuns7Bit = @(Get-PrintableRuns -Bytes $bytes -Limit $MaximumStrings)
    }
}

$outputDirectory = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
    [System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
}

[ordered]@{
    sourceReport = $resolvedReportPath
    generatedAt = [DateTimeOffset]::UtcNow.ToString('O')
    selectedEntryCount = @($analysis).Count
    entries = @($analysis)
} | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $OutputPath -Encoding utf8

Write-Output $OutputPath
