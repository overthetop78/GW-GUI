using GWGUI.Emulation.Common;
using GWGUI.MediaEngine.Containers.Msx.Raw;
using GWGUI.MediaEngine.Conversion.Msx;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Msx;
using GWGUI.MediaEngine.SectorImages;
using System.Buffers.Binary;
using System.IO;

namespace GWGUI.Tests;

public sealed class MsxRawImageWriterTests
{
    public static IEnumerable<object[]> Profiles() => MsxDiskGeometryCatalog.Supported.Select(geometry => new object[] { geometry.FormatId });

    [Theory]
    [MemberData(nameof(Profiles))]
    public async Task WriterPreservesEveryBlockAndBootStructure(string formatId)
    {
        Assert.True(MsxDiskGeometryCatalog.TryFromFormatId(formatId, out var geometry));
        var source = CreateImage(geometry);
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.dsk");
        try
        {
            await new MsxRawImageWriter().WriteAsync(source, path, formatId);
            var reopened = await new MsxRawImageReader().ReadAsync(path);

            Assert.Equal(formatId, reopened.FormatId);
            Assert.Equal(geometry.Capacity, new FileInfo(path).Length);
            for (var logicalBlock = 0; logicalBlock < source.BlockCount; logicalBlock++) Assert.Equal(source.GetBlock(logicalBlock).ToArray(), reopened.GetBlock(logicalBlock).ToArray());
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task WriterUsesTheSelectedProfileAndRejectsMissingBlocks()
    {
        var sourceGeometry = MsxDiskGeometryCatalog.Supported.Single(geometry => geometry.FormatId == DiskImageFormatIds.Msx1Dd);
        var source = CreateImage(sourceGeometry);
        var incomplete = new SectorImage(source.FormatId, source.BlockSize, source.Cylinders, source.Heads, source.SectorsPerTrack, source.AvailableBlocks.Where(block => block.LogicalBlock != 4));
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.dsk");
        try
        {
            await Assert.ThrowsAsync<InvalidDataException>(() => new MsxRawImageWriter().WriteAsync(source, path, DiskImageFormatIds.Msx2D));
            await Assert.ThrowsAsync<InvalidDataException>(() => new MsxRawImageWriter().WriteAsync(incomplete, path, source.FormatId));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Theory]
    [InlineData(DiskImageFormatIds.Msx1D, ".dsk", true)]
    [InlineData(DiskImageFormatIds.Msx2Dd, ".DSK", true)]
    [InlineData(DiskImageFormatIds.Msx2D, ".img", false)]
    [InlineData(DiskImageFormatIds.Ibm720, ".dsk", false)]
    public void InternalRoutingRequiresAnMsxDskProfile(string formatId, string extension, bool expected) => Assert.Equal(expected, MsxRawConversionService.CanCreate(formatId, extension));

    private static SectorImage CreateImage(MsxDiskGeometry geometry)
    {
        var blockCount = geometry.Capacity / FatBootSectorLayout.SectorSize;
        var boot = new byte[FatBootSectorLayout.SectorSize];
        System.Text.Encoding.ASCII.GetBytes("MSXDOS  ").CopyTo(boot, FatBootSectorLayout.OemOffset);
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.BytesPerSectorOffset), FatBootSectorLayout.SectorSize);
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.TotalSectors16Offset), checked((ushort)blockCount));
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.SectorsPerTrackOffset), checked((ushort)geometry.SectorsPerTrack));
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.HeadCountOffset), checked((ushort)geometry.Heads));
        boot[FatBootSectorLayout.MediaDescriptorOffset] = geometry.FormatId == DiskImageFormatIds.Msx1Dd ? MsxDiskGeometryCatalog.OneDoubleDensityMediaDescriptor : (byte)0xf9;
        var blocks = Enumerable.Range(0, blockCount).Select(logicalBlock =>
        {
            var track = logicalBlock / geometry.SectorsPerTrack;
            var data = logicalBlock == 0 ? boot : Enumerable.Range(0, FatBootSectorLayout.SectorSize).Select(index => (byte)(logicalBlock * 11 + index * 43)).ToArray();
            return new SectorBlock(logicalBlock, new(track / geometry.Heads, track % geometry.Heads, logicalBlock % geometry.SectorsPerTrack + 1), data);
        });
        return new(geometry.FormatId, FatBootSectorLayout.SectorSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, blocks);
    }
}
