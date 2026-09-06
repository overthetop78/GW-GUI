using System.Buffers.Binary;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class DosVariantTests
{
    [Theory]
    [InlineData("ofs-intl",2)] [InlineData("ffs-intl",3)]
    [InlineData("ofs-dircache",4)] [InlineData("ffs-dircache",5)]
    public void VariantMatchesPartitionAndAllocatedDirectoryCache(string id,int variant)
    {
        const long offset=16384, length=8L<<20;
        using var disk=new SparseMemoryStream();
        DiskImageBuilder.Write(disk,new(offset+length,"raw","rdb",[new(offset,length,id,"DATA")]));
        var part=Read(disk,512); Assert.Equal(0x444f5300u+(uint)variant,U32(part,192));
        var boot=Read(disk,offset); Assert.Equal(variant,boot[3]);
        var rootIndex=U32(boot,8); var root=Read(disk,offset+rootIndex*512L); Checksum(root);
        var cacheIndex=U32(root,504);
        if(variant<4) {Assert.Equal(0u,cacheIndex);return;}
        Assert.NotEqual(0u,cacheIndex);
        var cache=Read(disk,offset+cacheIndex*512L); Checksum(cache);
        Assert.Equal(33u,U32(cache,0)); Assert.Equal(cacheIndex,U32(cache,4)); Assert.Equal(rootIndex,U32(cache,8));
        Assert.Equal(0u,U32(cache,12)); Assert.Equal(0u,U32(cache,16));
        var bitmapNumber=(cacheIndex-2)/4064;
        var bitmapIndex=U32(root,316+(int)bitmapNumber*4);
        var bitmap=Read(disk,offset+bitmapIndex*512L); Checksum(bitmap);
        var bit=(cacheIndex-2)%4064;
        Assert.Equal(0u,U32(bitmap,4+(int)(bit/32)*4)&(1u<<(int)(bit%32)));
    }
    private static byte[] Read(Stream s,long offset){var bytes=new byte[512];s.Position=offset;s.ReadExactly(bytes);return bytes;}
    private static uint U32(byte[] b,int o)=>BinaryPrimitives.ReadUInt32BigEndian(b.AsSpan(o));
    private static void Checksum(byte[] bytes){uint sum=0;for(var i=0;i<512;i+=4)sum=unchecked(sum+U32(bytes,i));Assert.Equal(0u,sum);}
}
