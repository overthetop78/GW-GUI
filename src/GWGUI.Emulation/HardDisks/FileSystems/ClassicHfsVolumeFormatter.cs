using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>Classic HFS, with 512-byte B-tree nodes and an empty root directory.</summary>
public static class ClassicHfsVolumeFormatter
{
    public static void Validate(long capacity,string label)
    {
        if(capacity < 128L<<10 || capacity > 4L<<30 || capacity%512!=0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        _=ClassicMacLabel.Encode(label);
    }
    public static void Format(Stream volume,string label)
    {
        Validate(volume.Length,label);
        // Sixteen bitmap sectors cover the format's full 16-bit allocation space.
        const int allocationStart=19;
        var sectors=volume.Length/512; var allocationSectors=1;
        while((sectors-allocationStart-2)/allocationSectors>65535) allocationSectors*=2;
        var allocationSize=allocationSectors*512;
        var count=checked((int)((sectors-allocationStart-2)/allocationSectors));
        var catalogBlocks=(1024+allocationSize-1)/allocationSize;
        var used=1+catalogBlocks;
        var bitmap=new byte[8192];
        for(var i=0;i<used;i++) bitmap[i/8]|=(byte)(0x80>>(i%8));
        for(var i=count;i<bitmap.Length*8;i++) bitmap[i/8]|=(byte)(0x80>>(i%8));
        var mdb=new byte[512]; U16(mdb,0,0x4244); U16(mdb,10,0x100); U16(mdb,14,3);
        U16(mdb,16,used); U16(mdb,18,count); U32(mdb,20,(uint)allocationSize); U32(mdb,24,(uint)allocationSize);
        U16(mdb,28,allocationStart); U32(mdb,30,16); U16(mdb,34,count-used);
        var name=ClassicMacLabel.Encode(label); mdb[36]=(byte)name.Length; name.CopyTo(mdb,37);
        U32(mdb,70,1); U32(mdb,74,(uint)allocationSize); U32(mdb,78,(uint)(catalogBlocks*allocationSize));
        U32(mdb,130,(uint)allocationSize); U16(mdb,134,0); U16(mdb,136,1);
        U32(mdb,146,(uint)(catalogBlocks*allocationSize)); U16(mdb,150,1); U16(mdb,152,catalogBlocks);
        Put(volume,0,new byte[1024]); Put(volume,1024,mdb); Put(volume,1536,bitmap);
        Put(volume,allocationStart*512L,ClassicHfsBTreeWriter.Create(allocationSize,null));
        Put(volume,allocationStart*512L+allocationSize,ClassicHfsBTreeWriter.Create(catalogBlocks*allocationSize,label));
        Put(volume,volume.Length-1024,mdb); Put(volume,volume.Length-512,new byte[512]); volume.Flush();
    }
    private static void U16(byte[] bytes,int offset,int value)=>BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(offset),checked((ushort)value));
    private static void U32(byte[] bytes,int offset,uint value)=>BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset),value);
    private static void Put(Stream stream,long offset,byte[] bytes){stream.Position=offset;stream.Write(bytes);}
}
