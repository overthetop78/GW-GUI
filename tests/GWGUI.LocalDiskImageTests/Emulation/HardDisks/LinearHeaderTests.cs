using System.Buffers.Binary;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class LinearHeaderTests
{
    [Theory]
    [InlineData("hdi",4096)] [InlineData("nhd",512)] [InlineData("thd",256)]
    public void HeaderOffsetPreservesLogicalContent(string id,int headerSize)
    {
        var capacity=ThdImageWriter.CylinderBytes*32L;
        using var disk=new SparseMemoryStream();
        DiskImageBuilder.Write(disk,new(capacity,id,"none",[new(0,capacity,"fat12","DATA")]));
        var header=new byte[headerSize]; disk.Position=0; disk.ReadExactly(header);
        if(id=="hdi")
        {
            Assert.Equal(4096u,U32(header,8)); Assert.Equal((uint)capacity,U32(header,12));
            Assert.Equal(capacity,(long)U32(header,16)*U32(header,20)*U32(header,24)*U32(header,28));
        }
        else if(id=="nhd")
        {
            Assert.Equal("T98HDDIMAGE.R0"u8.ToArray(),header[.."T98HDDIMAGE.R0".Length]);
            Assert.Equal(512u,U32(header,272));
            Assert.Equal(capacity,(long)U32(header,276)*U16(header,280)*U16(header,282)*U16(header,284));
        }
        else Assert.Equal(32,U16(header,0));
        Assert.Equal(capacity+headerSize,disk.Length);
        using var content=new SubStream(disk,Ownership.None,headerSize,capacity);
        using var fs=new DiscUtils.Fat.FatFileSystem(content);
        Assert.Equal("DATA",fs.VolumeLabel.Trim());
        using(var file=fs.OpenFile("TEST",FileMode.Create,FileAccess.Write)) file.WriteByte(42);
        using var read=fs.OpenFile("TEST",FileMode.Open); Assert.Equal(42,read.ReadByte());
    }

    [Theory]
    [InlineData(128)] [InlineData(256)] [InlineData(512)] [InlineData(4096)]
    public void ExplicitGeometryControlsHeader(int sectorBytes)
    {
        var capacity=5L*4*17*sectorBytes;
        using var hdi=new MemoryStream(); using var nhd=new MemoryStream();
        HdiImageWriter.Write(hdi,capacity,s=>{s.Position=capacity-1;s.WriteByte(42);},4,17,sectorBytes);
        NhdImageWriter.Write(nhd,capacity,s=>{s.Position=capacity-1;s.WriteByte(43);},4,17,sectorBytes,"DATA");
        Assert.Equal(5u,U32(hdi.ToArray(),28)); Assert.Equal((uint)sectorBytes,U32(hdi.ToArray(),16));
        Assert.Equal(5u,U32(nhd.ToArray(),276)); Assert.Equal(sectorBytes,U16(nhd.ToArray(),284));
        Assert.Equal(42,hdi.ToArray()[^1]); Assert.Equal(43,nhd.ToArray()[^1]);
    }

    [Fact]
    public void GeometryAndInitializationErrorsPreserveEmptyDestination()
    {
        using var disk=new MemoryStream();
        Assert.ThrowsAny<ArgumentException>(()=>HdiImageWriter.Write(disk,1L<<32));
        Assert.ThrowsAny<ArgumentException>(()=>NhdImageWriter.Write(disk,4096,heads:3));
        Assert.ThrowsAny<ArgumentException>(()=>ThdImageWriter.Write(disk,4096));
        Assert.Throws<InvalidOperationException>(()=>NhdImageWriter.Write(disk,4096,s=>s.SetLength(8192)));
        Assert.Equal(0,disk.Length);
    }
    private static uint U32(byte[] bytes,int offset)=>BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset));
    private static ushort U16(byte[] bytes,int offset)=>BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset));
}
