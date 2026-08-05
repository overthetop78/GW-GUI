param(
    [string]$InputPath = (Join-Path $PSScriptRoot '..\src\GWGUI.App\Assets\app-icon-chroma.png'),
    [string]$PngPath = (Join-Path $PSScriptRoot '..\src\GWGUI.App\Assets\app-icon.png'),
    [string]$IcoPath = (Join-Path $PSScriptRoot '..\src\GWGUI.App\Assets\app-icon.ico')
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$source = [Drawing.Bitmap]::new([IO.Path]::GetFullPath($InputPath))
try {
    $key = $source.GetPixel(0, 0)
    $transparent = [Drawing.Bitmap]::new($source.Width, $source.Height, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        for ($y = 0; $y -lt $source.Height; $y++) {
            for ($x = 0; $x -lt $source.Width; $x++) {
                $pixel = $source.GetPixel($x, $y)
                $distance = [Math]::Sqrt([Math]::Pow($pixel.R-$key.R,2) + [Math]::Pow($pixel.G-$key.G,2) + [Math]::Pow($pixel.B-$key.B,2))
                $alpha = if ($distance -le 12) { 0 } elseif ($distance -ge 220) { 255 } else { $t=($distance-12)/208; [int](255*$t*$t*(3-2*$t)) }
                if ($alpha -gt 0 -and $alpha -lt 255) {
                    $a = $alpha / 255.0
                    $red = [Math]::Min(255,[Math]::Max(0,[int](($pixel.R - $key.R*(1-$a))/$a)))
                    $green = [Math]::Min(255,[Math]::Max(0,[int](($pixel.G - $key.G*(1-$a))/$a)))
                    $blue = [Math]::Min(255,[Math]::Max(0,[int](($pixel.B - $key.B*(1-$a))/$a)))
                } else { $red=$pixel.R; $green=$pixel.G; $blue=$pixel.B }
                $transparent.SetPixel($x, $y, [Drawing.Color]::FromArgb($alpha, $red, $green, $blue))
            }
        }
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
