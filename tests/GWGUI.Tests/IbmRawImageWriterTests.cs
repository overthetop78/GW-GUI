using GWGUI.Emulation.Common;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Containers.Ibm.Raw;
using GWGUI.MediaEngine.Conversion.Ibm;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Ibm;
using GWGUI.MediaEngine.SectorImages;
using System.IO;

namespace GWGUI.Tests;

public sealed class IbmRawImageWriterTests
{
    public static IEnumerable<object[]> Profiles() => IbmPcGeometryCatalog.All.SelectMany(geometry => new[] { DiskImageFileExtensions.Ima, DiskImageFileExtensions.Img }.Select(extension => new object[] { geometry.FormatId, extension }));

    [Theory]
    [MemberData(nameof(Profiles))]
    public async Task WriterRoundTripsEverySupportedGeometry(string formatId, string extension)
    {
        Assert.True(IbmPcGeometryCatalog.TryFromFormatId(formatId, out var geometry));
        var source = CreateImage(geometry);
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}{extension}");
        try
        {
            await new IbmRawImageWriter().WriteAsync(source, path, formatId);
            var reopened = await new IbmRawImageReader().ReadAsync(path);

            Assert.Equal(geometry.Capacity, new FileInfo(path).Length);
            Assert.Equal(source.BlockCount, reopened.BlockCount);
            for (var logicalBlock = 0; logicalBlock < source.BlockCount; logicalBlock++) Assert.Equal(source.GetBlock(logicalBlock).ToArray(), reopened.GetBlock(logicalBlock).ToArray());
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Theory]
    [MemberData(nameof(Profiles))]
    public async Task ConversionServiceRoundTripsImaAndImgWithTheSameBlocks(string formatId, string sourceExtension)
    {
        Assert.True(IbmPcGeometryCatalog.TryFromFormatId(formatId, out var geometry));
        var source = CreateImage(geometry);
        var targetExtension = sourceExtension == DiskImageFileExtensions.Ima ? DiskImageFileExtensions.Img : DiskImageFileExtensions.Ima;
        var sourcePath = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}{sourceExtension}");
        var targetPath = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}{targetExtension}");
        try
        {
            await new IbmRawImageWriter().WriteAsync(source, sourcePath, formatId);
            await MediaEngineFactory.CreateIbmRawConversionService().ConvertAsync(sourcePath, targetPath, formatId);
            var reopened = await new IbmRawImageReader().ReadAsync(targetPath);
            Assert.Equal(geometry.Capacity, new FileInfo(targetPath).Length);
            Assert.Equal(source.BlockCount, reopened.BlockCount);
            for (var logicalBlock = 0; logicalBlock < source.BlockCount; logicalBlock++) Assert.Equal(source.GetBlock(logicalBlock).ToArray(), reopened.GetBlock(logicalBlock).ToArray());
        }
        finally
        {
            if (File.Exists(sourcePath)) File.Delete(sourcePath);
            if (File.Exists(targetPath)) File.Delete(targetPath);
        }
    }

    [Fact]
    public async Task WriterKeepsDmfExplicitAndRejectsGeometryChangesAndMissingBlocks()
    {
        Assert.True(IbmPcGeometryCatalog.TryFromFormatId(DiskImageFormatIds.IbmDmf, out var dmf));
        Assert.True(IbmPcGeometryCatalog.TryFromFormatId(DiskImageFormatIds.Ibm720, out var smaller));
        var source = CreateImage(dmf);
        var incomplete = new SectorImage(source.FormatId, source.BlockSize, source.Cylinders, source.Heads, source.SectorsPerTrack, source.AvailableBlocks.Where(block => block.LogicalBlock != 1));
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.img");
        try
        {
            await new IbmRawImageWriter().WriteAsync(source, path, DiskImageFormatIds.IbmDmf);
            Assert.Equal(dmf.Capacity, new FileInfo(path).Length);
            await Assert.ThrowsAsync<InvalidDataException>(() => new IbmRawImageWriter().WriteAsync(source, path, smaller.FormatId));
            await Assert.ThrowsAsync<InvalidDataException>(() => new IbmRawImageWriter().WriteAsync(incomplete, path, DiskImageFormatIds.IbmDmf));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Theory]
    [InlineData(DiskImageFormatIds.Ibm720, ".ima", true)]
    [InlineData(DiskImageFormatIds.IbmDmf, ".img", true)]
    [InlineData(DiskImageFormatIds.IbmScan, ".img", false)]
    [InlineData(DiskImageFormatIds.Ibm720, ".adf", false)]
    public void InternalRoutingRequiresAnExplicitImaOrImgProfile(string formatId, string extension, bool expected) => Assert.Equal(expected, IbmRawConversionService.CanCreate(formatId, extension));

    private static SectorImage CreateImage(IbmPcGeometry geometry)
    {
        var blocks = Enumerable.Range(0, geometry.Capacity / FatBootSectorLayout.SectorSize).Select(logicalBlock =>
        {
            var track = logicalBlock / geometry.SectorsPerTrack;
            var data = logicalBlock == 0 ? new byte[FatBootSectorLayout.SectorSize] : Enumerable.Range(0, FatBootSectorLayout.SectorSize).Select(index => (byte)(logicalBlock * 29 + index * 13)).ToArray();
            return new SectorBlock(logicalBlock, new(track / geometry.Heads, track % geometry.Heads, logicalBlock % geometry.SectorsPerTrack + 1), data);
        });
        return new(geometry.FormatId, FatBootSectorLayout.SectorSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, blocks);
    }
}
