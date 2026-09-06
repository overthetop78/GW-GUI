using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Definitions;
using System.Buffers.Binary;
namespace GWGUI.Tests.Media.ImageContainers;
internal static class AppleContainerScenarios
{
    public static async Task Woz2(int damage)
    {
        var payload=Enumerable.Range(0,256).Select(i=>(byte)i).ToArray();
        var bits=new GWGUI.MediaEngine.Encoding.AppleIIGcrTrackEncoder().Encode(new(0,0,[new(0,payload)],new Dictionary<string,int>{{"sectorsPerTrack",16}})).Bits;
        int blockCount=(bits.Count+4095)/4096; var bytes=new byte[512+blockCount*512];
        "WOZ2"u8.CopyTo(bytes); new byte[]{255,10,13,10}.CopyTo(bytes,4); "INFO"u8.CopyTo(bytes.AsSpan(12)); bytes[16]=2; bytes[20]=2; bytes[21]=1;
        "TMAP"u8.CopyTo(bytes.AsSpan(22)); bytes[26]=160; Array.Fill(bytes,(byte)255,30,160); bytes[30]=0;
        "TRKS"u8.CopyTo(bytes.AsSpan(190)); BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(194),bytes.Length-198);
        bytes[198]=1; BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(200),(ushort)blockCount); BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(202),bits.Count);
        for(int bit=0;bit<bits.Count;bit++) if(bits[bit]) bytes[512+bit/8]|=(byte)(128>>(bit%8));
        if(damage==2) bytes[198]=255;
        if(damage==3) bytes[30]=159;
        if(damage==4) bytes[21]=2;
        uint crc=uint.MaxValue; foreach(byte value in bytes.Skip(12)) { crc^=value; for(int bit=0;bit<8;bit++) crc=(crc&1)!=0?(crc>>1)^0xedb88320:crc>>1; }
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(8),~crc); if(damage==1) bytes[8]^=1;
        var reader=new AppleDiskImageReader();
        if(damage==4) { await Assert.ThrowsAsync<NotSupportedException>(()=>reader.ReadAsync(bytes.AsMemory(),".woz",null)); return; }
        if(damage!=0) { await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync(bytes.AsMemory(),".woz",null)); return; }
        var image=await reader.ReadAsync(bytes.AsMemory(),".wrong",null); Assert.Equal(payload,Assert.Single(image.AvailableBlocks).Data);
    }

    public static async Task WriterFacade(bool woz,bool rwts)
    {
        int sectors=rwts?6:16, size=rwts?768:256;
        var blocks=Enumerable.Range(0,sectors*2).Select(i=>new GWGUI.MediaEngine.SectorImages.SectorBlock(i,new(i/sectors,0,i%sectors),Enumerable.Range(0,size).Select(j=>(byte)(j+i)).ToArray())).ToArray();
        var image=new GWGUI.MediaEngine.SectorImages.SectorImage(rwts?DiskImageFormatIds.AppleIIRwts18:DiskImageFormatIds.AppleIIDos33,size,2,1,sectors,blocks);
        var files=new Dictionary<string,byte[]>(); var writer=new AppleDiskImageWriter(writeBytes:(path,bytes,token)=>{token.ThrowIfCancellationRequested();files[path]=bytes;return Task.CompletedTask;});
        string path=woz?"output.woz":"output.nib"; await writer.WriteAsync(image,path);
        var read=await new AppleDiskImageReader().ReadAsync(files[path].AsMemory(),woz?".woz":".nib",null);
        Assert.Equal(sectors*2,read.AvailableBlocks.Count);
        foreach(var block in blocks) Assert.Equal(block.Data,read.AvailableBlocks.Single(b=>b.Address.Cylinder==block.Address.Cylinder&&b.Address.Number==block.Address.Number).Data);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>writer.WriteAsync(image,path,new CancellationToken(true)));
        await Assert.ThrowsAsync<NotSupportedException>(()=>writer.WriteAsync(image,"bad.ext"));
        var incomplete=new GWGUI.MediaEngine.SectorImages.SectorImage(image.FormatId,size,2,1,sectors,blocks.Skip(1));
        await Assert.ThrowsAsync<InvalidDataException>(()=>writer.WriteAsync(incomplete,path));
    }

    public static async Task RawVariant(int kind)
    {
        var plan=new GWGUI.MediaEngine.Migration.MigrationPlan("synthetic","prodos",kind<2?"DOS-007":"TEST",[]);
        var source=kind switch
        {
            0=>new GWGUI.MediaEngine.FileSystems.Apple.Dos.AppleDosVolumeWriter().Create(plan,DiskImageFormatIds.AppleIIDos32),
            1=>new GWGUI.MediaEngine.FileSystems.Apple.Dos.AppleDosVolumeWriter().Create(plan,DiskImageFormatIds.AppleIIDos33),
            2=>new GWGUI.MediaEngine.FileSystems.Sos.SosVolumeWriter().Create(plan),
            _=>new GWGUI.MediaEngine.FileSystems.Apple.ProDos.ProDosVolumeWriter().Create(plan,DiskImageFormatIds.AppleIIProDos800)
        };
        string target=kind switch {0=>DiskImageFormatIds.AppleIIAppleDos113,1=>DiskImageFormatIds.AppleIIAppleDos140,2=>DiskImageFormatIds.AppleIIISos,_=>DiskImageFormatIds.AppleIIProDos800};
        string extension=kind switch{0=>".d13",1=>".do",_=>".po"}; var files=new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles();
        var writer=new GWGUI.MediaEngine.Containers.Apple.Raw.AppleRawImageWriter(files); await writer.WriteAsync(source,"out"+extension,target);
        byte[] bytes=files.Files["out"+extension]; Assert.Equal(source.AvailableBlocks.OrderBy(b=>b.LogicalBlock).SelectMany(b=>b.Data),bytes);
        var read=await new AppleDiskImageReader().ReadAsync(bytes.AsMemory(),extension,null); Assert.Equal(source.Capacity,read.Capacity); Assert.Equal(source.BlockSize,read.BlockSize);
        Assert.Equal(kind==2?DiskImageFormatIds.AppleIIISos:kind==3?DiskImageFormatIds.AppleIIProDos:source.FormatId,read.FormatId);
        await new GWGUI.MediaEngine.Containers.Apple.TwoImg.TwoImgWriter(files).WriteAsync(source,"out.2mg",target);
        var restored=await new AppleDiskImageReader().ReadAsync(files.Files["out.2mg"].AsMemory(),".2mg",null);
        Assert.Equal(bytes,restored.AvailableBlocks.OrderBy(b=>b.LogicalBlock).SelectMany(b=>b.Data));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>writer.WriteAsync(source,"cancelled"+extension,target,new CancellationToken(true)));
    }

    public static async Task LisaDiskCopy(int count)
    {
        var blocks=Enumerable.Range(0,count).Select(i=>new GWGUI.MediaEngine.SectorImages.SectorBlock(i,new(0,0,i),Enumerable.Repeat((byte)(i%251),512).ToArray(),Tag:new byte[]{0,0,0,0,0,1,0,(byte)i,0,0,0,0})).ToArray();
        var image=new GWGUI.MediaEngine.SectorImages.SectorImage(DiskImageFormatIds.AppleLisaOffice,512,1,1,count,blocks);
        var files=new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles(); var writer=new GWGUI.MediaEngine.Containers.Apple.DiskCopy.DiskCopyWriter(files);
        await writer.WriteAsync(image,"out.dc42",new(image,[84,69,83,84],0,0)); var bytes=files.Files["out.dc42"];
        var read=GWGUI.MediaEngine.Containers.Apple.DiskCopy.DiskCopyReader.ReadDetailed(bytes).Image;
        Assert.Equal(count,read.BlockCount); Assert.Equal(DiskImageFormatIds.AppleLisaOffice,read.FormatId);
        Assert.Equal(count==1702?46:count==800?80:1,read.Cylinders); Assert.Equal(count==1702?2:1,read.Heads);
        Assert.Equal(blocks.SelectMany(b=>b.Data),read.AvailableBlocks.OrderBy(b=>b.LogicalBlock).SelectMany(b=>b.Data));
        Assert.Equal(blocks[^1].Tag,read.AvailableBlocks.Single(b=>b.LogicalBlock==count-1).Tag);
        bytes[^1]^=1; Assert.Throws<InvalidDataException>(()=>GWGUI.MediaEngine.Containers.Apple.DiskCopy.DiskCopyReader.Read(bytes));
        bytes[^1]^=1; BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(68),1); Assert.Throws<InvalidDataException>(()=>GWGUI.MediaEngine.Containers.Apple.DiskCopy.DiskCopyReader.Read(bytes));
    }

    public static async Task BitContainer(bool woz,int sectors)
    {
        var payload = Enumerable.Range(0,256).Select(x=>(byte)x).ToArray();
        var encoded = new GWGUI.MediaEngine.Encoding.AppleIIGcrTrackEncoder().Encode(new(0,0,
            [new GWGUI.MediaEngine.Encoding.TrackSector(0,payload)],new Dictionary<string,int>{{"sectorsPerTrack",sectors}}));
        var files = new Dictionary<string,byte[]>();
        Task Store(string path,byte[] bytes,CancellationToken token) { token.ThrowIfCancellationRequested(); files[path]=bytes; return Task.CompletedTask; }
        Task Write(IReadOnlyList<IReadOnlyList<bool>> tracks,string path,CancellationToken token = default) => woz
            ? GWGUI.MediaEngine.Containers.Apple.Woz.WozWriter.WriteAsync(tracks,path,token,Store)
            : GWGUI.MediaEngine.Containers.Apple.Nib.NibWriter.WriteAsync(tracks,path,token,Store);
        await Write([encoded.Bits],"output"); var written = files["output"];
        if(woz)
        {
            Assert.Equal("WOZ1"u8.ToArray(),written.Take(4)); Assert.Equal(new byte[]{255,10,13,10},written.Skip(4).Take(4));
            Assert.Equal("INFO"u8.ToArray(),written.Skip(12).Take(4));
            uint crc=uint.MaxValue; foreach(var value in written.Skip(12)) { crc ^= value; for(var bit=0;bit<8;bit++) crc=(crc&1)!=0?(crc>>1)^0xedb88320:crc>>1; }
            Assert.Equal(~crc,BinaryPrimitives.ReadUInt32LittleEndian(written.AsSpan(8)));
        }
        else
        {
            Assert.Equal(6656,written.Length); Assert.Equal(255,written[^1]);
            var wrapper=new byte[64+written.Length]; "2IMG"u8.CopyTo(wrapper); wrapper[8]=64; wrapper[10]=1; wrapper[12]=2; wrapper[24]=64;
            BinaryPrimitives.WriteInt32LittleEndian(wrapper.AsSpan(28),written.Length); written.CopyTo(wrapper,64);
            var wrapped=await new AppleDiskImageReader().ReadAsync(wrapper.AsMemory(),".2mg",null); Assert.Equal(payload,Assert.Single(wrapped.AvailableBlocks).Data);
        }
        var image = await new AppleDiskImageReader().ReadAsync(written.AsMemory(),woz?".woz":".nib",null);
        var first = Assert.Single(image.AvailableBlocks); Assert.Equal(payload,first.Data); Assert.Equal(0,first.Address.Cylinder); Assert.Equal(0,first.Address.Number);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>Write([encoded.Bits],"cancelled",new CancellationToken(true)));
        await Assert.ThrowsAsync<InvalidDataException>(()=>Write([],"invalid"));
        Assert.Single(files);
        var invalid = (byte[])written.Clone(); if(woz) invalid[^1]^=1; else invalid=invalid[..^1];
        await Assert.ThrowsAsync<InvalidDataException>(()=>new AppleDiskImageReader().ReadAsync(invalid.AsMemory(),woz?".woz":".nib",null));
    }
    public static async Task Macintosh(int capacity,bool tagged)
    {
        var data = new byte[capacity]; data[0]=42; data[1024]=0x42; data[1025]=0x44; data[^1]=93;
        var reader = new AppleDiskImageReader(); var image = await reader.ReadAsync(data.AsMemory(),".img",null);
        Assert.Equal(capacity/512,image.BlockCount); Assert.Equal(capacity==409600?1:2,image.Heads);
        var ordered = image.AvailableBlocks.OrderBy(x=>x.LogicalBlock).ToArray();
        Assert.Equal(0,ordered[0].Address.Number); Assert.Equal(79,ordered[^1].Address.Cylinder);
        Assert.Equal(capacity==1474560?17:7,ordered[^1].Address.Number);
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles();
        await new GWGUI.MediaEngine.Containers.Apple.Raw.MacintoshRawImageWriter(files).WriteAsync(image,"raw"); Assert.Equal(data,files.Files["raw"]);
        if(tagged)
        {
            var blocks = ordered.Select((block,index)=>new GWGUI.MediaEngine.SectorImages.SectorBlock(block.LogicalBlock,block.Address,block.Data,Tag:Enumerable.Repeat((byte)(index%251),12).ToArray())).ToArray();
            image = new(image.FormatId,512,80,image.Heads,image.SectorsPerTrack,blocks,capacity:capacity,logicalBlockCount:blocks.Length);
        }
        var writer = new GWGUI.MediaEngine.Containers.Apple.DiskCopy.DiskCopyWriter(files); await writer.WriteAsync(image,"disk.dc42"); var output = files.Files["disk.dc42"];
        Assert.Equal(capacity+84+(tagged?capacity/512*12:0),output.Length);
        Assert.Equal(4,output[0]); Assert.Equal("disk"u8.ToArray(),output.Skip(1).Take(4));
        Assert.Equal((uint)capacity,BinaryPrimitives.ReadUInt32BigEndian(output.AsSpan(64)));
        Assert.Equal(new byte[]{1,0},output.Skip(82).Take(2)); Assert.Equal(data,output.Skip(84).Take(capacity));
        uint checksum=0; for(var index=0;index<data.Length;index+=2) { checksum=unchecked(checksum+(uint)(data[index]*256+data[index+1])); checksum=(checksum>>1)|(checksum<<31); }
        Assert.Equal(checksum,BinaryPrimitives.ReadUInt32BigEndian(output.AsSpan(72)));
        var decoded = GWGUI.MediaEngine.Containers.Apple.DiskCopy.DiskCopyReader.ReadDetailed(output);
        Assert.Equal(data,decoded.Image.AvailableBlocks.OrderBy(x=>x.LogicalBlock).SelectMany(x=>x.Data));
        if(tagged) Assert.Equal(Enumerable.Repeat((byte)1,12),decoded.Image.AvailableBlocks.Single(x=>x.LogicalBlock==1).Tag!);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>writer.WriteAsync(image,"cancelled",cancellationToken:new CancellationToken(true)));
        output[84]^=1; Assert.Throws<InvalidDataException>(()=>GWGUI.MediaEngine.Containers.Apple.DiskCopy.DiskCopyReader.Read(output));
    }
    public static async Task Read(bool container)
    {
        var offset = container ? 64 : 0;
        var bytes = new byte[143360 + offset]; bytes[offset] = 42; bytes[^1] = 93;
        if (container)
        {
            "2IMG"u8.CopyTo(bytes); bytes[8] = 64; bytes[10] = 1; bytes[12] = 1; bytes[24] = 64;
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(28), 143360);
        }
        var reader = new AppleDiskImageReader(); var image = await reader.ReadAsync(bytes.AsMemory(), container ? ".unexpected" : ".po", null);
        Assert.Equal(DiskImageFormatIds.AppleIIProDos, image.FormatId); Assert.Equal(512, image.BlockSize); Assert.Equal(280, image.BlockCount);
        Assert.Equal(42, image.AvailableBlocks.Single(block => block.LogicalBlock == 0).Data[0]);
        Assert.Equal(93, image.AvailableBlocks.Single(block => block.LogicalBlock == 279).Data[^1]);
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles();
        await new GWGUI.MediaEngine.Containers.Apple.Raw.AppleRawImageWriter(files).WriteAsync(image,"raw.po",DiskImageFormatIds.AppleIIProDos140);
        Assert.Equal(bytes.Skip(offset),files.Files["raw.po"]);
        await new GWGUI.MediaEngine.Containers.Apple.TwoImg.TwoImgWriter(files).WriteAsync(image,"output.2mg",DiskImageFormatIds.AppleIIProDos140);
        var output = files.Files["output.2mg"]; Assert.Equal(143424,output.Length);
        Assert.Equal("2IMG"u8.ToArray(),output.Take(4)); Assert.Equal(64,output[8]); Assert.Equal(1,output[10]); Assert.Equal(1,output[12]);
        Assert.Equal(280u,BinaryPrimitives.ReadUInt32LittleEndian(output.AsSpan(20))); Assert.Equal(64u,BinaryPrimitives.ReadUInt32LittleEndian(output.AsSpan(24)));
        Assert.Equal(bytes.Skip(offset),output.Skip(64));
        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(bytes.AsMemory(0, bytes.Length - 1), ".po", null));
        using var source = new CancellationTokenSource(); source.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => reader.ReadAsync(bytes.AsMemory(), ".po", null, source.Token));
    }

    public static async Task RawStructures(bool lisa)
    {
        var bytes=new byte[409600];
        if(lisa) { bytes[1]=14; bytes[12]=4; "TEST"u8.CopyTo(bytes.AsSpan(13)); }
        else { bytes[1024]=0xd2; bytes[1025]=0xd7; }
        var reader=new AppleDiskImageReader(); var image=await reader.ReadAsync(bytes.AsMemory(),".img",null);
        Assert.Equal(lisa?DiskImageFormatIds.AppleLisaRaw:DiskImageFormatIds.AppleMacMfs,image.FormatId); Assert.Equal(800,image.BlockCount);
        var files=new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles(); await new GWGUI.MediaEngine.Containers.Apple.Raw.MacintoshRawImageWriter(files).WriteAsync(image,"out.img"); Assert.Equal(bytes,files.Files["out.img"]);
        Array.Clear(bytes); await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync(bytes.AsMemory(),".img",null));
    }

    public static async Task TwoImgInvalid(int damage)
    {
        var bytes=new byte[143424]; "2IMG"u8.CopyTo(bytes); bytes[8]=64; bytes[10]=1; bytes[12]=1; bytes[24]=64; BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(28),143360);
        if(damage==0) bytes[10]=2;
        if(damage==1) bytes[12]=3;
        if(damage==2) bytes[8]=1;
        if(damage==3) bytes[24]=32;
        if(damage==4) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(28),143361);
        var reader=new AppleDiskImageReader();
        if(damage<2) await Assert.ThrowsAsync<NotSupportedException>(()=>reader.ReadAsync(bytes.AsMemory(),".2mg",null));
        else await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync(bytes.AsMemory(),".2mg",null));
    }
}
