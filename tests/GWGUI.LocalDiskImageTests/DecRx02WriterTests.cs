using GWGUI.MediaEngine.Containers.Dec.Rx02;
using GWGUI.MediaEngine.Conversion.Dec;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Geometries.Dec;
using System.IO;

namespace GWGUI.Tests;

/// <summary>Vérifie l'ordre physique produit par le Writer DEC RX02.</summary>
public sealed class DecRx02WriterTests
{
    [Fact]
    public async Task RealRx02ImageRoundTripsByteForByte()
    {
        var sourcePath = ImagePath();
        var sourceBytes = await File.ReadAllBytesAsync(sourcePath);
        var image = await new DecRx02Reader().ReadAsync(sourcePath);
        var outputPath = Path.Combine(Path.GetDirectoryName(sourcePath)!, "_gwgui-rx02-roundtrip.img");
        try
        {
            await new DecRx02Writer().WriteAsync(image, outputPath);
            var result = await new DecRx02Reader().ReadAsync(outputPath);

            Assert.Equal(DecRx02Geometry.Capacity, new FileInfo(outputPath).Length);
            Assert.Equal(sourceBytes, await File.ReadAllBytesAsync(outputPath));
            Assert.Equal(image.AvailableBlocks.SelectMany(block => block.Data), result.AvailableBlocks.SelectMany(block => block.Data));
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public void ConversionServiceAcceptsOnlyRx02Img()
    {
        Assert.True(DecRx02ConversionService.CanCreate(DiskImageFormatIds.DecRx02, ".img"));
        Assert.False(DecRx02ConversionService.CanCreate(DiskImageFormatIds.DecRx02, ".imd"));
    }

    private static string ImagePath()
    {
        var path = Path.Combine(FindImageTestRoot(), "validated_images", "DEC", "MINC", "8 pouces - RX02 - DEC RT-11 - 500 Kio", "BA-J837B-BC_MINC_MA_DEMO_23_V2.0_BIN_RX2.img");
        return File.Exists(path) ? path : throw new FileNotFoundException("L'image RX02 RT-11 locale requise est absente.", path);
    }

    private static string FindImageTestRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "image_test");
            if (Directory.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("Le dossier local image_test est introuvable.");
    }
}
