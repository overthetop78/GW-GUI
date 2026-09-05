param()

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$built = Join-Path $repository 'build\wiki'
& (Join-Path $PSScriptRoot 'build-wiki.ps1')
if (-not (Get-Command git -ErrorAction SilentlyContinue)) { throw 'Git is required.' }

# Use a fresh checkout so local files can never be mistaken for old wiki pages.
$checkout = Join-Path $repository ('build\wiki-publish-' + [Guid]::NewGuid().ToString('N'))
git clone 'https://github.com/overthetop78/GW-GUI.wiki.git' $checkout
if ($LASTEXITCODE -ne 0) { throw 'Wiki clone failed. Enable the wiki and create its initial page on GitHub before the first publication.' }
try {
    # Only files recorded by our previous publication may be removed.
    $manifestPath = Join-Path $checkout '.gwgui-wiki-files.json'
    if (Test-Path -LiteralPath $manifestPath) {
        $previousFiles = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
        foreach ($relative in $previousFiles) {
            $target = [IO.Path]::GetFullPath((Join-Path $checkout $relative))
            if (-not $target.StartsWith($checkout + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -or $relative -match '(^|[/\\])\.git([/\\]|$)') { throw "Invalid wiki manifest entry: $relative" }
            if (Test-Path -LiteralPath $target -PathType Leaf) { Remove-Item -LiteralPath $target }
        }
    }
    $files = @(Get-ChildItem -LiteralPath $built -Recurse -File)
    $manifest = @()
    foreach ($file in $files) {
        $relative = $file.FullName.Substring($built.Length + 1)
        $destination = Join-Path $checkout $relative
        New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($destination)) -Force | Out-Null
        Copy-Item -LiteralPath $file.FullName -Destination $destination -Force
        $manifest += $relative.Replace('\', '/')
    }
    $manifest | ConvertTo-Json | Set-Content -LiteralPath $manifestPath -Encoding UTF8
    git -C $checkout add --all
    if ($LASTEXITCODE -ne 0) { throw 'Could not stage wiki pages.' }
    git -C $checkout diff --cached --quiet
    if ($LASTEXITCODE -eq 0) { Write-Output 'Wiki is already up to date.'; return }
    if ($LASTEXITCODE -ne 1) { throw 'Could not inspect wiki changes.' }
    git -C $checkout commit -m 'Update GW GUI wiki'
    if ($LASTEXITCODE -ne 0) { throw 'Could not commit wiki pages.' }
    git -C $checkout push
    if ($LASTEXITCODE -ne 0) { throw 'Wiki publication failed.' }
    Write-Output 'Wiki published: https://github.com/overthetop78/GW-GUI/wiki'
}
finally {
    $expectedParent = [IO.Path]::GetFullPath((Join-Path $repository 'build'))
    if ([IO.Path]::GetDirectoryName($checkout) -ne $expectedParent -or [IO.Path]::GetFileName($checkout) -notmatch '^wiki-publish-[0-9a-f]{32}$') { throw 'Unexpected wiki checkout path.' }
    Remove-Item -LiteralPath $checkout -Recurse -Force
}
