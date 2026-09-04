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
