param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$buildRoot = Join-Path $repository 'build'
$configurations = if ([string]::IsNullOrWhiteSpace($Configuration)) { @('Debug', 'Release') } else { @($Configuration) }
. (Join-Path $PSScriptRoot 'emulation-modules.ps1')

function New-GwGuiBuild {
    param([string]$BuildConfiguration)

    $output = Join-Path $buildRoot "$BuildConfiguration\GW GUI"
    $staging = Join-Path $buildRoot ".staging\$BuildConfiguration"
    $applicationPublish = Join-Path $staging 'application'
    $moduleStaging = Join-Path $staging 'modules'
    $updaterPublish = Join-Path $staging 'updater'

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

    $moduleOutput = Join-Path $applicationPublish 'Modules'
    New-Item -ItemType Directory -Path $moduleOutput -Force | Out-Null
    $applicationFiles = @(Get-ChildItem -LiteralPath $applicationPublish -Recurse -File | Where-Object {
        -not $_.FullName.StartsWith($moduleOutput + [IO.Path]::DirectorySeparatorChar,
            [StringComparison]::OrdinalIgnoreCase)
    })
    $applicationHashes = @{}
    foreach ($file in $applicationFiles) {
        if (-not $applicationHashes.ContainsKey($file.Name)) {
            $applicationHashes[$file.Name] = [Collections.Generic.HashSet[string]]::new(
                [StringComparer]::OrdinalIgnoreCase)
        }
        $null = $applicationHashes[$file.Name].Add((Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash)
    }
    $modules = @(Get-GwGuiEmulationModules -RepositoryRoot $repository)
    $hostApiVersion = [Version]'1.0'
    foreach ($module in $modules) {
        $project = $module.ProjectPath
        $manifest = $module.Manifest
        if ($hostApiVersion -lt [Version]$manifest.hostApiMinimum `
            -or $hostApiVersion -gt [Version]$manifest.hostApiMaximum) {
            throw "Module '$($module.Id)' is not compatible with host API $hostApiVersion."
        }
        $destination = Join-Path $moduleOutput $module.Id
        New-Item -ItemType Directory -Path $destination -Force | Out-Null
        $publish = Join-Path $moduleStaging $module.AssemblyName
        dotnet publish $project `
            -c $BuildConfiguration -r win-x64 --self-contained false -p:Version=$($manifest.moduleVersion) -o $publish --disable-build-servers
        if ($LASTEXITCODE -ne 0) { throw "$BuildConfiguration $($module.AssemblyName) module publish failed." }
        $entryAssembly = Join-Path $publish $module.EntryAssembly
        if (-not (Test-Path -LiteralPath $entryAssembly -PathType Leaf)) {
            throw "Module entry assembly was not produced: $entryAssembly"
        }
        foreach ($source in Get-ChildItem -LiteralPath $publish -Recurse -File) {
            $isRequiredMetadata = $source.Name -ieq 'module.json' `
                -or $source.Name -ieq $module.EntryAssembly `
                -or $source.Name -ieq ($module.AssemblyName + '.pdb') `
                -or $source.Name -ieq ($module.AssemblyName + '.deps.json')
            if (-not $isRequiredMetadata -and $applicationHashes.ContainsKey($source.Name)) {
                $sourceHash = (Get-FileHash -LiteralPath $source.FullName -Algorithm SHA256).Hash
                if ($applicationHashes[$source.Name].Contains($sourceHash)) { continue }
            }
            $relativePath = $source.FullName.Substring($publish.Length).TrimStart('\', '/')
            $target = Join-Path $destination $relativePath
            $targetDirectory = Split-Path -Parent $target
            if (-not (Test-Path -LiteralPath $targetDirectory -PathType Container)) {
                New-Item -ItemType Directory -Path $targetDirectory -Force | Out-Null
            }
            Copy-Item -LiteralPath $source.FullName -Destination $target -Force
        }
    }

    dotnet publish (Join-Path $repository 'src\GWGUI.Launcher\GWGUI.Launcher.csproj') `
        -c $BuildConfiguration -r win-x64 --self-contained false -o $output --disable-build-servers
    if ($LASTEXITCODE -ne 0) { throw "$BuildConfiguration bootstrap publish failed." }

    Copy-Item -Path (Join-Path $applicationPublish '*') -Destination $output -Recurse -Force
    Remove-Item -LiteralPath (Join-Path $output 'gwgui.app.exe'),(Join-Path $output 'gwgui.app.runtimeconfig.json') -Force

    & (Join-Path $repository 'scripts\organize-application-output.ps1') -OutputDirectory $output
    dotnet publish (Join-Path $repository 'src\GWGUI.Updater\GWGUI.Updater.csproj') `
        -c $BuildConfiguration -r win-x64 --self-contained false -o $updaterPublish --disable-build-servers
    if ($LASTEXITCODE -ne 0) { throw "$BuildConfiguration updater publish failed." }
    $updaterOutput = Join-Path $output 'Updater'
    New-Item -ItemType Directory -Path $updaterOutput -Force | Out-Null
    Copy-Item -Path (Join-Path $updaterPublish '*') -Destination $updaterOutput -Recurse -Force
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
