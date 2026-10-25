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
        [string[]]$RequiredFiles
    )

    foreach ($source in Get-ChildItem -LiteralPath $SourceDirectory -Recurse -File) {
        if ($source.Name -notin $RequiredFiles -and $SharedHashes.ContainsKey($source.Name)) {
            $sourceHash = (Get-FileHash -LiteralPath $source.FullName -Algorithm SHA256).Hash
            if ($SharedHashes[$source.Name].Contains($sourceHash)) { continue }
        }
        $relativePath = $source.FullName.Substring($SourceDirectory.Length).TrimStart('\', '/')
        $target = Join-Path $DestinationDirectory $relativePath
        $targetDirectory = Split-Path -Parent $target
        if (-not (Test-Path -LiteralPath $targetDirectory -PathType Container)) {
            New-Item -ItemType Directory -Path $targetDirectory -Force | Out-Null
        }
        Copy-Item -LiteralPath $source.FullName -Destination $target -Force
    }
}
