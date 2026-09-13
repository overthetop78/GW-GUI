using System.Buffers.Binary;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Partitioning;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class RdbGeometryTests
{
    [Theory]
    [InlineData(1,32,40)] [InlineData(4,17,3)] [InlineData(1,1,3)]
    public void GeometryAndReservedCylindersMatchPartitionChain(int heads,int sectors,int count)
    {
        var cylinder=heads*sectors*512L;
        var reserved=((count+1L)*512+cylinder-1)/cylinder;
        var volumes=Enumerable.Range(0,count).Select(i=>new DiskVolumePlan((reserved+i*2)*cylinder,cylinder,Label:$"DH{i}")).ToArray();
        var registry=DiskFormatRegistry.CreateDefault();
        registry.Register(RdbPartitionWriter.Describe("rdb-custom",new(heads,sectors)));
        using var disk=new SparseMemoryStream();
        DiskImageBuilder.Write(disk,new((reserved+count*2)*cylinder,"raw","rdb-custom",volumes),registry);
        var root=Read(disk,0);
        Assert.Equal((uint)heads,U32(root,72)); Assert.Equal((uint)sectors,U32(root,68));
        Assert.Equal((uint)reserved,U32(root,136)); Assert.Equal((uint)(reserved*cylinder/512-1),U32(root,132));
        for(var i=0;i<count;i++)
        {
            var part=Read(disk,i+1);
            Assert.Equal((uint)heads,U32(part,140)); Assert.Equal((uint)sectors,U32(part,148));
            Assert.Equal(volumes[i].OffsetBytes/cylinder,(long)U32(part,164));
            Assert.Equal(i+1<count?(uint)i+2:uint.MaxValue,U32(part,16));
        }
    }
    [Fact]
    public void MetadataCollisionFailsBeforeWriting()
    {
        var registry=DiskFormatRegistry.CreateDefault();
        registry.Register(RdbPartitionWriter.Describe("rdb-small",new(1,1)));
        using var disk=new MemoryStream();
        Assert.ThrowsAny<ArgumentException>(()=>DiskImageBuilder.Write(disk,new(8192,"raw","rdb-small",[new(512,512),new(1024,512)]),registry));
        Assert.Equal(0,disk.Length);
    }
    private static byte[] Read(Stream disk,int block)
    {
        var data=new byte[512]; disk.Position=block*512L; disk.ReadExactly(data);
        uint sum=0; for(var i=0;i<256;i+=4) sum=unchecked(sum+U32(data,i)); Assert.Equal(0u,sum);
        return data;
    }
    private static uint U32(byte[] bytes,int offset)=>BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset));
}
