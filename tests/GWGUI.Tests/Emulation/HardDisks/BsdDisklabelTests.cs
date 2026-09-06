using System.Buffers.Binary;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class BsdDisklabelTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void LabelChecksumAndRawSlotPreserveAllSevenVolumes(bool bigEndian)
    {
        var volumes=Enumerable.Range(0,7).Select(i=>new DiskVolumePlan((1+i*3L)<<20,2L<<20,"fat12",$"VOL{i}",PartitionType:"8")).ToArray();
        using var disk=new SparseMemoryStream();
        DiskImageBuilder.Write(disk,new(32L<<20,"raw",bigEndian?"bsd-disklabel-be":"bsd-disklabel",volumes));
        var label=new byte[512];disk.Position=512;disk.ReadExactly(label);
        uint Long(int o)=>bigEndian?BinaryPrimitives.ReadUInt32BigEndian(label.AsSpan(o)):BinaryPrimitives.ReadUInt32LittleEndian(label.AsSpan(o));
        ushort Word(int o)=>bigEndian?BinaryPrimitives.ReadUInt16BigEndian(label.AsSpan(o)):BinaryPrimitives.ReadUInt16LittleEndian(label.AsSpan(o));
        Assert.Equal(0x82564557u,Long(0));Assert.Equal(Long(0),Long(132));Assert.Equal(8,Word(138));
        ushort sum=0;for(var i=0;i<276;i+=2)sum^=Word(i);Assert.Equal(0,sum);
        Assert.Equal((uint)(disk.Length/512),Long(180));Assert.Equal(0u,Long(184));
        for(var i=0;i<7;i++)
        {
            var slot=i<2?i:i+1;var at=148+slot*16;
            Assert.Equal(volumes[i].OffsetBytes/512,(long)Long(at+4));Assert.Equal(8,label[at+12]);
            using var content=new SubStream(disk,Ownership.None,Long(at+4)*512L,Long(at)*512L);
            using var fs=new DiscUtils.Fat.FatFileSystem(content);Assert.Equal(volumes[i].Label,fs.VolumeLabel.Trim());
        }
    }
    [Fact]
    public void MissingTypeAndReservedAreaAreRejected()
    {
        using var disk=new MemoryStream();
        Assert.ThrowsAny<ArgumentException>(()=>DiskImageBuilder.Write(disk,new(8L<<20,"raw","bsd-disklabel",[new(512,4096)])));
        Assert.ThrowsAny<ArgumentException>(()=>DiskImageBuilder.Write(disk,new(8L<<20,"raw","bsd-disklabel",[new(1L<<20,2L<<20,"fat12")])));
        Assert.Equal(0,disk.Length);
    }
}
