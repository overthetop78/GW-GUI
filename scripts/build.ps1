param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$buildRoot = Join-Path $repository 'build'
$configurations = if ([string]::IsNullOrWhiteSpace($Configuration)) { @('Debug', 'Release') } else { @($Configuration) }

function New-GwGuiBuild {
    param([string]$BuildConfiguration)

    $output = Join-Path $buildRoot "$BuildConfiguration\GW GUI"
    $staging = Join-Path $buildRoot ".staging\$BuildConfiguration"
    $applicationPublish = Join-Path $staging 'application'
    $moduleStaging = Join-Path $staging 'modules'

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
    $modules = @(
        @{ Project = 'src\GWGUI.Emulation.Amiga\GWGUI.Emulation.Amiga.csproj'; Assembly = 'gwgui.emulation.amiga'; Folder = 'Amiga' },
        @{ Project = 'src\GWGUI.Emulation.Atari\GWGUI.Emulation.Atari.csproj'; Assembly = 'gwgui.emulation.atari'; Folder = 'Atari' }
    )
    foreach ($module in $modules) {
        $project = Join-Path $repository $module.Project
        $manifestPath = Join-Path (Split-Path -Parent $project) 'module.json'
        $manifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
        if ($manifest.schemaVersion -ne 1 -or $manifest.id -ine $module.Folder -or
            $manifest.entryAssembly -ine ($module.Assembly + '.dll') -or
            $manifest.moduleVersion -notmatch '^\d+\.\d+\.\d+$' -or
            $manifest.hostApiMinimum -ne '1.0' -or $manifest.hostApiMaximum -ne '1.0') {
            throw "Invalid manifest for official module '$($module.Folder)': $manifestPath"
        }
        $destination = Join-Path $moduleOutput $module.Folder
        New-Item -ItemType Directory -Path $destination -Force | Out-Null
        $publish = Join-Path $moduleStaging $module.Assembly
        dotnet publish $project `
            -c $BuildConfiguration -r win-x64 --self-contained false -p:Version=$($manifest.moduleVersion) -o $publish --disable-build-servers
        if ($LASTEXITCODE -ne 0) { throw "$BuildConfiguration $($module.Assembly) module publish failed." }
        Copy-Item -LiteralPath (Join-Path $publish 'module.json') -Destination $destination -Force
        foreach ($extension in @('.dll', '.pdb')) {
            $source = Join-Path $publish ($module.Assembly + $extension)
            if (Test-Path -LiteralPath $source -PathType Leaf) {
                Copy-Item -LiteralPath $source -Destination $destination -Force
            } elseif ($extension -eq '.dll') {
                throw "Module entry assembly was not produced: $source"
            }
        }
    }

    dotnet publish (Join-Path $repository 'src\GWGUI.Launcher\GWGUI.Launcher.csproj') `
        -c $BuildConfiguration -r win-x64 --self-contained false -o $output --disable-build-servers
    if ($LASTEXITCODE -ne 0) { throw "$BuildConfiguration bootstrap publish failed." }

    Copy-Item -Path (Join-Path $applicationPublish '*') -Destination $output -Recurse -Force
    Remove-Item -LiteralPath (Join-Path $output 'gwgui.app.exe'),(Join-Path $output 'gwgui.app.runtimeconfig.json') -Force

    & (Join-Path $repository 'scripts\organize-application-output.ps1') -OutputDirectory $output
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
