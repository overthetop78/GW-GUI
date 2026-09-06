using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Apple.ProDos;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Migration;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.FileSystems.Apple.Dos;
using GWGUI.MediaEngine.FileSystems.Apple.Lisa;
using GWGUI.MediaEngine.FileSystems.Apple.InformXzip;
using System.Buffers.Binary;
namespace GWGUI.Tests.Media.FileSystems;
internal static class AppleFileSystemScenarios
{
    public static void Dos(bool thirteen,int damage)
    {
        string format=thirteen?DiskImageFormatIds.AppleIIDos32:DiskImageFormatIds.AppleIIDos33;
        var writer=new AppleDosVolumeWriter(); byte[] payload=Enumerable.Range(0,700).Select(i=>(byte)(i*7)).ToArray();
        var plan=new MigrationPlan("synthetic","prodos","DOS-007",[new("FILE","FILE",FileSystemEntryKind.File,payload,null,"",0,true,[]),new("EMPTY","EMPTY",FileSystemEntryKind.File,[],null,"",0,true,[])]);
        var original=writer.Create(plan,format); var blocks=original.AvailableBlocks.ToDictionary(b=>b.LogicalBlock,b=>b.Data.ToArray()); int sectors=thirteen?13:16;
        var vtoc=blocks[17*sectors]; Assert.Equal(7,vtoc[6]); Assert.Equal(sectors,vtoc[53]);
        var catalog=blocks[vtoc[1]*sectors+vtoc[2]]; int listBlock=catalog[11]*sectors+catalog[12]; var list=blocks[listBlock];
        if(damage==1) { list[1]=(byte)(listBlock/sectors); list[2]=(byte)(listBlock%sectors); }
        if(damage==2) list[12]=99;
        if(damage==3) blocks.Remove(list[12]*sectors+list[13]);
        if(damage==4) blocks.Remove(listBlock);
        var image=new SectorImage(format,256,35,1,sectors,blocks.Select(p=>new SectorBlock(p.Key,new(p.Key/sectors,0,p.Key%sectors),p.Value)));
        var reader=new AppleDosFileSystemReader(); Assert.True(reader.CanRead(image)); var volume=reader.Read(image); Assert.Equal("DOS-007",volume.Name);
        var file=Assert.Single(volume.Entries,e=>e.Name=="FILE"); Assert.Equal(damage==0,file.MetadataValid);
        if(damage==0) { Assert.Equal(payload,file.Content); Assert.Empty(volume.Warnings); } else Assert.NotEmpty(volume.Warnings);
        Assert.Empty(Assert.Single(volume.Entries,e=>e.Name=="EMPTY").Content!);
        Assert.Throws<InvalidDataException>(()=>writer.Create(new("synthetic","prodos","DOS-007",[new("DIR","DIR",FileSystemEntryKind.Directory,[],null,"",0,true,[])]),format));
        Assert.Throws<InvalidDataException>(()=>writer.Create(new("synthetic","prodos","DOS-007",Enumerable.Range(0,10).Select(i=>new MigrationEntry("F"+i,"F"+i,FileSystemEntryKind.File,new byte[16000],null,"",0,true,[])).ToArray()),format));
    }

    public static void ProDosStorage(int storage,int damage)
    {
        var blocks=new Dictionary<int,byte[]> { [3]=new byte[512], [5]=new byte[512], [7]=Enumerable.Repeat((byte)42,512).ToArray(), [9]=Enumerable.Repeat((byte)93,512).ToArray() };
        blocks[3][0]=7; blocks[3][2]=9; blocks[5][0]=3;
        int key=storage==1?7:storage==2?3:5, length=storage==1?200:1200;
        if(damage==1) blocks.Remove(7);
        if(damage==2) blocks[3][2]=99;
        if(damage==3) blocks[5][0]=5;
        var image=new SectorImage(DiskImageFormatIds.AppleIIProDos140,512,1,1,12,blocks.Select(p=>new SectorBlock(p.Key,new(0,0,p.Key),p.Value)));
        var warnings=new List<string>(); var read=ProDosFileContentReader.Read(image,(ProDosStorageType)storage,key,length,"FILE",warnings);
        Assert.Equal(damage==0,read.IsValid); Assert.Equal(length,read.Content.Count);
        if(damage==0)
        {
            Assert.Equal(storage==1?Enumerable.Repeat((byte)42,200):Enumerable.Repeat((byte)42,512).Concat(new byte[512]).Concat(Enumerable.Repeat((byte)93,176)),read.Content);
            Assert.Empty(warnings);
        }
        else Assert.NotEmpty(warnings);
    }

    public static void Lisa(ushort version,int damage)
    {
        var header=new byte[512]; BinaryPrimitives.WriteUInt16BigEndian(header,version); header[12]=4; "TEST"u8.CopyTo(header.AsSpan(13));
        var catalog=new byte[512]; int offset=version==14?0:80; catalog[offset]=version==14?(byte)4:(byte)0; "FILE"u8.CopyTo(catalog.AsSpan(offset+1)); catalog[offset+37]=5;
        SectorBlock Block(int logical,ushort file,ushort page,byte[] data)
        {
            var tag=new byte[12]; BinaryPrimitives.WriteUInt16BigEndian(tag.AsSpan(4),file); BinaryPrimitives.WriteUInt16BigEndian(tag.AsSpan(6),page);
            return new(logical,new(0,0,logical),data,Tag:tag);
        }
        var blocks=new List<SectorBlock>{Block(0,1,0,header),Block(1,4,0,catalog),Block(7,5,(ushort)(damage==1?2:damage==2?0:1),Enumerable.Repeat((byte)93,512).ToArray()),Block(3,5,0,Enumerable.Repeat((byte)42,512).ToArray()),Block(9,0,0,new byte[512])};
        if(damage==3) blocks[0]=Block(0,1,0,new byte[32]);
        var image=new SectorImage(DiskImageFormatIds.AppleLisaOffice,512,1,1,10,blocks,allowVariableBlockSize:true); var reader=new LisaFileSystemReader(); Assert.True(reader.CanRead(image));
        if(damage==3) { Assert.Throws<InvalidDataException>(()=>reader.Read(image)); return; }
        var volume=reader.Read(image); Assert.Equal("TEST",volume.Name); Assert.Equal(512,volume.FreeBytes); var file=Assert.Single(volume.Entries); Assert.Equal("FILE",file.Name); Assert.Equal(damage==0,file.MetadataValid);
        var expected=Enumerable.Repeat((byte)42,512);
        if(damage==1) expected=expected.Concat(new byte[512]);
        if(damage!=2) expected=expected.Concat(Enumerable.Repeat((byte)93,512));
        Assert.Equal(expected,file.Content); if(damage==0) Assert.Empty(volume.Warnings); else Assert.NotEmpty(volume.Warnings);
    }

    public static void Inform(int damage)
    {
        var story=new byte[256]; story[0]=5;
        foreach(int offset in new[]{4,6,8,10,12,14}) BinaryPrimitives.WriteUInt16BigEndian(story.AsSpan(offset),64);
        BinaryPrimitives.WriteUInt16BigEndian(story.AsSpan(26),64); story[64]=42; story[255]=93; BinaryPrimitives.WriteUInt16BigEndian(story.AsSpan(28),135);
        if(damage==1) story[64]^=1;
        if(damage==2) story[0]=3;
        var blocks=Enumerable.Range(0,64).Select(i=>new SectorBlock(i,new(i/16,0,i%16),Enumerable.Repeat((byte)i,256).ToArray())).ToList();
        if(damage!=3) blocks.Add(new(64,new(4,0,0),story));
        var image=new SectorImage(DiskImageFormatIds.AppleIIDos33,256,35,1,16,blocks); var reader=new AppleInformXzipFileSystemReader();
        if(damage!=0) { Assert.False(reader.CanRead(image)); Assert.Throws<InvalidDataException>(()=>reader.Read(image)); return; }
        Assert.True(reader.CanRead(image)); var volume=reader.Read(image); Assert.Equal(2,volume.Entries.Count); Assert.Equal("STORY.Z5",volume.Entries[1].Name); Assert.Equal(story,volume.Entries[1].Content);
        Assert.Equal(Enumerable.Range(0,64).SelectMany(i=>Enumerable.Repeat((byte)i,256)),volume.Entries[0].Content); Assert.Equal(126720,volume.FreeBytes);
    }

    public static void Volume()
    {
        var payload=Enumerable.Range(0,700).Select(i=>(byte)(i%251)).ToArray();
        var plan=new MigrationPlan("synthetic","prodos","TEST",[new("FILE","FILE",FileSystemEntryKind.File,payload,null,"",0,true,[])]);
        var image=new ProDosVolumeWriter().Create(plan,DiskImageFormatIds.AppleIIProDos140);
        Assert.Equal(143360,image.Capacity);
        Assert.Equal(0xf4,image.AvailableBlocks.Single(b=>b.LogicalBlock==2).Data[4]);
        var reader=new ProDosFileSystemReader();
        Assert.True(reader.CanRead(image));
        var volume=reader.Read(image);
        Assert.Equal("TEST",volume.Name);
        var file=Assert.Single(volume.Entries);
        Assert.Equal("FILE",file.Name);Assert.Equal(700,file.Size);Assert.Equal(payload,file.Content);
        var writer=new ProDosVolumeWriter();
        var nested=writer.Create(new("synthetic","prodos","TEST",[new("DIR","DIR",FileSystemEntryKind.Directory,[],null,"",0,true,[new("DIR/EMPTY","EMPTY",FileSystemEntryKind.File,[],null,"",0,true,[])])]),DiskImageFormatIds.AppleIIProDos800);
        Assert.Equal(819200,nested.Capacity); var folder=Assert.Single(reader.Read(nested).Entries); Assert.Equal("DIR",folder.Name); Assert.Empty(Assert.Single(folder.Children).Content!);
        Assert.Throws<InvalidDataException>(()=>writer.Create(new("synthetic","prodos","TEST",[new("BIG","BIG",FileSystemEntryKind.File,new byte[143360],null,"",0,true,[])]),DiskImageFormatIds.AppleIIProDos140));
    }
}
