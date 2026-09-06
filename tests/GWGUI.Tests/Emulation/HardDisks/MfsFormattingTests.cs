using System.Buffers.Binary;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Mfs;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class MfsFormattingTests
{
    [Theory]
    [InlineData(128,"DATA")] [InlineData(400,"DATA")] [InlineData(16384,"DATA")] [InlineData(65536,"DATA")]
    [InlineData(400,"Données")]
    public void EmptyVolumeReopensAndLeavesBackupOutsideAllocation(int kib,string label)
    {
        using var disk = new SparseMemoryStream();
        DiskImageBuilder.Write(disk, new(kib * 1024L, "raw", "none", [new(0,kib * 1024L,"mfs",label)]));
        var metadata = new byte[16 * 512]; disk.Position = 0; disk.ReadExactly(metadata);
        var copy = new byte[1024]; disk.Position = disk.Length - 1024; disk.ReadExactly(copy);
        Assert.Equal(metadata[1024..2048], copy);
        var count = BinaryPrimitives.ReadUInt16BigEndian(copy.AsSpan(18));
        var size = BinaryPrimitives.ReadUInt32BigEndian(copy.AsSpan(20));
        Assert.InRange(count, 1, 640);
        Assert.True(16 * 512L + count * (long)size <= disk.Length - 1024);
        Assert.All(copy[64..], value => Assert.Equal(0, value));
        var blocks = Enumerable.Range(0,16).Select(i => new SectorBlock(i,new(0,0,i),metadata.AsSpan(i*512,512).ToArray()));
        var image = new SectorImage(DiskImageFormatIds.AppleMacMfs,512,1,1,kib*2,blocks);
        var reader = new MacMfsFileSystemReader(); Assert.True(reader.CanRead(image));
        var volume = reader.Read(image);
        Assert.Equal(label,volume.Name); Assert.Equal(count*(long)size,volume.FreeBytes);
        if(label=="Données") Assert.Equal(0x8e,copy[41]);
        Assert.Empty(volume.Entries); Assert.Empty(volume.Warnings);
    }

    [Theory]
    [InlineData(64, "DATA")] [InlineData(65537, "DATA")]
    [InlineData(400, "")] [InlineData(400, "😀")]
    public void UnsupportedParametersDoNotWrite(int kib,string label)
    {
        using var disk = new MemoryStream(); disk.WriteByte(42);
        Assert.ThrowsAny<ArgumentException>(()=>DiskImageBuilder.Write(disk,new(kib*1024L,"raw","none",[new(0,kib*1024L,"mfs",label)])));
        Assert.Equal(new byte[]{42},disk.ToArray());
    }
}
