using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Ucsd;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class PascalFormattingTests
{
    [Theory]
    [InlineData(6, false)]
    [InlineData(280, false)]
    [InlineData(65535, false)]
    [InlineData(4096, true)]
    public void CreatedVolumeReopensWithExistingReader(int blocks, bool partitioned)
    {
        var offset = partitioned ? 1L << 20 : 0;
        using var disk = new SparseMemoryStream();
        DiskImageBuilder.Write(disk, new DiskImagePlan(offset + blocks * 512L, "raw", partitioned ? "mbr" : "none",
            [new DiskVolumePlan(offset, blocks * 512L, "pascal", "DATA", MbrType: 0xda)]));
        var sectors = new List<SectorBlock>();
        for (var index = 0; index < 6; index++)
        {
            var bytes = new byte[512]; disk.Position = offset + index * 512L; disk.ReadExactly(bytes);
            sectors.Add(new SectorBlock(index, new(0, 0, index), bytes));
        }
        var image = new SectorImage(DiskImageFormatIds.UcsdIbmMfm, 512, 1, 1, blocks, sectors);
        var reader = new UcsdFileSystemReader();
        Assert.True(reader.CanRead(image));
        var volume = reader.Read(image);
        Assert.Equal("DATA", volume.Name);
        Assert.Equal(blocks * 512L, volume.Capacity);
        Assert.Equal((blocks - 6) * 512L, volume.FreeBytes);
        Assert.Empty(volume.Entries);
        Assert.Empty(volume.Warnings);
        if (partitioned)
        {
            disk.Position = 510;
            Assert.Equal(0x55, disk.ReadByte()); Assert.Equal(0xaa, disk.ReadByte());
        }
    }

    [Theory]
    [InlineData(5, "DATA")]
    [InlineData(65536, "DATA")]
    [InlineData(280, "")]
    [InlineData(280, "TOO.LONG")]
    [InlineData(280, "lower")]
    [InlineData(280, "A:B")]
    [InlineData(280, "A B")]
    [InlineData(280, "DÉMO")]
    public void InvalidPlansPreserveDestination(int blocks, string label)
    {
        using var destination = new MemoryStream(); destination.WriteByte(42);
        Assert.ThrowsAny<ArgumentException>(() => DiskImageBuilder.Write(destination,
            new DiskImagePlan(blocks * 512L, "raw", "none", [new DiskVolumePlan(0, blocks * 512L, "pascal", label)])));
        Assert.Equal(new byte[] { 42 }, destination.ToArray());
    }
}
