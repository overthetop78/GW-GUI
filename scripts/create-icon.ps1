param(
    [string]$InputPath = (Join-Path $PSScriptRoot '..\src\GWGUI.App\Assets\app-icon-source.png'),
    [string]$PngPath = (Join-Path $PSScriptRoot '..\src\GWGUI.App\Assets\app-icon.png'),
    [string]$IcoPath = (Join-Path $PSScriptRoot '..\src\GWGUI.App\Assets\app-icon.ico')
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$source = [Drawing.Bitmap]::new([IO.Path]::GetFullPath($InputPath))
try {
    $canvasSize = [Math]::Max($source.Width, $source.Height)
    $transparent = [Drawing.Bitmap]::new($canvasSize, $canvasSize, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $graphics = [Drawing.Graphics]::FromImage($transparent)
        try {
            $graphics.Clear([Drawing.Color]::Transparent)
            $graphics.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceCopy
            $x = [int](($canvasSize - $source.Width) / 2)
            $y = [int](($canvasSize - $source.Height) / 2)
            $graphics.DrawImageUnscaled($source, $x, $y)
        } finally { $graphics.Dispose() }
        $transparent.Save([IO.Path]::GetFullPath($PngPath), [Drawing.Imaging.ImageFormat]::Png)

        $sizes = @(16,24,32,48,64,128,256)
        $images = foreach ($size in $sizes) {
            $bitmap = [Drawing.Bitmap]::new($size, $size, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
            try {
                $graphics = [Drawing.Graphics]::FromImage($bitmap)
                try {
                    $graphics.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceCopy
                    $graphics.CompositingQuality = [Drawing.Drawing2D.CompositingQuality]::HighQuality
                    $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                    $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::HighQuality
                    $graphics.DrawImage($transparent, 0, 0, $size, $size)
                } finally { $graphics.Dispose() }
                $memory = [IO.MemoryStream]::new(); $bitmap.Save($memory, [Drawing.Imaging.ImageFormat]::Png); ,$memory.ToArray()
            } finally { $bitmap.Dispose() }
        }
        $stream = [IO.File]::Create([IO.Path]::GetFullPath($IcoPath)); $writer = [IO.BinaryWriter]::new($stream)
        try {
            $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]$sizes.Count)
            $offset = 6 + 16*$sizes.Count
            for ($index=0; $index -lt $sizes.Count; $index++) {
                $size=$sizes[$index]; $encodedSize = if($size -eq 256){0}else{$size}; $writer.Write([byte]$encodedSize); $writer.Write([byte]$encodedSize); $writer.Write([byte]0); $writer.Write([byte]0); $writer.Write([uint16]1); $writer.Write([uint16]32); $writer.Write([uint32]$images[$index].Length); $writer.Write([uint32]$offset); $offset += $images[$index].Length
            }
            foreach ($image in $images) { $writer.Write($image) }
        } finally { $writer.Dispose(); $stream.Dispose() }
    } finally { $transparent.Dispose() }
} finally { $source.Dispose() }
