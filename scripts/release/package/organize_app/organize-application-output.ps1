param(
    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory,
    [string]$ApplicationAssemblyName = 'gwgui.app',
    [string]$RootAssemblyName = 'gwgui'
)

$ErrorActionPreference = 'Stop'
$output = [IO.Path]::GetFullPath($OutputDirectory)
if (-not (Test-Path -LiteralPath $output -PathType Container)) {
    throw "Application output directory does not exist: $output"
}

$dependencyManifests = @(Get-ChildItem -LiteralPath $output -File -Filter '*.deps.json')
if ($dependencyManifests.Count -eq 0) {
    throw "GW GUI dependency manifests are missing from: $output"
}

function Update-DependencyPath {
    param([string]$FileName, [string]$PackagedPath)

    foreach ($manifest in $dependencyManifests) {
        $content = [IO.File]::ReadAllText($manifest.FullName)
        $pattern = '"(?:[^"\r\n]*/)?' + [regex]::Escape($FileName) + '"(?=\s*:)'
        $updated = [regex]::Replace($content, $pattern, ('"' + $PackagedPath + '"'))
        if ($updated -ne $content) {
            [IO.File]::WriteAllText($manifest.FullName, $updated)
        }
    }
}

$languageDirectories = @(Get-ChildItem -LiteralPath $output -Directory | Where-Object {
    Test-Path -LiteralPath (Join-Path $_.FullName "$ApplicationAssemblyName.resources.dll") -PathType Leaf
})
if ($languageDirectories.Count -gt 0) {
    $languages = Join-Path $output 'Languages'
    New-Item -ItemType Directory -Path $languages -Force | Out-Null
    foreach ($languageDirectory in $languageDirectories) {
        $resourceFileName = "$ApplicationAssemblyName.resources.dll"
        $resource = Join-Path $languageDirectory.FullName $resourceFileName
        $publishedPath = "$($languageDirectory.Name)/$resourceFileName"
        $packagedPath = "Languages/$($languageDirectory.Name).dll"
        foreach ($manifest in $dependencyManifests) {
            $content = [IO.File]::ReadAllText($manifest.FullName)
            $updated = $content.Replace(('"' + $publishedPath + '"'), ('"' + $packagedPath + '"'))
            if ($updated -ne $content) {
                [IO.File]::WriteAllText($manifest.FullName, $updated)
            }
        }
        Move-Item -LiteralPath $resource -Destination (Join-Path $languages "$($languageDirectory.Name).dll") -Force
        Remove-Item -LiteralPath $languageDirectory.FullName -Recurse -Force
    }
}

$libraries = Join-Path $output 'lib'
$applicationLibraries = @(Get-ChildItem -LiteralPath $output -File -Filter '*.dll' | Where-Object {
    $_.Name -ne "$RootAssemblyName.dll"
})
if ($applicationLibraries.Count -gt 0) {
    New-Item -ItemType Directory -Path $libraries -Force | Out-Null
    foreach ($library in $applicationLibraries) {
        $category = switch -Regex ($library.Name) {
            '^gwgui\.' { $null; break }
            '^(SkiaSharp|libSkiaSharp)' { 'SkiaSharp'; break }
            '^NAudio' { 'NAudio'; break }
            '^Newtonsoft\.Json' { 'Newtonsoft.Json'; break }
            '^(OpenTK|glfw3)' { 'OpenTK'; break }
            '^(Veldrid|libveldrid|vk\.dll|NativeLibraryLoader)' { 'Veldrid'; break }
            '^(Vortice|SharpGen)' { 'Vortice'; break }
            '^GLWpfControl' { 'GLWpfControl'; break }
            '^(Microsoft\.|WinRT\.)' { 'Microsoft'; break }
            default { [IO.Path]::GetFileNameWithoutExtension($library.Name) }
        }
        $destinationDirectory = if ($null -eq $category) { $libraries } else { Join-Path $libraries $category }
        New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null
        $packagedPath = if ($null -eq $category) { "lib/$($library.Name)" } else { "lib/$category/$($library.Name)" }
        Update-DependencyPath $library.Name $packagedPath
        Move-Item -LiteralPath $library.FullName -Destination (Join-Path $destinationDirectory $library.Name) -Force

        $debugSymbols = Join-Path $output "$($library.BaseName).pdb"
        if (Test-Path -LiteralPath $debugSymbols -PathType Leaf) {
            Move-Item -LiteralPath $debugSymbols -Destination (Join-Path $destinationDirectory "$($library.BaseName).pdb") -Force
        }
    }
}

$applicationDependencyManifest = Join-Path $output "$ApplicationAssemblyName.deps.json"
if (Test-Path -LiteralPath $applicationDependencyManifest -PathType Leaf) {
    Move-Item -LiteralPath $applicationDependencyManifest -Destination (Join-Path $libraries "$ApplicationAssemblyName.deps.json") -Force
}
