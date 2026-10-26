function Get-GwGuiFileHashes {
    param([object[]]$Files)

    $hashes = @{}
    foreach ($file in $Files) {
        if (-not $hashes.ContainsKey($file.Name)) {
            $hashes[$file.Name] = [Collections.Generic.HashSet[string]]::new(
                [StringComparer]::OrdinalIgnoreCase)
        }
        $null = $hashes[$file.Name].Add((Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash)
    }
    return $hashes
}

function Copy-GwGuiComponentFiles {
    param(
        [string]$SourceDirectory,
        [string]$DestinationDirectory,
        [hashtable]$SharedHashes,
        [string[]]$RequiredFiles,
        [string]$AssetsDestinationDirectory
    )

    foreach ($source in Get-ChildItem -LiteralPath $SourceDirectory -Recurse -File) {
        $relativePath = $source.FullName.Substring($SourceDirectory.Length).TrimStart('\', '/')
        $isExternalAsset = $relativePath -match '^Assets[\\/]'
        if (-not $isExternalAsset -and $source.Name -notin $RequiredFiles -and $SharedHashes.ContainsKey($source.Name)) {
            $sourceHash = (Get-FileHash -LiteralPath $source.FullName -Algorithm SHA256).Hash
            if ($SharedHashes[$source.Name].Contains($sourceHash)) { continue }
        }
        $destinationRoot = if ($isExternalAsset -and -not [string]::IsNullOrWhiteSpace($AssetsDestinationDirectory)) { $AssetsDestinationDirectory } else { $DestinationDirectory }
        $target = Join-Path $destinationRoot $relativePath
        $targetDirectory = Split-Path -Parent $target
        if (-not (Test-Path -LiteralPath $targetDirectory -PathType Container)) {
            New-Item -ItemType Directory -Path $targetDirectory -Force | Out-Null
        }
        Copy-Item -LiteralPath $source.FullName -Destination $target -Force
    }
}
