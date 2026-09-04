param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$buildRoot = Join-Path $repository 'build'
$configurations = if ([string]::IsNullOrWhiteSpace($Configuration)) { @('Debug', 'Release') } else { @($Configuration) }

function Update-DependencyPath {
    param([IO.FileInfo[]]$Manifests, [string]$FileName, [string]$PackagedPath)

    foreach ($manifest in $Manifests) {
        $content = [IO.File]::ReadAllText($manifest.FullName)
        $pattern = '"(?:[^"\r\n]*/)?' + [regex]::Escape($FileName) + '"(?=\s*:)'
        $updated = [regex]::Replace($content, $pattern, ('"' + $PackagedPath + '"'))
        if ($updated -ne $content) { [IO.File]::WriteAllText($manifest.FullName, $updated) }
    }
}

function New-GwGuiBuild {
    param([string]$BuildConfiguration)

    $output = Join-Path $buildRoot "$BuildConfiguration\GW GUI"
    $staging = Join-Path $buildRoot ".staging\$BuildConfiguration"
    $applicationPublish = Join-Path $staging 'application'

    $runningExecutable = Join-Path $output 'gwgui.exe'
    if (Test-Path -LiteralPath $runningExecutable -PathType Leaf) {
        $resolvedExecutable = [IO.Path]::GetFullPath($runningExecutable)
        $runningProcesses = @(Get-Process -ErrorAction SilentlyContinue | Where-Object {
            try { [string]::Equals($_.Path, $resolvedExecutable, [StringComparison]::OrdinalIgnoreCase) }
            catch { $false }
        })
        foreach ($process in $runningProcesses) {
            $null = $process.CloseMainWindow()
            if (-not $process.WaitForExit(3000)) {
                Stop-Process -Id $process.Id -Force
                $null = $process.WaitForExit(3000)
            }
        }
        $stillRunning = @(Get-Process -ErrorAction SilentlyContinue | Where-Object {
            try { [string]::Equals($_.Path, $resolvedExecutable, [StringComparison]::OrdinalIgnoreCase) }
            catch { $false }
        })
        if ($stillRunning.Count -gt 0) {
            $identifiers = $stillRunning.Id -join ', '
            throw "GW GUI Debug is still running (PID: $identifiers). Close it before rebuilding."
        }
    }

    foreach ($target in @($output, $staging)) {
        if (Test-Path -LiteralPath $target) { Remove-Item -LiteralPath $target -Recurse -Force }
    }
    New-Item -ItemType Directory -Path $output,$applicationPublish -Force | Out-Null

    dotnet publish (Join-Path $repository 'src\GWGUI.App\GWGUI.App.csproj') `
        -c $BuildConfiguration -r win-x64 --self-contained false -o $applicationPublish --disable-build-servers
    if ($LASTEXITCODE -ne 0) { throw "$BuildConfiguration application publish failed." }

    dotnet publish (Join-Path $repository 'src\GWGUI.Launcher\GWGUI.Launcher.csproj') `
        -c $BuildConfiguration -r win-x64 --self-contained false -o $output --disable-build-servers
    if ($LASTEXITCODE -ne 0) { throw "$BuildConfiguration launcher publish failed." }

    Copy-Item -Path (Join-Path $applicationPublish '*') -Destination $output -Recurse -Force
    Remove-Item -LiteralPath (Join-Path $output 'gwgui.app.exe'),(Join-Path $output 'gwgui.app.runtimeconfig.json') -Force

    $languageDirectories = @(Get-ChildItem -LiteralPath $output -Directory | Where-Object {
        @(Get-ChildItem -LiteralPath $_.FullName -File -Filter '*.resources.dll').Count -gt 0
    })
    if ($languageDirectories.Count -gt 0) {
        $languages = Join-Path $output 'Languages'
        New-Item -ItemType Directory -Path $languages -Force | Out-Null
        $dependencyManifests = @(Get-ChildItem -LiteralPath $output -File -Filter '*.deps.json')
        foreach ($languageDirectory in $languageDirectories) {
            $resource = Join-Path $languageDirectory.FullName 'gwgui.app.resources.dll'
            if (-not (Test-Path -LiteralPath $resource -PathType Leaf)) {
                throw "GW GUI satellite resource is missing for '$($languageDirectory.Name)'."
            }
            $publishedPath = "$($languageDirectory.Name)/gwgui.app.resources.dll"
            $packagedPath = "Languages/$($languageDirectory.Name).dll"
            foreach ($manifest in $dependencyManifests) {
                $content = [IO.File]::ReadAllText($manifest.FullName)
                $updated = $content.Replace(('"' + $publishedPath + '"'), ('"' + $packagedPath + '"'))
                if ($updated -ne $content) { [IO.File]::WriteAllText($manifest.FullName, $updated) }
            }
            Move-Item -LiteralPath $resource -Destination (Join-Path $languages "$($languageDirectory.Name).dll")
            Remove-Item -LiteralPath $languageDirectory.FullName -Recurse -Force
        }
    }

    $libraries = Join-Path $output 'lib'
    $applicationLibraries = @(Get-ChildItem -LiteralPath $output -File -Filter '*.dll' | Where-Object {
        $_.Name -ne 'gwgui.dll'
    })
    if ($applicationLibraries.Count -gt 0) {
        New-Item -ItemType Directory -Path $libraries -Force | Out-Null
        $dependencyManifests = @(Get-ChildItem -LiteralPath $output -File -Filter '*.deps.json')
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
            Update-DependencyPath $dependencyManifests $library.Name $packagedPath
            Move-Item -LiteralPath $library.FullName -Destination (Join-Path $destinationDirectory $library.Name)
        }
    }

    $applicationDependencyManifest = Join-Path $output 'gwgui.app.deps.json'
    if (Test-Path -LiteralPath $applicationDependencyManifest) {
        Move-Item -LiteralPath $applicationDependencyManifest -Destination (Join-Path $libraries 'gwgui.app.deps.json')
    }

    Remove-Item -LiteralPath $staging -Recurse -Force

    $executable = Join-Path $output 'gwgui.exe'
    if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) {
        throw "$BuildConfiguration executable was not produced: $executable"
    }
    Write-Output "$BuildConfiguration build ready: $executable"
}

foreach ($buildConfiguration in $configurations) {
    New-GwGuiBuild $buildConfiguration
}

$stagingRoot = Join-Path $buildRoot '.staging'
if (Test-Path -LiteralPath $stagingRoot) { Remove-Item -LiteralPath $stagingRoot -Recurse -Force }
