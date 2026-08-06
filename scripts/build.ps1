param(
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$solution = Join-Path $repository 'GWGUI.sln'

dotnet build $solution -c $Configuration
if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed.' }

$outputRoot = Join-Path $repository "src\GWGUI.App\bin\$Configuration"
$executable = Get-ChildItem -LiteralPath $outputRoot -Recurse -File -Filter 'GW GUI.exe' |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1
if ($executable) { Write-Output "Build ready: $($executable.FullName)" }
