using System.Buffers.Binary;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.FileSystems;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class MinixFormattingTests
{
    [Theory]
    [InlineData(1,64)] [InlineData(1,65535)] [InlineData(2,64)] [InlineData(2,131072)] [InlineData(3,64)] [InlineData(3,131072)]
    public void RootAndAllocationMapsAgree(int version,int kib)
    {
        using var disk=new SparseMemoryStream();
        DiskImageBuilder.Write(disk,new(kib*1024L,"raw","none",[new(0,kib*1024L,$"minix{version}","")]));
        var super=Read(disk,1);
        var imaps=U16(super,version==3?6:4); var zmaps=U16(super,version==3?8:6); var first=U16(super,version==3?10:8);
        Assert.Equal(version==1?0x138f:version==2?0x2478:0x4d5a,U16(super,version==3?24:16));
        Assert.Equal((uint)kib,version==1?U16(super,2):U32(super,20));
        var inode=Read(disk,2+imaps+zmaps);
        Assert.Equal(0x41ed,U16(inode,0)); Assert.Equal(2,version==1?inode[13]:U16(inode,2));
        Assert.Equal((uint)first,version==1?U16(inode,14):U32(inode,24));
        var root=Read(disk,first);var width=version==3?4:2;var entry=version==3?64:32;
        Assert.Equal(1u,version==3?U32(root,0):U16(root,0)); Assert.Equal((byte)'.',root[width]);
        Assert.Equal(1u,version==3?U32(root,entry):U16(root,entry)); Assert.Equal(".."u8.ToArray(),root[(entry+width)..(entry+width+2)]);
        var imap=Read(disk,2); Assert.Equal(3,imap[0]);
        long free=0;
        for(var i=0;i<zmaps;i++) foreach(var value in Read(disk,2+imaps+i)) free+=8-System.Numerics.BitOperations.PopCount((uint)value);
        Assert.Equal(kib-first-1L,free);
    }
    [Fact]
    public void InvalidParametersAreRejected()
    {
        Assert.ThrowsAny<ArgumentException>(()=>MinixVolumeFormatter.Validate(64L<<20,1));
        Assert.ThrowsAny<ArgumentException>(()=>MinixVolumeFormatter.Validate(64L<<10,3,65535));
        using var disk=new MemoryStream();
        Assert.ThrowsAny<ArgumentException>(()=>DiskImageBuilder.Write(disk,new(1L<<20,"raw","none",[new(0,1L<<20,"minix3","NAME")])));
        Assert.Equal(0,disk.Length);
    }
    private static byte[] Read(Stream disk,int block){var bytes=new byte[1024];disk.Position=block*1024L;disk.ReadExactly(bytes);return bytes;}
    private static ushort U16(byte[] b,int o)=>BinaryPrimitives.ReadUInt16LittleEndian(b.AsSpan(o));
    private static uint U32(byte[] b,int o)=>BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(o));
}
