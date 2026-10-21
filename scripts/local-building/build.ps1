param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration,
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version,
    [switch]$AllModules,
    [string[]]$Module
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$buildRoot = Join-Path $repository 'build'
$applicationProject = Join-Path $repository 'src\GWGUI.App\GWGUI.App.csproj'
$hostApiSource = Join-Path $repository 'src\GWGUI.Emulation\Constants\EmulationHostApi.cs'
$hostApiDeclaration = [regex]::Match((Get-Content -LiteralPath $hostApiSource -Raw),
    'CurrentVersion\s*\{[^}]+\}\s*=\s*new(?:\s+Version)?\s*\(\s*(?<major>\d+)\s*,\s*(?<minor>\d+)\s*\)')
if (-not $hostApiDeclaration.Success) {
    throw "Cannot read the host API version from '$hostApiSource'."
}
$hostApiVersion = [Version]::new([int]$hostApiDeclaration.Groups['major'].Value, [int]$hostApiDeclaration.Groups['minor'].Value)
if ([string]::IsNullOrWhiteSpace($Version)) {
    [xml]$applicationProjectDocument = Get-Content -LiteralPath $applicationProject -Raw -Encoding UTF8
    $Version = [string]$applicationProjectDocument.Project.PropertyGroup.Version.'#text'
    if ([string]::IsNullOrWhiteSpace($Version)) {
        $Version = [string]$applicationProjectDocument.Project.PropertyGroup.Version
    }
}
if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    throw 'The application build version must use X.Y.Z.'
}
$configurations = if ([string]::IsNullOrWhiteSpace($Configuration)) { @('Debug', 'Release') } else { @($Configuration) }
. (Join-Path $repository 'scripts\release\update-catalog\emulation-modules\emulation-modules.ps1')
. (Join-Path $repository 'scripts\release\package\organize_app\shared-component-files.ps1')

$moduleWasSpecified = $PSBoundParameters.ContainsKey('Module')
if ($AllModules -and $moduleWasSpecified) {
    throw 'Use either --AllModules or --Module, not both.'
}

$selectedModules = @()
if ($AllModules -or $moduleWasSpecified) {
    $availableModules = @(Get-GwGuiEmulationModules -RepositoryRoot $repository)
    if ($AllModules) {
        $selectedModules = $availableModules
    }
    else {
        $selectedIdentifiers = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        foreach ($moduleArgument in $Module) {
            foreach ($requestedModule in @($moduleArgument -split ',')) {
                $requestedModule = $requestedModule.Trim()
                if ([string]::IsNullOrWhiteSpace($requestedModule)) {
                    throw 'Module identifiers cannot be empty.'
                }
                $resolvedModule = Resolve-GwGuiEmulationModule -Modules $availableModules -Module $requestedModule
                if (-not $selectedIdentifiers.Add($resolvedModule.Id)) {
                    throw "Module '$($resolvedModule.Id)' was selected more than once."
                }
                $selectedModules += $resolvedModule
            }
        }
    }
}

function New-GwGuiBuild {
    param(
        [string]$BuildConfiguration,
        [object[]]$SelectedModules
    )

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

    dotnet publish $applicationProject `
        -c $BuildConfiguration -r win-x64 --self-contained false -p:Version=$Version -o $applicationPublish --disable-build-servers
    if ($LASTEXITCODE -ne 0) { throw "$BuildConfiguration application publish failed." }

    $moduleOutput = Join-Path $applicationPublish 'Modules'
    New-Item -ItemType Directory -Path $moduleOutput -Force | Out-Null
    $applicationFiles = @(Get-ChildItem -LiteralPath $applicationPublish -Recurse -File | Where-Object {
        -not $_.FullName.StartsWith($moduleOutput + [IO.Path]::DirectorySeparatorChar,
            [StringComparison]::OrdinalIgnoreCase)
    })
    $applicationHashes = Get-GwGuiFileHashes -Files $applicationFiles
    # Example accompagne toujours l’application, même avec --modules=0. Il utilise le contrat
    # de fabrique normal, mais suit la version de l’application et ne constitue pas un paquet à télécharger.
    $exampleProject = Join-Path $repository 'src\GWGUI.Emulation.Example\GWGUI.Emulation.Example.csproj'
    $examplePublish = Join-Path $staging 'example'
    dotnet publish $exampleProject `
        -c $BuildConfiguration -r win-x64 --self-contained false -p:Version=$Version -o $examplePublish --disable-build-servers
    if ($LASTEXITCODE -ne 0) { throw "$BuildConfiguration integrated Example publish failed." }
    $integratedOutput = Join-Path $applicationPublish 'BuiltIn\Emulation\example'
    New-Item -ItemType Directory -Path $integratedOutput -Force | Out-Null
    Copy-GwGuiComponentFiles -SourceDirectory $examplePublish -DestinationDirectory $integratedOutput `
        -SharedHashes $applicationHashes -RequiredFiles @('gwgui.emulation.example.dll', 'gwgui.emulation.example.pdb', 'gwgui.emulation.example.deps.json')
    if ($SelectedModules.Count -gt 0) {
        foreach ($moduleDefinition in $SelectedModules) {
            $project = $moduleDefinition.ProjectPath
            $manifest = $moduleDefinition.Manifest
            if ($hostApiVersion -lt [Version]$manifest.hostApiMinimum `
                -or $hostApiVersion -gt [Version]$manifest.hostApiMaximum) {
                throw "Module '$($moduleDefinition.Id)' is not compatible with host API $hostApiVersion."
            }
            $destination = Join-Path $moduleOutput $moduleDefinition.Id
            New-Item -ItemType Directory -Path $destination -Force | Out-Null
            $publish = Join-Path $moduleStaging $moduleDefinition.AssemblyName
            dotnet publish $project `
                -c $BuildConfiguration -r win-x64 --self-contained false -p:Version=$($manifest.moduleVersion) -o $publish --disable-build-servers
            if ($LASTEXITCODE -ne 0) { throw "$BuildConfiguration $($moduleDefinition.AssemblyName) module publish failed." }
            $entryAssembly = Join-Path $publish $moduleDefinition.EntryAssembly
            if (-not (Test-Path -LiteralPath $entryAssembly -PathType Leaf)) {
                throw "Module entry assembly was not produced: $entryAssembly"
            }
            Copy-GwGuiComponentFiles -SourceDirectory $publish -DestinationDirectory $destination `
                -SharedHashes $applicationHashes -RequiredFiles @('module.json', $moduleDefinition.EntryAssembly,
                    ($moduleDefinition.AssemblyName + '.pdb'), ($moduleDefinition.AssemblyName + '.deps.json'))
        }
    }

    dotnet publish (Join-Path $repository 'src\GWGUI.Launcher\GWGUI.Launcher.csproj') `
        -c $BuildConfiguration -r win-x64 --self-contained false -p:Version=$Version -o $output --disable-build-servers
    if ($LASTEXITCODE -ne 0) { throw "$BuildConfiguration bootstrap publish failed." }

    Copy-Item -Path (Join-Path $applicationPublish '*') -Destination $output -Recurse -Force
    Remove-Item -LiteralPath (Join-Path $output 'gwgui.app.exe'),(Join-Path $output 'gwgui.app.runtimeconfig.json') -Force

    & (Join-Path $repository 'scripts\release\package\organize_app\organize-application-output.ps1') -OutputDirectory $output
    dotnet publish (Join-Path $repository 'src\GWGUI.Updater\GWGUI.Updater.csproj') `
        -c $BuildConfiguration -r win-x64 --self-contained false -p:Version=$Version -o $updaterPublish --disable-build-servers
    if ($LASTEXITCODE -ne 0) { throw "$BuildConfiguration updater publish failed." }
    $updaterOutput = Join-Path $output 'Updater'
    New-Item -ItemType Directory -Path $updaterOutput -Force | Out-Null
    $sharedHashes = Get-GwGuiFileHashes -Files @(Get-ChildItem -LiteralPath (Join-Path $output 'lib') -Recurse -File)
    Copy-GwGuiComponentFiles -SourceDirectory $updaterPublish -DestinationDirectory $updaterOutput `
        -SharedHashes $sharedHashes -RequiredFiles @('gwgui.updater.exe', 'gwgui.updater.dll', 'gwgui.updater.pdb',
            'gwgui.updater.deps.json', 'gwgui.updater.runtimeconfig.json')
    Remove-Item -LiteralPath $staging -Recurse -Force

    $executable = Join-Path $output 'gwgui.exe'
    if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) {
        throw "$BuildConfiguration executable was not produced: $executable"
    }
    if (-not (Test-Path -LiteralPath (Join-Path $output 'BuiltIn\Emulation\example\gwgui.emulation.example.dll') -PathType Leaf)) {
        throw "$BuildConfiguration integrated Example was not produced."
    }
    $moduleSummary = if ($SelectedModules.Count -eq 0) {
        'without emulation modules'
    }
    else {
        "with emulation modules: $(@($SelectedModules.Id) -join ', ')"
    }
    Write-Output "$BuildConfiguration build ready $moduleSummary`: $executable"
}

foreach ($buildConfiguration in $configurations) {
    New-GwGuiBuild -BuildConfiguration $buildConfiguration -SelectedModules $selectedModules
}

$stagingRoot = Join-Path $buildRoot '.staging'
if (Test-Path -LiteralPath $stagingRoot) { Remove-Item -LiteralPath $stagingRoot -Recurse -Force }
