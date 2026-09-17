[CmdletBinding()]
param(
    [string]$Root = 'F:\Rétro',
    [string]$OutputRoot,
    [string]$StartAt = 'F:\Rétro\A Trier\Atari 400-800\Atari 8bit - Applications - [ATR] (TOSEC-v2023-08-29)\8bit Mouse, The v2.01 (19xx)(Broomfield, Graham - Hunt, Colin)',
    [string]$ImagePath,
    [ValidateRange(1, [int]::MaxValue)]
    [int]$MaximumCount,
    [switch]$Restart
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if (-not $OutputRoot) {
    $OutputRoot = Join-Path $repositoryRoot 'artifacts\media-audit'
}
$auditProject = Join-Path $repositoryRoot 'tests\GWGUI.LocalDiskImageTests\GWGUI.LocalDiskImageTests.csproj'
$auditAssembly = Join-Path $repositoryRoot 'tests\GWGUI.LocalDiskImageTests\bin\Debug\net10.0-windows10.0.19041.0\GWGUI.LocalDiskImageTests.dll'
$checkpointPath = Join-Path $OutputRoot 'checkpoint.json'
$failurePath = Join-Path $OutputRoot 'failure.json'

function Write-JsonAtomic {
    param([Parameter(Mandatory)]$Value, [Parameter(Mandatory)][string]$Path)

    $temporaryPath = "$Path.tmp"
    $json = $Value | ConvertTo-Json -Depth 20
    [System.IO.File]::WriteAllText($temporaryPath, $json, [System.Text.UTF8Encoding]::new($false))
    Move-Item -LiteralPath $temporaryPath -Destination $Path -Force
}

function Get-PathIdentifier {
    param([Parameter(Mandatory)][string]$Path)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Path.ToUpperInvariant())
    $algorithm = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hash = $algorithm.ComputeHash($bytes)
    } finally {
        $algorithm.Dispose()
    }
    return ([BitConverter]::ToString($hash) -replace '-', '').Substring(0, 16).ToLowerInvariant()
}

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
        if (-not $process.Start()) {
            throw "Le processus dotnet n'a pas pu démarrer."
        }
        $started = $true
        $standardOutput = $process.StandardOutput.ReadToEndAsync()
        $standardError = $process.StandardError.ReadToEndAsync()
        $process.WaitForExitAsync().GetAwaiter().GetResult()
        $output = $standardOutput.GetAwaiter().GetResult()
        $errorOutput = $standardError.GetAwaiter().GetResult()
        [pscustomobject]@{
            ExitCode = $process.ExitCode
            Output = $output
            Error = $errorOutput
        }
    } finally {
        if ($started) {
            try {
                if (-not $process.HasExited) {
                    $process.Kill($true)
                    $process.WaitForExit()
                }
            } catch [System.InvalidOperationException] {
                # Le processus s'est terminé entre le contrôle et la libération.
            }
        }
        $process.Dispose()
    }
}

function Invoke-MediaValidator {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Destination
    )

    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    foreach ($staleName in @('failure.json', 'execution.log')) {
        $stalePath = Join-Path $Destination $staleName
        if (Test-Path -LiteralPath $stalePath) { Remove-Item -LiteralPath $stalePath -Force }
    }
    Get-ChildItem -LiteralPath $Destination -File -Filter 'failed-source.*' -ErrorAction SilentlyContinue |
        Remove-Item -Force
    $result = Invoke-DotNetProcess -Arguments @($auditAssembly, '--image', $Path, '--output', $Destination)
    $message = (@($result.Output, $result.Error) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) -join [Environment]::NewLine
    $message = $message.Trim()
    if ($result.ExitCode -ne 0) {
        if (-not $message) { $message = "Le validateur s'est arrêté avec le code $($result.ExitCode)." }
        [System.IO.File]::WriteAllText((Join-Path $Destination 'execution.log'), $message, [System.Text.UTF8Encoding]::new($false))
        Copy-Item -LiteralPath $Path -Destination (Join-Path $Destination ("failed-source" + [System.IO.Path]::GetExtension($Path))) -Force
        throw [System.IO.InvalidDataException]::new($message)
    }
}

New-Item -ItemType Directory -Path $OutputRoot -Force | Out-Null

$buildResult = Invoke-DotNetProcess -Arguments @(
    'build', $auditProject, '--configuration', 'Debug', '--no-restore', '--disable-build-servers',
    '-p:UseSharedCompilation=false', '-nodeReuse:false'
)
if ($buildResult.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $auditAssembly)) {
    throw "La compilation de l'outil d'audit a échoué."
}

if ($ImagePath) {
    $resolvedImage = (Resolve-Path -LiteralPath $ImagePath).Path
    $singleDestination = Join-Path (Join-Path $OutputRoot 'single-tests') (Get-PathIdentifier $resolvedImage)
    Invoke-MediaValidator -Path $resolvedImage -Destination $singleDestination
    Write-Output "Audit terminé : $resolvedImage"
    exit 0
}

$resolvedRoot = (Resolve-Path -LiteralPath $Root).Path
$candidateResult = Invoke-DotNetProcess -Arguments @($auditAssembly, '--list-images', $resolvedRoot)
if ($candidateResult.ExitCode -ne 0) {
    throw "L'énumération des images du corpus a échoué."
}
$candidateJson = $candidateResult.Output
$candidates = @(($candidateJson | ConvertFrom-Json) | ForEach-Object { [System.IO.FileInfo]::new([string]$_) })

$effectiveStartAt = $null
if ($StartAt) {
    $resolvedStart = [System.IO.Path]::GetFullPath($StartAt)
    $rootPrefix = $resolvedRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    if ($resolvedStart -ieq $resolvedRoot -or $resolvedStart.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        $effectiveStartAt = $resolvedStart
        $firstIncludedIndex = 0
        while ($firstIncludedIndex -lt $candidates.Count -and [string]::Compare($candidates[$firstIncludedIndex].FullName, $resolvedStart, $true) -lt 0) {
            $firstIncludedIndex++
        }
        if ($firstIncludedIndex -ge $candidates.Count) {
            $candidates = @()
        } elseif ($firstIncludedIndex -gt 0) {
            $candidates = @($candidates[$firstIncludedIndex..($candidates.Count - 1)])
        }
    }
}

if ($MaximumCount -gt 0 -and $candidates.Count -gt $MaximumCount) {
    $candidates = @($candidates[0..($MaximumCount - 1)])
}

$startIndex = 0
if (-not $Restart -and (Test-Path -LiteralPath $checkpointPath)) {
    $checkpoint = Get-Content -LiteralPath $checkpointPath -Raw | ConvertFrom-Json
    $sameScope = $checkpoint.root -and ([string]$checkpoint.root -ieq $resolvedRoot) -and
        ([string]$checkpoint.startAt -ieq [string]$effectiveStartAt)
    if ($sameScope -and $checkpoint.status -eq 'failed' -and $checkpoint.failedPath) {
        $failedCandidateFound = $false
        for ($candidateIndex = 0; $candidateIndex -lt $candidates.Count; $candidateIndex++) {
            if ($candidates[$candidateIndex].FullName -ieq [string]$checkpoint.failedPath) {
                $startIndex = $candidateIndex
                $failedCandidateFound = $true
                break
            }
        }
        if (-not $failedCandidateFound) {
            while ($startIndex -lt $candidates.Count -and
                [string]::Compare($candidates[$startIndex].FullName, [string]$checkpoint.failedPath, $true) -lt 0) {
                $startIndex++
            }
        }
    } elseif ($sameScope -and ($checkpoint.nextIndex -is [long] -or $checkpoint.nextIndex -is [int])) {
        $startIndex = [int]$checkpoint.nextIndex
    }
}

for ($index = $startIndex; $index -lt $candidates.Count; $index++) {
    $candidate = $candidates[$index]
    $destination = Join-Path (Join-Path $OutputRoot 'items') (('{0:D8}-' -f $index) + (Get-PathIdentifier $candidate.FullName))
    try {
        Invoke-MediaValidator -Path $candidate.FullName -Destination $destination
        if (Test-Path -LiteralPath $failurePath) { Remove-Item -LiteralPath $failurePath -Force }
        Write-JsonAtomic -Path $checkpointPath -Value ([ordered]@{
            status = 'running'
            root = $resolvedRoot
            startAt = $effectiveStartAt
            total = $candidates.Count
            lastSuccessfulPath = $candidate.FullName
            nextIndex = $index + 1
            updatedAt = [DateTimeOffset]::UtcNow
        })
    } catch {
        $failure = [ordered]@{
            status = 'failed'
            root = $resolvedRoot
            startAt = $effectiveStartAt
            total = $candidates.Count
            index = $index
            failedPath = $candidate.FullName
            outputDirectory = $destination
            error = $_.Exception.Message
            failedAt = [DateTimeOffset]::UtcNow
        }
        Write-JsonAtomic -Path $checkpointPath -Value $failure
        Write-JsonAtomic -Path $failurePath -Value $failure
        Write-Error "Échec de l'audit : $($candidate.FullName)`n$($_.Exception.Message)"
        exit 1
    }
}

Write-JsonAtomic -Path $checkpointPath -Value ([ordered]@{
    status = 'complete'
    root = $resolvedRoot
    startAt = $effectiveStartAt
    total = $candidates.Count
    nextIndex = $candidates.Count
    completedAt = [DateTimeOffset]::UtcNow
})
if (Test-Path -LiteralPath $failurePath) {
    Remove-Item -LiteralPath $failurePath -Force
}
Write-Output "Audit terminé : $($candidates.Count) média(s)."
