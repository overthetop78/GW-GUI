[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$ImagePath,

    [Parameter(Mandatory)]
    [string]$OutputDirectory,

    [Parameter(Mandatory)]
    [string]$CorePath,

    [Parameter(Mandatory)]
    [string]$ConfigurationPath,

    [Parameter(Mandatory)]
    [string]$DataDirectory,

    [string]$SwapImagePath,

    [int]$SwapFrame,

    [string[]]$Press = @()
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$projectPath = Join-Path $repositoryRoot 'tests\GWGUI.LocalDiskImageTests\TemporaryLibretroMediaReader\TemporaryLibretroMediaReader.csproj'
$assemblyPath = Join-Path $repositoryRoot 'tests\GWGUI.LocalDiskImageTests\TemporaryLibretroMediaReader\bin\TemporaryLibretroMediaReader\Debug\net10.0-windows10.0.19041.0\GWGUI.TemporaryLibretroMediaReader.dll'

function Invoke-DotNetProcess {
    param([Parameter(Mandatory)][string[]]$Arguments)

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = (Get-Command dotnet -CommandType Application).Source
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    foreach ($argument in $Arguments) {
        [void]$startInfo.ArgumentList.Add($argument)
    }

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $started = $false
    try {
        if (-not $process.Start()) { throw 'The dotnet process could not be started.' }
        $started = $true
        $standardOutput = $process.StandardOutput.ReadToEndAsync()
        $standardError = $process.StandardError.ReadToEndAsync()
        $process.WaitForExitAsync().GetAwaiter().GetResult()
        [pscustomobject]@{
            ExitCode = $process.ExitCode
            Output = $standardOutput.GetAwaiter().GetResult()
            Error = $standardError.GetAwaiter().GetResult()
        }
    }
    finally {
        if ($started) {
            try {
                if (-not $process.HasExited) {
                    $process.Kill($true)
                    $process.WaitForExit()
                }
            }
            catch [System.InvalidOperationException] {
                # The process exited between the check and cleanup.
            }
        }
        $process.Dispose()
    }
}

$build = Invoke-DotNetProcess -Arguments @(
    'build', $projectPath, '--configuration', 'Debug', '--disable-build-servers',
    '-p:UseSharedCompilation=false', '-nodeReuse:false'
)
if ($build.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $assemblyPath)) {
    $message = (@($build.Output, $build.Error) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) -join [Environment]::NewLine
    throw "Libretro media reader build failed.`n$message"
}

$arguments = [System.Collections.Generic.List[string]]::new()
$arguments.Add($assemblyPath)
$arguments.Add('--image')
$arguments.Add((Resolve-Path -LiteralPath $ImagePath).Path)
$arguments.Add('--output')
$arguments.Add([System.IO.Path]::GetFullPath($OutputDirectory))
$arguments.Add('--core')
$arguments.Add((Resolve-Path -LiteralPath $CorePath).Path)
$arguments.Add('--configuration')
$arguments.Add((Resolve-Path -LiteralPath $ConfigurationPath).Path)
$arguments.Add('--data-directory')
$arguments.Add((Resolve-Path -LiteralPath $DataDirectory).Path)
foreach ($value in $Press) {
    $arguments.Add("--press=$value")
}
if (-not [string]::IsNullOrWhiteSpace($SwapImagePath)) {
    if ($SwapFrame -le 0) { throw 'SwapFrame must be greater than zero when SwapImagePath is provided.' }
    $arguments.Add('--swap-image')
    $arguments.Add((Resolve-Path -LiteralPath $SwapImagePath).Path)
    $arguments.Add('--swap-frame')
    $arguments.Add($SwapFrame.ToString([Globalization.CultureInfo]::InvariantCulture))
}

$execution = Invoke-DotNetProcess -Arguments $arguments.ToArray()
if ($execution.ExitCode -ne 0) {
    $message = (@($execution.Output, $execution.Error) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) -join [Environment]::NewLine
    throw "Libretro media reader failed.`n$message"
}

$execution.Output.Trim()
