using GWGUI.Emulation.Common;
using GWGUI.MediaEngine.Containers.Acorn.BbcDfs;
using GWGUI.MediaEngine.Conversion.Acorn;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Acorn;
using GWGUI.MediaEngine.SectorImages;
using System.IO;

namespace GWGUI.Tests;

public sealed class BbcDfsImageWriterTests
{
    public static IEnumerable<object[]> Profiles() => BbcDfsGeometry.Supported.Select(geometry => new object[] { geometry.FormatId, geometry.Heads == 1 ? DiskImageFileExtensions.Ssd : DiskImageFileExtensions.Dsd });

    [Theory]
    [MemberData(nameof(Profiles))]
    public async Task WriterPreservesCatalogDataAndExplicitHeadOrder(string formatId, string extension)
    {
        var geometry = BbcDfsGeometry.FindByFormatId(formatId)!;
        var source = CreateImage(geometry);
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}{extension}");
        try
        {
            await new BbcDfsImageWriter().WriteAsync(source, path, formatId);
            var reopened = await new BbcDfsReader().ReadAsync(path);

            Assert.Equal(formatId, reopened.FormatId);
            Assert.Equal(geometry.Capacity, new FileInfo(path).Length);
            for (var logicalBlock = 0; logicalBlock < source.BlockCount; logicalBlock++) Assert.Equal(source.GetBlock(logicalBlock).ToArray(), reopened.GetBlock(logicalBlock).ToArray());
            if (geometry.Heads == 2) Assert.NotEqual(reopened.GetBlock(0).ToArray(), reopened.GetBlock(BbcDfsGeometry.SectorsPerTrack).ToArray());
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task WriterRejectsAnExtensionThatDoesNotMatchTheNumberOfHeads()
    {
        var source = CreateImage(BbcDfsGeometry.Dsd40);
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.ssd");
        try
        {
            await Assert.ThrowsAsync<InvalidDataException>(() => new BbcDfsImageWriter().WriteAsync(source, path, source.FormatId));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Theory]
    [InlineData(DiskImageFormatIds.AcornDfsSingleSided, ".ssd", true)]
    [InlineData(DiskImageFormatIds.AcornDfsDoubleSided80, ".dsd", true)]
    [InlineData(DiskImageFormatIds.AcornDfsDoubleSided, ".ssd", false)]
    [InlineData(DiskImageFormatIds.AcornAdfs800, ".dsd", false)]
    public void InternalRoutingRequiresTheMatchingDfsContainer(string formatId, string extension, bool expected) => Assert.Equal(expected, BbcDfsConversionService.CanCreate(formatId, extension));

    private static SectorImage CreateImage(BbcDfsGeometry geometry)
    {
        var blocks = Enumerable.Range(0, geometry.BlockCount).Select(logicalBlock =>
        {
            var track = logicalBlock / BbcDfsGeometry.SectorsPerTrack;
            var cylinder = track / geometry.Heads;
            var head = track % geometry.Heads;
            var sector = logicalBlock % BbcDfsGeometry.SectorsPerTrack;
            var data = Enumerable.Range(0, BbcDfsGeometry.SectorSize).Select(index => (byte)(cylinder * 7 + head * 101 + sector * 13 + index)).ToArray();
            return new SectorBlock(logicalBlock, new(cylinder, head, sector), data);
        });
        return new(geometry.FormatId, BbcDfsGeometry.SectorSize, geometry.Cylinders, geometry.Heads, BbcDfsGeometry.SectorsPerTrack, blocks);
    }
}
