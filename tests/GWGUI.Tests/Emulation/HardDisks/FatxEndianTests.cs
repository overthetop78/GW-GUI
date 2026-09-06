using System.Buffers.Binary;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class FatxEndianTests
{
    [Theory]
    [InlineData(256)] [InlineData(0xffef)] [InlineData(0xfff0)]
    public void BigEndianHeaderFatAndRootAgree(int clusters)
    {
        const int clusterBytes=4096;
        using var disk=new SparseMemoryStream();
        DiskImageBuilder.Write(disk,new(clusters*(long)clusterBytes,"raw","none",
            [new(0,clusters*(long)clusterBytes,"fatx-be","",SectorsPerCluster:8)]));
        var header=new byte[16];disk.Position=0;disk.ReadExactly(header);
        Assert.Equal("XTAF"u8.ToArray(),header[..4]);
        Assert.Equal(8u,BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(8)));
        Assert.Equal(1u,BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(12)));
        var width=clusters<0xfff0?2:4;var fat=new byte[8];disk.Position=4096;disk.ReadExactly(fat);
        Assert.Equal(width==2?0xfff8u:0xfffffff8u,width==2?BinaryPrimitives.ReadUInt16BigEndian(fat):BinaryPrimitives.ReadUInt32BigEndian(fat));
        Assert.Equal(width==2?0xffffu:uint.MaxValue,width==2?BinaryPrimitives.ReadUInt16BigEndian(fat.AsSpan(2)):BinaryPrimitives.ReadUInt32BigEndian(fat.AsSpan(4)));
        var fatSize=(clusters*(long)width/4096+1)*4096;
        var root=new byte[clusterBytes];disk.Position=4096+fatSize;disk.ReadExactly(root);
        Assert.All(root,b=>Assert.Equal(0xff,b));
    }
}
