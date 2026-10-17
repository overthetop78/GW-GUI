[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$ImagePath,

    [Parameter(Mandatory)]
    [string]$OutputDirectory,

    [Parameter(Mandatory)]
    [string[]]$EntryName
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$projectPath = Join-Path $repositoryRoot 'tests\GWGUI.LocalDiskImageTests\MediaFileExtractor\GWGUI.MediaFileExtractor.csproj'
$assemblyPath = Join-Path $repositoryRoot 'tests\GWGUI.LocalDiskImageTests\MediaFileExtractor\bin\Debug\net10.0-windows10.0.19041.0\GWGUI.MediaFileExtractor.dll'
$resolvedImagePath = (Resolve-Path -LiteralPath $ImagePath).Path
$resolvedOutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)

. (Join-Path $PSScriptRoot 'common/DotNetProcess.ps1')

$build = Invoke-DotNetProcess -Arguments @(
    'build', $projectPath, '--configuration', 'Debug', '--disable-build-servers',
    '-p:UseSharedCompilation=false', '-nodeReuse:false'
)
if ($build.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $assemblyPath)) {
    $message = (@($build.Output, $build.Error) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) -join [Environment]::NewLine
    throw "Media file extractor build failed.`n$message"
}

[System.IO.Directory]::CreateDirectory($resolvedOutputDirectory) | Out-Null
$arguments = [System.Collections.Generic.List[string]]::new()
$arguments.Add($assemblyPath)
$arguments.Add('--image')
$arguments.Add($resolvedImagePath)
$arguments.Add('--output')
$arguments.Add($resolvedOutputDirectory)
foreach ($name in $EntryName) {
    $arguments.Add('--entry')
    $arguments.Add($name)
}

$extraction = Invoke-DotNetProcess -Arguments $arguments.ToArray()
if ($extraction.ExitCode -ne 0) {
    $message = (@($extraction.Output, $extraction.Error) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) -join [Environment]::NewLine
    throw "Media file extraction failed.`n$message"
}

$extraction.Output.Trim()
