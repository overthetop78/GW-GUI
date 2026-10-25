function Invoke-DotNetProcess {
    param(
        [Parameter(Mandatory)][string[]]$Arguments,
        [string]$StartFailureMessage = 'The dotnet process could not be started.'
    )

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
    $started = $false
    try {
        $process.StartInfo = $startInfo
        if (-not $process.Start()) { throw $StartFailureMessage }
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
        try {
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
        }
        finally {
            $process.Dispose()
        }
    }
}
