param([string]$WorkingDirectory)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$artifacts = [IO.Path]::GetFullPath((Join-Path $repository 'artifacts'))
$artifactsPrefix = $artifacts + [IO.Path]::DirectorySeparatorChar
if ([string]::IsNullOrWhiteSpace($WorkingDirectory)) { $WorkingDirectory = Join-Path $artifacts 'scp-corpus-smoke' }
$working = [IO.Path]::GetFullPath($WorkingDirectory)
if (-not $working.StartsWith($artifactsPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'WorkingDirectory must be located inside the repository artifacts directory.'
}
if (Test-Path -LiteralPath $working) { throw "SCP corpus directory already exists: $working" }

$captures = @(
    [pscustomobject]@{ Identifier='os9sys'; FileName='os9sys.scp'; DecoderPrefix='iso.'; MinimumTracks=80; Heads='0,1'; Size=39322724; Md5='63c931f79f33398ec4026bc1f5109cdb'; Sha1='3f20aeb3ac0b7328eb233b76267527d23b790ea3' },
    [pscustomobject]@{ Identifier='pubsoft1'; FileName='pubsoft1.scp'; DecoderPrefix='iso.'; MinimumTracks=80; Heads='0,1'; Size=38742166; Md5='6d7fccfa3399c6f06e20d269d707c4f0'; Sha1='d9a6ed364506a85579281d7d416bb6bc569e6c97' },
    [pscustomobject]@{ Identifier='amiga-amos-professional-demo-disc'; FileName='amiga amos professional demo disc.scp'; DecoderPrefix='amiga.mfm'; MinimumTracks=80; Heads='0,1'; Size=27447022; Md5='e5cab62655cb779cb70c91bde6a0a21c'; Sha1='43c8fe6c75a50c964c3f040995608bc810f1364d' },
    [pscustomobject]@{ Identifier='lexitroncpm'; FileName='cpm2.2e.scp'; DecoderPrefix='iso.fm'; MinimumTracks=40; Heads='0'; LicenseUrl='http://creativecommons.org/publicdomain/mark/1.0/'; Size=8814642; Md5='33548a76df23e6c02b3fc1c4967b9d75'; Sha1='af619d86ae30853fa58fa8ee3ccb66044a0dfdec' }
)
$previousEnvironment = $env:GWGUI_REAL_SCP_CORPUS
try {
    New-Item -ItemType Directory -Path $working | Out-Null
    $downloaded = foreach ($capture in $captures) {
        if ($capture.PSObject.Properties.Name -contains 'LicenseUrl') {
            $metadata = Invoke-RestMethod -Uri "https://archive.org/metadata/$($capture.Identifier)" -Headers @{ 'User-Agent'='GW-GUI-corpus-validation' }
            if ($metadata.metadata.licenseurl -ne $capture.LicenseUrl) { throw "Published licence metadata does not match $($capture.Identifier)." }
        }
        $path = Join-Path $working $capture.FileName
        $encodedFileName = [uri]::EscapeDataString($capture.FileName)
        $uri = "https://archive.org/download/$($capture.Identifier)/$encodedFileName"
        Invoke-WebRequest -Uri $uri -OutFile $path -Headers @{ 'User-Agent'='GW-GUI-corpus-validation' }
        $file = Get-Item -LiteralPath $path
        $md5 = (Get-FileHash -LiteralPath $path -Algorithm MD5).Hash.ToLowerInvariant()
        $sha1 = (Get-FileHash -LiteralPath $path -Algorithm SHA1).Hash.ToLowerInvariant()
        if ($file.Length -ne $capture.Size -or $md5 -ne $capture.Md5 -or $sha1 -ne $capture.Sha1) {
            throw "Published integrity metadata does not match $($capture.FileName)."
        }
        [pscustomobject]@{ Identifier=$capture.Identifier; DecoderPrefix=$capture.DecoderPrefix; MinimumTracks=$capture.MinimumTracks; Heads=$capture.Heads; Path=$path; Size=$file.Length; Md5=$md5; Sha1=$sha1 }
    }

    $env:GWGUI_REAL_SCP_CORPUS = [string]::Join([IO.Path]::PathSeparator, @($downloaded | ForEach-Object { "$($_.DecoderPrefix)|$($_.MinimumTracks)|$($_.Heads)|$($_.Path)" }))
    dotnet test (Join-Path $repository 'GWGUI.sln') -c Release --no-restore --filter 'FullyQualifiedName~RealScpCorpusTests'
    if ($LASTEXITCODE -ne 0) { throw 'The real SCP corpus integration tests failed.' }
    $downloaded | Select-Object Identifier,Size,Md5,Sha1
}
finally {
    $env:GWGUI_REAL_SCP_CORPUS = $previousEnvironment
    if (Test-Path -LiteralPath $working) { Remove-Item -LiteralPath $working -Recurse -Force }
}

Write-Output 'Public physical SCP corpus validation passed and the isolated downloads were removed.'
