param(
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$project = Join-Path $repository 'src\GWGUI.App\GWGUI.App.csproj'
$output = Join-Path $repository 'artifacts\build\GW GUI'

if (Test-Path -LiteralPath $output) { Remove-Item -LiteralPath $output -Recurse -Force }
New-Item -ItemType Directory -Path $output -Force | Out-Null

dotnet build $project -c $Configuration -o $output
if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed.' }

$executable = Join-Path $output 'GW GUI.exe'
if (-not (Test-Path -LiteralPath $executable)) { throw 'The application executable was not produced.' }
Write-Output "Build ready: $executable"
