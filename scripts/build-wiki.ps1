param()

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$source = Join-Path $repository 'wiki'
$output = [IO.Path]::GetFullPath((Join-Path $repository 'build\wiki'))
$wikiUrl = 'https://github.com/overthetop78/GW-GUI/wiki'
$assetsUrl = 'https://raw.githubusercontent.com/wiki/overthetop78/GW-GUI'
$utf8 = New-Object System.Text.UTF8Encoding($false)
$pages = @(Get-ChildItem -LiteralPath $source -Recurse -File -Filter '*.md')
if ($pages.Count -eq 0) { throw 'No wiki pages found.' }
if ($pages | Group-Object BaseName | Where-Object Count -gt 1) { throw 'Wiki page names must be unique across all languages.' }

# Validate all local targets before producing the publication copy.
$rendered = foreach ($page in $pages) {
    $content = [IO.File]::ReadAllText($page.FullName)
    $converted = [regex]::Replace($content, '\]\(([^)]+)\)|(<img\b[^>]*\bsrc=")([^"]+)(")', [System.Text.RegularExpressions.MatchEvaluator]{
        param($match)
        $htmlImage = $match.Groups[3].Success
        $link = if ($htmlImage) { $match.Groups[3].Value } else { $match.Groups[1].Value }
        $prefix = if ($htmlImage) { $match.Groups[2].Value } else { '](' }
        $suffix = if ($htmlImage) { $match.Groups[4].Value } else { ')' }
        if ($link -match '^(#|[a-zA-Z][a-zA-Z0-9+.-]*:)') { return $match.Value }
        $parts = $link -split '#', 2
        $target = [IO.Path]::GetFullPath((Join-Path $page.DirectoryName ([Uri]::UnescapeDataString($parts[0]))))
        if (-not $target.StartsWith($source + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw "Link outside wiki in $($page.Name): $link" }
        if (-not (Test-Path -LiteralPath $target -PathType Leaf)) { throw "Broken link in $($page.Name): $link" }
        $fragment = if ($parts.Length -eq 2) { '#' + $parts[1] } else { '' }
        if ([IO.Path]::GetExtension($target) -eq '.md') {
            return $prefix + $wikiUrl + '/' + [Uri]::EscapeDataString([IO.Path]::GetFileNameWithoutExtension($target)) + $fragment + $suffix
        }
        $relative = $target.Substring($source.Length + 1).Replace('\', '/')
        if (-not $relative.StartsWith('images/')) { throw "Unsupported wiki asset: $relative" }
        $escaped = ($relative.Split('/') | ForEach-Object { [Uri]::EscapeDataString($_) }) -join '/'
        return $prefix + $assetsUrl + '/' + $escaped + $fragment + $suffix
    })
    [pscustomobject]@{ Relative = $page.FullName.Substring($source.Length + 1); Content = $converted }
}

$catalog = [IO.File]::ReadAllText((Join-Path $repository 'src\GWGUI.App\Dictionaries\Localization\UiLanguageCatalog.cs'))
foreach ($language in [regex]::Matches($catalog, 'new\("([^"]+)",')) {
    $code = $language.Groups[1].Value
    if (-not (Test-Path -LiteralPath (Join-Path $source "$code\$code-Guide.md"))) { throw "Missing wiki language: $code" }
}

if ($output -ne (Join-Path $repository 'build\wiki')) { throw 'Unexpected wiki output directory.' }
if (Test-Path -LiteralPath $output) {
    if ((Get-Item -LiteralPath $output).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Wiki output must not be a symbolic link.' }
    Remove-Item -LiteralPath $output -Recurse -Force
}
New-Item -ItemType Directory -Path $output -Force | Out-Null
foreach ($page in $rendered) {
    $destination = Join-Path $output $page.Relative
    New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($destination)) -Force | Out-Null
    [IO.File]::WriteAllText($destination, $page.Content, $utf8)
}
Copy-Item -LiteralPath (Join-Path $source 'images') -Destination $output -Recurse
Write-Output "Wiki prepared: $($pages.Count) pages in $output"
