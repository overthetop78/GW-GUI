param([string]$BuildDirectory = (Join-Path $PSScriptRoot '..\build\Debug\GW GUI'))

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($BuildDirectory).TrimEnd('\') + '\'
$matches = Get-Process | Where-Object {
    try {
        $path = $_.Path
        -not [string]::IsNullOrWhiteSpace($path) -and
            [IO.Path]::GetFullPath($path).StartsWith($root, [StringComparison]::OrdinalIgnoreCase)
    }
    catch { $false }
}
foreach ($match in $matches) {
    [pscustomobject]@{
        Id = $match.Id
        Name = $match.ProcessName
        Path = $match.Path
    }
    $null = $match.CloseMainWindow()
    if (-not $match.WaitForExit(3000)) { Stop-Process -Id $match.Id -Force }
}
