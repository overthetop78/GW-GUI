using GWGUI.Emulation.Common;
using GWGUI.MediaEngine.Containers.Adf;
using GWGUI.MediaEngine.Conversion.Acorn;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Acorn;
using GWGUI.MediaEngine.SectorImages;
using System.IO;

namespace GWGUI.Tests;

public sealed class AcornAdfWriterTests
{
    [Fact]
    public async Task WriterProducesAnExactAcornAdfAndPreservesEveryBlock()
    {
        var source = CreateImage();
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.adf");
        try
        {
            await new AcornAdfWriter().WriteAsync(source, path);
            var reopened = await new AdfReader().ReadAsync(path);

            Assert.Equal(DiskImageFormatIds.AcornAdfs800, reopened.FormatId);
            Assert.Equal(AcornAdfGeometry.Capacity, new FileInfo(path).Length);
            for (var logicalBlock = 0; logicalBlock < source.BlockCount; logicalBlock++) Assert.Equal(source.GetBlock(logicalBlock).ToArray(), reopened.GetBlock(logicalBlock).ToArray());
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task WriterRejectsMissingBlocksAndAnAmigaGeometry()
    {
        var source = CreateImage();
        var incomplete = new SectorImage(source.FormatId, source.BlockSize, source.Cylinders, source.Heads, source.SectorsPerTrack, source.AvailableBlocks.Where(block => block.LogicalBlock != 9));
        var amiga = new SectorImage(DiskImageFormatIds.AmigaDos, 512, 80, 2, 11, []);
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.adf");
        try
        {
            await Assert.ThrowsAsync<InvalidDataException>(() => new AcornAdfWriter().WriteAsync(incomplete, path));
            await Assert.ThrowsAsync<InvalidDataException>(() => new AcornAdfWriter().WriteAsync(amiga, path));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Theory]
    [InlineData(DiskImageFormatIds.AcornAdfs800, ".adf", true)]
    [InlineData(DiskImageFormatIds.AcornAdfs800, ".img", false)]
    [InlineData(DiskImageFormatIds.AmigaDos, ".adf", false)]
    public void InternalRoutingUsesTheFormatIdToDisambiguateAdf(string formatId, string extension, bool expected) => Assert.Equal(expected, AcornAdfConversionService.CanCreate(formatId, extension));

    private static SectorImage CreateImage()
    {
        var geometry = AcornAdfGeometry.Geometry;
        var blocks = Enumerable.Range(0, geometry.BlockCount).Select(logicalBlock =>
        {
            var track = logicalBlock / geometry.SectorsPerTrack;
            var data = Enumerable.Range(0, geometry.BlockSize).Select(index => (byte)(logicalBlock * 37 + index * 19)).ToArray();
            return new SectorBlock(logicalBlock, new(track / geometry.Heads, track % geometry.Heads, logicalBlock % geometry.SectorsPerTrack), data);
        });
        return new(geometry.FormatId, geometry.BlockSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, blocks);
    }
}
