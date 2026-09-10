param()

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$docsRoot = [IO.Path]::GetFullPath((Join-Path $repository 'docs'))
$indexPath = [IO.Path]::GetFullPath((Join-Path $docsRoot 'README.md'))
$docsRootUri = [Uri]($docsRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar)
$markdownFiles = @(Get-ChildItem -LiteralPath $docsRoot -Recurse -File -Filter '*.md')
$filesByPath = @{}
$linksByPath = @{}
$brokenLinks = [Collections.Generic.List[string]]::new()

function Get-DocsRelativePath([string]$path) {
    $targetUri = [Uri]([IO.Path]::GetFullPath($path))
    return [Uri]::UnescapeDataString($docsRootUri.MakeRelativeUri($targetUri).ToString()).Replace('/', [IO.Path]::DirectorySeparatorChar)
}

foreach ($file in $markdownFiles) {
    $fullPath = [IO.Path]::GetFullPath($file.FullName)
    $filesByPath[$fullPath.ToLowerInvariant()] = $fullPath
    $linksByPath[$fullPath.ToLowerInvariant()] = [Collections.Generic.List[string]]::new()
}

foreach ($file in $markdownFiles) {
    $sourcePath = [IO.Path]::GetFullPath($file.FullName)
    $sourceKey = $sourcePath.ToLowerInvariant()
    $content = [IO.File]::ReadAllText($sourcePath)
    $matches = [regex]::Matches($content, '!?(?:\[[^\]]*\])\((?<target>[^)]+)\)')

    foreach ($match in $matches) {
        $target = $match.Groups['target'].Value.Trim()
        if ($target.StartsWith('<') -and $target.EndsWith('>')) {
            $target = $target.Substring(1, $target.Length - 2)
        }
        if ($target -match '^[a-zA-Z][a-zA-Z0-9+.-]*:' -or $target.StartsWith('#')) {
            continue
        }

        $targetWithoutAnchor = ($target -split '#', 2)[0]
        if ([string]::IsNullOrWhiteSpace($targetWithoutAnchor)) {
            continue
        }

        try {
            $decodedTarget = [Uri]::UnescapeDataString($targetWithoutAnchor)
            $resolvedTarget = [IO.Path]::GetFullPath((Join-Path $file.DirectoryName ($decodedTarget -replace '/', '\')))
        }
        catch {
            $relativeSource = Get-DocsRelativePath $sourcePath
            $brokenLinks.Add("$relativeSource -> $target (chemin invalide)")
            continue
        }

        if (-not (Test-Path -LiteralPath $resolvedTarget)) {
            $relativeSource = Get-DocsRelativePath $sourcePath
            $brokenLinks.Add("$relativeSource -> $target")
            continue
        }

        $targetKey = $resolvedTarget.ToLowerInvariant()
        if ($filesByPath.ContainsKey($targetKey)) {
            $linksByPath[$sourceKey].Add($targetKey)
        }
    }
}

$reachable = @{}
$pending = [Collections.Generic.Queue[string]]::new()
$pending.Enqueue($indexPath.ToLowerInvariant())
while ($pending.Count -gt 0) {
    $current = $pending.Dequeue()
    if ($reachable.ContainsKey($current)) {
        continue
    }
    $reachable[$current] = $true
    foreach ($target in $linksByPath[$current]) {
        if (-not $reachable.ContainsKey($target)) {
            $pending.Enqueue($target)
        }
    }
}

$unindexed = @(
    $markdownFiles |
        Where-Object { -not $reachable.ContainsKey(([IO.Path]::GetFullPath($_.FullName)).ToLowerInvariant()) } |
        ForEach-Object { Get-DocsRelativePath $_.FullName } |
        Sort-Object
)
$finishedTaskFiles = @(
    Get-ChildItem -LiteralPath (Join-Path $docsRoot 'tasks') -Recurse -File -Filter '*.md' |
        Where-Object { $_.Name -ne 'README.md' } |
        Where-Object { [IO.File]::ReadAllText($_.FullName) -notmatch '(?m)^\s*- \[ \]' } |
        ForEach-Object { Get-DocsRelativePath $_.FullName } |
        Sort-Object
)

$hasErrors = $false
if ($brokenLinks.Count -gt 0) {
    $hasErrors = $true
    Write-Error ("Liens Markdown locaux cassés :`n- " + (($brokenLinks | Sort-Object -Unique) -join "`n- ")) -ErrorAction Continue
}
if ($unindexed.Count -gt 0) {
    $hasErrors = $true
    Write-Error ("Documents absents de l'index :`n- " + ($unindexed -join "`n- ")) -ErrorAction Continue
}
if ($finishedTaskFiles.Count -gt 0) {
    $hasErrors = $true
    Write-Error ("Feuilles de tâches sans case ouverte :`n- " + ($finishedTaskFiles -join "`n- ")) -ErrorAction Continue
}
if ($hasErrors) {
    exit 1
}

Write-Host "Documentation valide : $($markdownFiles.Count) fichiers Markdown, aucun lien local cassé, aucun document non indexé et aucune feuille terminée conservée."
