using System.IO;
using GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;
using GWGUI.MediaEngine.Conversion.Amstrad;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Definitions;

namespace GWGUI.Tests;

/// <summary>Vérifie l'écriture CPCEMU sans perte des descripteurs CPC et PCW.</summary>
public sealed class CpcDskWriterTests
{
    [Theory]
    [InlineData("validated_images/Amstrad/CPC/3 pouces simple face - 180 Kio/007 - A View to a Kill (1985)(Domark).dsk")]
    [InlineData("validated_images/Amstrad/PCW/3 pouces double face - 720 Kio/CF2DD.DSK")]
    public async Task RewritesRealCpcAndPcwContainersWithoutLosingDescriptors(string relativePath)
    {
        var sourcePath = ImagePath(relativePath);
        var reader = new CpcDskReader();
        var source = await reader.ReadDetailedAsync(sourcePath);
        var extension = source.Kind == CpcDskContainerKind.Extended ? ".edsk" : ".dsk";
        var outputPath = GeneratedPath($"roundtrip-{Path.GetFileNameWithoutExtension(sourcePath)}{extension}");

        await new CpcDskWriter().WriteAsync(source, outputPath);
        var result = await reader.ReadDetailedAsync(outputPath);

        AssertEquivalent(source, result);
    }

    [Fact]
    public async Task ExtendedWriterPreservesStoredSizeStatusesOrderAndTrackFields()
    {
        var sectors = new[]
        {
            new CpcDskSector(0, 0, 0xc2, 2, 0x20, 0x01, Enumerable.Repeat((byte)0xa5, 768).ToArray()),
            new CpcDskSector(0, 0, 0xc1, 1, 0x04, 0x20, Enumerable.Range(0, 256).Select(value => checked((byte)value)).ToArray())
        };
        var imageBlocks = new[]
        {
            new GWGUI.MediaEngine.SectorImages.SectorBlock(0, new(0, 0, 0xc2), Enumerable.Repeat((byte)0xa5, 512).ToArray()),
            new GWGUI.MediaEngine.SectorImages.SectorBlock(1, new(0, 0, 0xc1), Enumerable.Range(0, 256).Select(value => checked((byte)value)).ToArray())
        };
        var sectorImage = new GWGUI.MediaEngine.SectorImages.SectorImage(DiskImageFormatIds.AmstradCpc, 512, 1, 1, 2, imageBlocks, true, 768, 2);
        var source = new CpcDskImage(CpcDskContainerKind.Extended, 1, 1, [new(0, true, 0, 0, 2, 0x2a, 0xe5, sectors)], sectorImage);
        var outputPath = GeneratedPath("descriptor-preservation.edsk");

        await new CpcDskWriter().WriteAsync(source, outputPath);
        var result = await new CpcDskReader().ReadDetailedAsync(outputPath);

        AssertEquivalent(source, result);
    }

    [Theory]
    [InlineData(DiskImageFormatIds.AmstradCpc, ".dsk")]
    [InlineData(DiskImageFormatIds.AmstradCpc, ".edsk")]
    [InlineData(DiskImageFormatIds.AmstradPcw, ".dsk")]
    [InlineData(DiskImageFormatIds.AmstradPcw, ".edsk")]
    public void ConversionServiceAcceptsCpcAndPcwDskTargets(string formatId, string extension) => Assert.True(AmstradDskConversionService.CanCreate(formatId, extension));

    [Theory]
    [InlineData("validated_images/Amstrad/CPC/3 pouces simple face - 180 Kio/007 - A View to a Kill (1985)(Domark).dsk", DiskImageFormatIds.AmstradCpc)]
    [InlineData("validated_images/Amstrad/PCW/3 pouces double face - 720 Kio/CF2DD.DSK", DiskImageFormatIds.AmstradPcw)]
    public async Task ConversionServiceCreatesReadableExtendedCpcAndPcwImages(string relativePath, string formatId)
    {
        var sourcePath = ImagePath(relativePath);
        var outputPath = GeneratedPath($"converted-{Path.GetFileNameWithoutExtension(sourcePath)}.edsk");

        await MediaEngineFactory.CreateAmstradDskConversionService().ConvertAsync(sourcePath, outputPath, formatId);

        var source = await new CpcDskReader().ReadDetailedAsync(sourcePath);
        var result = await new CpcDskReader().ReadDetailedAsync(outputPath);
        AssertEquivalent(source with { Kind = CpcDskContainerKind.Extended }, result);
    }

    [Fact]
    public async Task StandardWriterRejectsAStoredSizeDifferentFromTheNominalDescriptorSize()
    {
        var sector = new CpcDskSector(0, 0, 1, 2, 0, 0, new byte[768]);
        var sectorImage = new GWGUI.MediaEngine.SectorImages.SectorImage(DiskImageFormatIds.AmstradCpc, 512, 1, 1, 1, [new(0, new(0, 0, 1), new byte[512])]);
        var source = new CpcDskImage(CpcDskContainerKind.Standard, 1, 1, [new(0, true, 0, 0, 2, 0x4e, 0xe5, [sector])], sectorImage);

        await Assert.ThrowsAsync<InvalidDataException>(() => new CpcDskWriter().WriteAsync(source, GeneratedPath("lossy-standard.dsk")));
    }

    private static void AssertEquivalent(CpcDskImage expected, CpcDskImage actual)
    {
        Assert.Equal(expected.Kind, actual.Kind);
        Assert.Equal(expected.Cylinders, actual.Cylinders);
        Assert.Equal(expected.Heads, actual.Heads);
        Assert.Equal(expected.Tracks.Count, actual.Tracks.Count);
        for (var trackIndex = 0; trackIndex < expected.Tracks.Count; trackIndex++)
        {
            var left = expected.Tracks[trackIndex];
            var right = actual.Tracks[trackIndex];
            Assert.Equal((left.Index, left.IsPresent, left.Cylinder, left.Head, left.SectorSizeCode, left.Gap3Length, left.FillerByte), (right.Index, right.IsPresent, right.Cylinder, right.Head, right.SectorSizeCode, right.Gap3Length, right.FillerByte));
            Assert.Equal(left.Sectors.Count, right.Sectors.Count);
            for (var sectorIndex = 0; sectorIndex < left.Sectors.Count; sectorIndex++)
            {
                var expectedSector = left.Sectors[sectorIndex];
                var actualSector = right.Sectors[sectorIndex];
                Assert.Equal((expectedSector.Cylinder, expectedSector.Head, expectedSector.Id, expectedSector.SizeCode, expectedSector.Status1, expectedSector.Status2), (actualSector.Cylinder, actualSector.Head, actualSector.Id, actualSector.SizeCode, actualSector.Status1, actualSector.Status2));
                Assert.Equal(expectedSector.Data, actualSector.Data);
            }
        }
    }

    private static string GeneratedPath(string fileName)
    {
        var directory = Path.Combine(FindImageTestRoot(), "_generated", "cpcdsk-writer");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, fileName);
    }

    private static string ImagePath(string relativePath) => Path.Combine(FindImageTestRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar));

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
