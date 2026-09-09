function Get-GwGuiEmulationModules {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot
    )

    $repository = [IO.Path]::GetFullPath($RepositoryRoot)
    $sourceRoot = Join-Path $repository 'src'
    if (-not (Test-Path -LiteralPath $sourceRoot -PathType Container)) {
        throw "Source directory is missing: $sourceRoot"
    }

    $modules = @()
    $identifiers = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $directories = @(Get-ChildItem -LiteralPath $sourceRoot -Directory -Filter 'GWGUI.Emulation.*' |
        Sort-Object Name)
    foreach ($directory in $directories) {
        $manifestPath = Join-Path $directory.FullName 'module.json'
        if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) { continue }

        $name = $directory.Name.Substring('GWGUI.Emulation.'.Length)
        if ([string]::IsNullOrWhiteSpace($name)) {
            throw "Invalid emulation module directory: $($directory.FullName)"
        }
        $projectPath = Join-Path $directory.FullName "$($directory.Name).csproj"
        if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
            throw "Module project is missing: $projectPath"
        }

        $manifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
        $manifestIsInvalid = $manifest.schemaVersion -ne 1 `
            -or [string]::IsNullOrWhiteSpace($manifest.id) `
            -or $manifest.id -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]*$' `
            -or [string]::IsNullOrWhiteSpace($manifest.entryAssembly) `
            -or $manifest.entryAssembly -notmatch '^[^\\/]+\.dll$' `
            -or $manifest.moduleVersion -notmatch '^\d+\.\d+\.\d+$' `
            -or $manifest.hostApiMinimum -notmatch '^\d+\.\d+$' `
            -or $manifest.hostApiMaximum -notmatch '^\d+\.\d+$'
        if ($manifestIsInvalid) {
            throw "Invalid official module manifest: $manifestPath"
        }
        if (-not $identifiers.Add([string]$manifest.id)) {
            throw "Duplicate emulation module id '$($manifest.id)' in $manifestPath"
        }
        if ($manifest.id -ine $name) {
            throw "Module id '$($manifest.id)' does not match directory '$($directory.Name)'."
        }

        $minimum = [Version]$manifest.hostApiMinimum
        $maximum = [Version]$manifest.hostApiMaximum
        if ($minimum -gt $maximum) {
            throw "Module '$($manifest.id)' has reversed host API bounds in $manifestPath"
        }
        [xml]$project = Get-Content -LiteralPath $projectPath -Raw -Encoding UTF8
        $assemblyName = [string](@($project.Project.PropertyGroup.AssemblyName |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) | Select-Object -First 1)
        if ([string]::IsNullOrWhiteSpace($assemblyName)) {
            $assemblyName = [IO.Path]::GetFileNameWithoutExtension($projectPath)
        }
        if ($manifest.entryAssembly -ine ($assemblyName + '.dll')) {
            throw "Module entry assembly '$($manifest.entryAssembly)' does not match project assembly '$assemblyName.dll'."
        }

        $modules += [pscustomobject]@{
            Id = [string]$manifest.id
            Name = $name
            ProjectPath = $projectPath
            ManifestPath = $manifestPath
            AssemblyName = $assemblyName
            EntryAssembly = [string]$manifest.entryAssembly
            Manifest = $manifest
        }
    }

    if ($modules.Count -eq 0) {
        throw "No emulation module manifest was found under $sourceRoot"
    }
    return @($modules | Sort-Object Id)
}

function Resolve-GwGuiEmulationModule {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Modules,
        [Parameter(Mandatory = $true)]
        [string]$Module
    )

    $match = @($Modules | Where-Object {
        $_.Id -ieq $Module -or $_.Name -ieq $Module
    })
    if ($match.Count -ne 1) {
        throw "Unknown emulation module '$Module'. Available modules: $(@($Modules.Id) -join ', ')."
    }
    return $match[0]
}
