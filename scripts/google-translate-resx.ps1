param(
    [string[]]$Cultures = @(),
    [string]$CultureCsv = '',
    [int]$ParallelRequests = 8
)

$ErrorActionPreference = 'Stop'
$repository = Split-Path -Parent $PSScriptRoot
$resourceDirectory = Join-Path $repository 'src\GWGUI.App\Resources'
$sourcePath = Join-Path $resourceDirectory 'Strings.en-US.resx'

$languageCodes = [ordered]@{
    'de-DE' = 'de'; 'it-IT' = 'it'; 'es-ES' = 'es'; 'pl-PL' = 'pl'; 'ru-RU' = 'ru'
    'ja-JP' = 'ja'; 'zh-Hans' = 'zh-CN'; 'zh-Hant' = 'zh-TW'; 'pt-PT' = 'pt-PT'; 'pt-BR' = 'pt-BR'
    'el-GR' = 'el'; 'ko-KR' = 'ko'; 'nl-NL' = 'nl'; 'cs-CZ' = 'cs'; 'hu-HU' = 'hu'
    'tr-TR' = 'tr'; 'sv-SE' = 'sv'; 'da-DK' = 'da'; 'nb-NO' = 'no'; 'fi-FI' = 'fi'
    'ro-RO' = 'ro'; 'uk-UA' = 'uk'; 'ar-SA' = 'ar'; 'he-IL' = 'iw'; 'th-TH' = 'th'
    'id-ID' = 'id'; 'vi-VN' = 'vi'
}

if (-not [string]::IsNullOrWhiteSpace($CultureCsv)) { $Cultures = @($CultureCsv.Split(',') | ForEach-Object Trim) }
if ($Cultures.Count -eq 0) { $Cultures = @($languageCodes.Keys) }
[xml]$source = Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8
$entries = @($source.root.data)

function Protect-Text([string]$text, [hashtable]$tokens) {
    $pattern = '\{[^{}]+\}|--[a-z0-9][a-z0-9-]*|(?i)\b(?:gw\.exe|diskdefs\.cfg|SCP|ADF|IMA|IMG|MSA|D64|HFE|MFM|FM|GCR|PLL|TG43|USB|RPM|CRC|MIT|DD|HD|ED|KiB|MiB|SuperCard Pro|Greaseweazle|GW GUI|Commodore|Acorn|Atari ST|AmigaDOS|Amiga|IBM PC|Apple II|Apple Macintosh|NorthStar|Heathkit|Micral N|E-mu Emulator|TYCOM|DEC RX02|Arburg|Victor 9000|Membrain|AED 6200P|QD MO5|Centurion|HxC)\b'
    $text = [regex]::Replace($text, $pattern, {
        param($match)
        $marker = '__PH{0}__' -f $tokens.Count
        $tokens[$marker] = $match.Value
        $marker
    })
    return $text
}

function Get-TranslatedText($response) {
    $translationNode = $response[0]
    if ($translationNode -is [string]) { return [string]$translationNode }
    if ($translationNode.Count -gt 0 -and $translationNode[0] -is [string]) {
        return [string]$translationNode[0]
    }
    return $(for ($segment = 0; $segment -lt $translationNode.Count; $segment++) {
        if ($translationNode[$segment] -is [string]) { $translationNode[$segment] }
        else { $translationNode[$segment][0] }
    }) -join ''
}

function Translate-Batch([string[]]$texts, [string]$targetLanguage) {
    if ($texts.Count -gt 1) {
        return @($texts | ForEach-Object { (Translate-Batch @($_) $targetLanguage)[0] })
    }
    if ($texts.Count -eq 1 -and $texts[0] -match "`r?`n") {
        $lines = [regex]::Split($texts[0], "`r?`n")
        $translatedLines = foreach ($line in $lines) {
            if ($line.Length -eq 0) { '' } else { (Translate-Batch @($line) $targetLanguage)[0] }
        }
        return ,($translatedLines -join "`n")
    }
    $tokens = @{}
    $query = Protect-Text $texts[0] $tokens
    $uri = 'https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=' +
        [uri]::EscapeDataString($targetLanguage) + '&dt=t'

    $lastError = $null
    for ($attempt = 1; $attempt -le 5; $attempt++) {
        try {
            $response = Invoke-RestMethod -Uri $uri -Method Post -ContentType 'application/x-www-form-urlencoded; charset=UTF-8' -Body @{ q = $query } -TimeoutSec 45
            $translated = Get-TranslatedText $response
            $translated = [System.Net.WebUtility]::HtmlDecode($translated).Trim()
            foreach ($marker in $tokens.Keys) { $translated = $translated.Replace($marker, $tokens[$marker]) }
            return ,$translated
        }
        catch {
            $lastError = $_
            Start-Sleep -Seconds ([math]::Min(10, $attempt * 2))
        }
    }
    throw $lastError
}

function Translate-All([string[]]$texts, [string]$targetLanguage) {
    Add-Type -AssemblyName System.Net.Http
    $client = [System.Net.Http.HttpClient]::new()
    $client.Timeout = [TimeSpan]::FromSeconds(45)
    $results = [string[]]::new($texts.Count)
    try {
        for ($offset = 0; $offset -lt $texts.Count; $offset += $ParallelRequests) {
            $count = [Math]::Min($ParallelRequests, $texts.Count - $offset)
            $requests = @()
            for ($relative = 0; $relative -lt $count; $relative++) {
                $tokens = @{}
                $protected = Protect-Text $texts[$offset + $relative] $tokens
                $uri = 'https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=' +
                    [uri]::EscapeDataString($targetLanguage) + '&dt=t&q=' + [uri]::EscapeDataString($protected)
                $requests += [pscustomobject]@{ Index = $offset + $relative; Tokens = $tokens; Task = $client.GetStringAsync($uri) }
            }
            try {
                [System.Threading.Tasks.Task]::WaitAll([System.Threading.Tasks.Task[]]@($requests.Task))
                foreach ($request in $requests) {
                    $response = $request.Task.Result | ConvertFrom-Json
                    $value = Get-TranslatedText $response
                    $value = [System.Net.WebUtility]::HtmlDecode($value).Trim()
                    foreach ($marker in $request.Tokens.Keys) { $value = $value.Replace($marker, $request.Tokens[$marker]) }
                    $results[$request.Index] = $value
                }
            }
            catch {
                foreach ($request in $requests) {
                    try { $results[$request.Index] = (Translate-Batch @($texts[$request.Index]) $targetLanguage)[0] }
                    catch { throw "Translation failed at resource index $($request.Index): $($_.Exception.Message)" }
                }
            }
        }
    }
    finally { $client.Dispose() }
    return $results
}

foreach ($culture in $Cultures) {
    if (-not $languageCodes.Contains($culture)) { throw "Unsupported culture: $culture" }
    Write-Host "[$culture] translating $($entries.Count) resources..."
    $translatedValues = @(Translate-All @($entries | ForEach-Object { [string]$_.value }) $languageCodes[$culture])
    if ($translatedValues.Count -ne $entries.Count) { throw "[$culture] incomplete translation." }

    [xml]$target = Get-Content -LiteralPath $sourcePath -Raw -Encoding UTF8
    for ($i = 0; $i -lt $entries.Count; $i++) { $target.root.data[$i].value = $translatedValues[$i] }
    $targetPath = Join-Path $resourceDirectory "Strings.$culture.resx"
    $settings = [System.Xml.XmlWriterSettings]::new()
    $settings.Encoding = [System.Text.UTF8Encoding]::new($false)
    $settings.Indent = $true
    $writer = [System.Xml.XmlWriter]::Create($targetPath, $settings)
    try { $target.Save($writer) } finally { $writer.Dispose() }
    Write-Host "[$culture] written: $targetPath"
}
