using System.Buffers.Binary;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class ClassicHfsTests
{
    [Theory]
    [InlineData(128,"DATA")] [InlineData(32768,"DATA")] [InlineData(4194304,"DATA")]
    [InlineData(128,"Données")]
    public void CatalogReopensAndAllocationIsConsistent(int kib,string label)
    {
        using var disk=new SparseMemoryStream();
        DiskImageBuilder.Write(disk,new(kib*1024L,"raw","none",[new(0,kib*1024L,"hfs",label)]));
        var mdb=Read(disk,1024,512); Assert.Equal(mdb,Read(disk,disk.Length-1024,512));
        var count=U16(mdb,18); var allocation=U32(mdb,20); var start=U16(mdb,28)*512L;
        var bitmap=Read(disk,1536,8192);
        var free=Enumerable.Range(0,count).Count(i=>(bitmap[i/8]&(0x80>>(i%8)))==0);
        Assert.Equal(U16(mdb,34),free);
        Assert.True(start+count*(long)allocation<=disk.Length-1024);
        var catalogLength=U32(mdb,146);
        var tree=Read(disk,start+allocation,(int)catalogLength);
        Assert.Equal(512,U16(tree,32)); Assert.Equal(2u,U32(tree,20));
        Assert.Equal((uint)(catalogLength/512-2),U32(tree,40));
        var sectors=new List<SectorBlock>{new(2,new(0,0,2),mdb)};
        for(var i=0;i<catalogLength/512;i++)
        {
            var block=checked((int)((start+allocation)/512+i));
            sectors.Add(new(block,new(0,0,block),tree.AsSpan(i*512,512).ToArray()));
        }
        var image=new SectorImage(DiskImageFormatIds.AppleMacHfs,512,1,1,kib*2,sectors);
        var reader=new MacHfsFileSystemReader(); Assert.True(reader.CanRead(image));
        var volume=reader.Read(image); Assert.Equal(label,volume.Name);
        Assert.Equal(free*(long)allocation,volume.FreeBytes); Assert.Empty(volume.Entries); Assert.Empty(volume.Warnings);
    }
    private static byte[] Read(Stream disk,long offset,int length){var data=new byte[length];disk.Position=offset;disk.ReadExactly(data);return data;}
    private static ushort U16(byte[] b,int o)=>BinaryPrimitives.ReadUInt16BigEndian(b.AsSpan(o));
    private static uint U32(byte[] b,int o)=>BinaryPrimitives.ReadUInt32BigEndian(b.AsSpan(o));
}
