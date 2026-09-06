using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Mfs;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.SectorImages;
using System.Buffers.Binary;
namespace GWGUI.Tests.Media.FileSystems;
internal static class MacintoshFileSystemScenarios
{
    public static void Hfs(bool resourceFork,int damage)
    {
        var bytes=new byte[4608];
        void Word(int offset,ushort value)=>BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(offset),value);
        void Long(int offset,uint value)=>BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset),value);
        Word(1024,0x4244); Word(1042,5); Long(1044,512); Word(1052,4); Word(1058,1); bytes[1060]=6; "VOLUME"u8.CopyTo(bytes.AsSpan(1061));
        Long(1170,1024); Word(1174,0); Word(1176,2); Word(2080,512); // Catalog in allocation blocks 0 and 1.
        const int leaf=2560; bytes[leaf+8]=0xff; Word(leaf+10,damage==2?(ushort)513:(ushort)3);
        void Key(int offset,uint parent,string name)
        { bytes[leaf+offset]=(byte)(6+name.Length); Long(leaf+offset+2,parent); bytes[leaf+offset+6]=(byte)name.Length; System.Text.Encoding.ASCII.GetBytes(name).CopyTo(bytes,leaf+offset+7); }
        Key(14,2,"DIR"); bytes[leaf+24]=1; Long(leaf+30,3);
        Key(94,3,"FILE"); bytes[leaf+106]=2; "TEXT"u8.CopyTo(bytes.AsSpan(leaf+110)); Long(leaf+126,4);
        Long(leaf+106+(resourceFork?36:26),700);
        var extent=leaf+106+(resourceFork?86:74); Word(extent,2); Word(extent+2,1); Word(extent+4,4); Word(extent+6,1);
        Key(208,2,"EMPTY"); bytes[leaf+220]=2; Long(leaf+240,5);
        Word(leaf+510,14); Word(leaf+508,94); Word(leaf+506,208); Word(leaf+504,322);
        var payload=Enumerable.Range(0,700).Select(i=>(byte)(i%251)).ToArray(); payload.AsSpan(0,512).CopyTo(bytes.AsSpan(3072)); payload.AsSpan(512).CopyTo(bytes.AsSpan(4096));
        var image=new SectorImage(DiskImageFormatIds.Mac800,512,1,1,9,Enumerable.Range(0,9).Where(index=>damage!=1||index!=8).Select(index=>new SectorBlock(index,new(0,0,index),bytes.AsSpan(index*512,512).ToArray())));
        var reader=new MacHfsFileSystemReader(); Assert.True(reader.CanRead(image)); var volume=reader.Read(image); Assert.Equal("VOLUME",volume.Name); Assert.Equal(2560,volume.Capacity); Assert.Equal(512,volume.FreeBytes);
        if(damage==2) { Assert.Empty(volume.Entries); Assert.NotEmpty(volume.Warnings); }
        else
        {
            Assert.Equal(2,volume.Entries.Count); var directory=Assert.Single(volume.Entries,entry=>entry.Name=="DIR"); var file=Assert.Single(directory.Children);
            Assert.Equal("FILE",file.Name); Assert.Equal(700,file.Size); Assert.Equal("TEXT",file.Comment); Assert.Equal(4,file.StorageReference); Assert.Equal(damage==0,file.MetadataValid);
            if(damage==0) { Assert.Equal(payload,file.Content); Assert.Empty(volume.Warnings); } else Assert.NotEmpty(volume.Warnings);
            Assert.Empty(Assert.Single(volume.Entries,entry=>entry.Name=="EMPTY").Content!);
        }
        Long(1170,32);
        var truncated=new SectorImage(image.FormatId,512,1,1,9,Enumerable.Range(0,9).Select(index=>new SectorBlock(index,new(0,0,index),bytes.AsSpan(index*512,512).ToArray())));
        Assert.Throws<InvalidDataException>(()=>reader.Read(truncated));
    }
    public static void Mfs(bool resourceFork,int damage)
    {
        var bytes=new byte[4096];
        void Word(int offset,ushort value)=>BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(offset),value);
        void Long(int offset,uint value)=>BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset),value);
        Word(1024,0xd2d7); Word(1038,4); Word(1040,1); Word(1042,3); Long(1044,512); Word(1052,5); Word(1058,1); bytes[1060]=6; "VOLUME"u8.CopyTo(bytes.AsSpan(1061));
        // Allocation 2 -> 4 -> end, deliberately skipping free allocation 3.
        bytes[1088]=0; bytes[1089]=0x40; bytes[1090]=0; bytes[1091]=0xff; bytes[1092]=0x10;
        if(damage==2) bytes[1089]=0x20;
        if(damage==3) { bytes[1088]=0x3e; bytes[1089]=0x70; }
        bytes[2048]=0x80; "TEXT"u8.CopyTo(bytes.AsSpan(2050)); Long(2066,7);
        Word(2048+(resourceFork?32:22),2); Long(2048+(resourceFork?34:24),700); bytes[2098]=4; "FILE"u8.CopyTo(bytes.AsSpan(2099));
        bytes[2104]=0x80; Long(2122,8); bytes[2154]=5; "EMPTY"u8.CopyTo(bytes.AsSpan(2155));
        var payload=Enumerable.Range(0,700).Select(i=>(byte)(i%251)).ToArray(); payload.AsSpan(0,512).CopyTo(bytes.AsSpan(2560)); payload.AsSpan(512).CopyTo(bytes.AsSpan(3584));
        var blocks=Enumerable.Range(0,8).Where(index=>damage!=1||index!=7).Select(index=>new SectorBlock(index,new(0,0,index),bytes.AsSpan(index*512,512).ToArray()));
        var image=new SectorImage(DiskImageFormatIds.Mac400,512,1,1,8,blocks); var reader=new MacMfsFileSystemReader(); Assert.True(reader.CanRead(image)); var volume=reader.Read(image);
        Assert.Equal("VOLUME",volume.Name); Assert.Equal(512,volume.FreeBytes); Assert.Equal(2,volume.Entries.Count);
        var file=Assert.Single(volume.Entries,entry=>entry.Name=="FILE"); Assert.Equal(700,file.Size); Assert.Equal("TEXT",file.Comment); Assert.Equal(7,file.StorageReference);
        Assert.Equal(damage==0,file.MetadataValid); if(damage==0) { Assert.Equal(payload,file.Content); Assert.Empty(volume.Warnings); } else Assert.NotEmpty(volume.Warnings);
        var empty=Assert.Single(volume.Entries,entry=>entry.Name=="EMPTY"); Assert.Equal(0,empty.Size); Assert.Empty(empty.Content!); Assert.True(empty.MetadataValid);
        Long(1044,1);
        var invalid=new SectorImage(image.FormatId,512,1,1,8,Enumerable.Range(0,8).Select(index=>new SectorBlock(index,new(0,0,index),bytes.AsSpan(index*512,512).ToArray())));
        Assert.Throws<InvalidDataException>(()=>reader.Read(invalid));
    }
}
