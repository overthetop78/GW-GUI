using GWGUI.MediaEngine.Containers.TeleDisk;
namespace GWGUI.Tests.Media.ImageContainers;
internal static class TeleDiskContainerScenarios
{
    public static async Task Flags(byte flags)
    {
        bool missing=(flags&0x30)!=0; byte[] payload=Enumerable.Range(0,64).SelectMany(_=>new byte[]{42,93}).ToArray();
        var image=new Td0Image(new(0,0,21,0,3,0,0,1),new(126,8,6,12,34,56,[65,0,66]),
            [new(0,0,[new(0,0,1,0,flags,missing?null:payload)])],new("synthetic",128,1,1,1,[]));
        var files=new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles(); var writer=new Td0Writer(files); await writer.WriteAsync(image,"output");
        var bytes=files.Files["output"]; Assert.Equal(0x80,bytes[7]); Assert.Equal(new byte[]{65,0,66},bytes[22..25]);
        var read=Td0Reader.ReadDetailed(bytes); Assert.Equal(image.Comment!.Data,read.Comment!.Data); Assert.Equal((byte)126,read.Comment.Year);
        var sector=Assert.Single(Assert.Single(read.Tracks).Sectors); Assert.Equal(flags,sector.Flags);
        if(missing) { Assert.Null(sector.Data); Assert.Empty(read.SectorImage.AvailableBlocks); }
        else { Assert.Equal(payload,sector.Data); Assert.Equal((flags&2)==0,Assert.Single(read.SectorImage.AvailableBlocks).IntegrityValid); }
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>writer.WriteAsync(image,"cancelled",new CancellationToken(true))); Assert.False(files.Files.ContainsKey("cancelled"));
        Assert.Throws<OperationCanceledException>(()=>Td0Reader.ReadDetailed(bytes,new CancellationToken(true)));
        bytes[22]^=1; Assert.Throws<InvalidDataException>(()=>Td0Reader.ReadDetailed(bytes));
    }

    public static void Encodings(int encoding)
    {
        byte[] encoded=encoding switch { 0=>[42,93,42,93,42,93],1=>[3,0,42,93],_=>[0,2,42,93,1,2,42,93] };
        var expected=new byte[]{42,93,42,93,42,93};
        Assert.Equal(expected,Td0SectorDecoder.Decode(encoded,(Td0SectorEncoding)encoding,6,0,0,1));
        Assert.Throws<InvalidDataException>(()=>Td0SectorDecoder.Decode(encoded,(Td0SectorEncoding)encoding,7,0,0,1));
        Assert.Throws<InvalidDataException>(()=>Td0SectorDecoder.Decode(encoded[..^1],(Td0SectorEncoding)encoding,6,0,0,1));
    }

    public static void ReadWrite()
    {
        var payload = Enumerable.Range(0, 128).Select(i => (byte)i).ToArray();
        var image = new Td0Image(new(0, 0, 21, 0, 3, 0, 0, 1), null,
            [new(0, 0, [new(0, 0, 1, 0, 0, payload)])], new("synthetic", 128, 1, 1, 1, []));
        var bytes = Td0Writer.Build(image);
        Assert.Equal(new byte[] { 84, 68, 0, 0, 21, 0, 3, 0, 0, 1 }, bytes[..10]);
        Assert.Equal(new byte[] { 1, 0, 0 }, bytes[12..15]);
        Assert.Equal(new byte[] { 0, 0, 1, 0, 0 }, bytes[16..21]);
        Assert.Equal(255, bytes[^1]);
        var restored = Td0Reader.ReadDetailed(bytes);
        Assert.Equal(payload, Assert.Single(Assert.Single(restored.Tracks).Sectors).Data);
        Assert.Equal(payload, Assert.Single(restored.SectorImage.AvailableBlocks).Data);
        bytes[10] ^= 1;
        Assert.Throws<InvalidDataException>(() => Td0Reader.ReadDetailed(bytes));
        using var source = new CancellationTokenSource(); source.Cancel();
        Assert.Throws<OperationCanceledException>(() => Td0Writer.Build(image, source.Token));
        Assert.Throws<InvalidDataException>(() => Td0Reader.ReadDetailed(new byte[] { 84, 68 }));
    }
}
