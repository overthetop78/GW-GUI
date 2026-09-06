using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Amiga;
using GWGUI.MediaEngine.Migration;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.FileSystems.Amiga.FlatArchive;
using System.Buffers.Binary;
namespace GWGUI.Tests.Media.FileSystems;
internal static class AmigaFileSystemScenarios
{
    public static void FileBlocks(byte variantValue,int damage)
    {
        var variant=(AmigaDosVariant)variantValue;
        byte[] payload=Enumerable.Range(0,700).Select(i=>(byte)(i*7)).ToArray();
        var header=new byte[512]; Write(header,8,2); Write(header,308,3); Write(header,304,7);
        var blocks=new Dictionary<int,byte[]>(); int chunk=variant.IsFastFileSystem()?512:488;
        for(int index=0;index<2;index++)
        {
            var data=new byte[512]; int length=Math.Min(chunk,700-index*chunk);
            payload.AsSpan(index*chunk,length).CopyTo(data.AsSpan(variant.IsFastFileSystem()?0:24));
            if(!variant.IsFastFileSystem()) { Write(data,0,8); Write(data,12,length); Checksum(data); }
            blocks[index==0?3:7]=data;
        }
        if(damage==1) blocks.Remove(7);
        if(damage==2) Write(header,304,99);
        if(damage==3)
        {
            Write(header,504,9); var extension=new byte[512]; Write(extension,0,16); Write(extension,508,-3); Write(extension,504,9); Checksum(extension); blocks[9]=extension;
        }
        if(damage==4) blocks[3][25]^=1;
        var image=new SectorImage("amiga.amigados",512,1,1,12,blocks.Select(p=>new SectorBlock(p.Key,new(0,0,p.Key),p.Value)));
        var warnings=new List<string>(); var read=AmigaDosFileReader.Read(image,header,700,variant,warnings);
        Assert.Equal(damage==0,read.IsValid);
        if(damage==0) { Assert.Equal(payload,read.Content); Assert.Empty(warnings); }
        else Assert.NotEmpty(warnings);
        var empty=AmigaDosFileReader.Read(image,new byte[512],0,variant,[]); Assert.True(empty.IsValid); Assert.Empty(empty.Content);
    }

    public static void Archive(int damage)
    {
        var directory=new byte[512]; "Reserved"u8.CopyTo(directory); Write(directory,12,1536);
        "FILE"u8.CopyTo(directory.AsSpan(16)); Write(directory,28,700); "EMPTY"u8.CopyTo(directory.AsSpan(32)); directory[48]=255;
        byte[] payload=Enumerable.Range(0,700).Select(i=>(byte)(i*7)).ToArray(); var last=new byte[512]; payload.AsSpan(512).CopyTo(last);
        var blocks=new List<SectorBlock>{new(2,new(0,0,2),directory),new(3,new(0,0,3),payload[..512]),new(4,new(0,0,4),last)};
        if(damage==1) blocks.RemoveAt(2);
        if(damage==2) { directory[0]=0; blocks[0]=new(2,new(0,0,2),directory); }
        var image=new SectorImage("amiga.amigados",512,1,1,6,blocks); var reader=new AmigaFlatResourceArchiveReader();
        if(damage==2) { Assert.False(reader.CanRead(image)); Assert.Throws<InvalidDataException>(()=>reader.Read(image)); return; }
        Assert.True(reader.CanRead(image)); var volume=reader.Read(image); Assert.Equal(836,volume.FreeBytes); Assert.Equal(2,volume.Entries.Count);
        Assert.Equal("FILE",volume.Entries[0].Name); Assert.Equal(700,volume.Entries[0].Size); Assert.Empty(volume.Entries[1].Content!);
        if(damage==0) { Assert.Equal(payload,volume.Entries[0].Content); Assert.Empty(volume.Warnings); }
        else { Assert.NotEmpty(volume.Warnings); Assert.Equal(payload.Take(512).Concat(new byte[188]),volume.Entries[0].Content); }
    }

    private static void Write(byte[] bytes,int offset,int value)=>BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(offset),value);
    private static void Checksum(byte[] bytes)
    {
        Write(bytes,20,0); uint sum=0;
        for(int offset=0;offset<512;offset+=4) sum=unchecked(sum+BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset)));
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(20),unchecked(0u-sum));
    }

    public static void Volume(AmigaDosVariant variant)
    {
        var payload=Enumerable.Range(0,700).Select(i=>(byte)(i%251)).ToArray();
        var plan=new MigrationPlan("synthetic",variant.FileSystemId(),"TEST",[new("file","file",FileSystemEntryKind.File,payload,null,"",0,true,[])]);
        var image=new AmigaDosVolumeWriter().Create(plan,variant);
        Assert.Equal(new byte[]{68,79,83,(byte)variant},image.AvailableBlocks.Single(b=>b.LogicalBlock==0).Data.Take(4));
        var reader=new AmigaDosFileSystemReader();
        Assert.True(reader.CanRead(image));
        var volume=reader.Read(image);
        Assert.Equal("TEST",volume.Name);
        var file=Assert.Single(volume.Entries);
        Assert.Equal("file",file.Name);Assert.Equal(700,file.Size);Assert.Equal(payload,file.Content);
        Assert.Throws<InvalidDataException>(()=>new AmigaDosVolumeWriter().Create(plan,AmigaDosVariant.FfsLongNames));
        var writer=new AmigaDosVolumeWriter();
        var nested=writer.Create(new("synthetic",variant.FileSystemId(),"TEST",[new("DIR","DIR",FileSystemEntryKind.Directory,[],null,"",0,true,
            [new("DIR/EMPTY","EMPTY",FileSystemEntryKind.File,[],null,"note",7,true,[])])]),variant);
        var folder=Assert.Single(reader.Read(nested).Entries); Assert.Equal("DIR",folder.Name); Assert.Equal(FileSystemEntryKind.Directory,folder.Kind);
        var empty=Assert.Single(folder.Children); Assert.Equal("EMPTY",empty.Name); Assert.Empty(empty.Content!); Assert.Equal("note",empty.Comment); Assert.Equal((uint)7,empty.RawAttributes);
        Assert.Throws<InvalidDataException>(()=>writer.Create(new("synthetic",variant.FileSystemId(),"TEST",[new("BIG","BIG",FileSystemEntryKind.File,new byte[901120],null,"",0,true,[])]),variant));
        Assert.Throws<InvalidDataException>(()=>writer.Create(new("synthetic",variant.FileSystemId(),"TEST",[new("BAD",new string('A',31),FileSystemEntryKind.File,[],null,"",0,true,[])]),variant));
    }
}
