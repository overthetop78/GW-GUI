using GWGUI.MediaEngine.Containers.Atari.St;
namespace GWGUI.Tests.Media.ImageContainers;
internal static class AtariContainerScenarios
{
    public static void Rle(int damage)
    {
        var literal = new byte[] { 1, 0xe5, 2, 42, 42, 42, 42, 42 };
        var packed = new byte[] { 1, 0xe5, 0xe5, 0, 1, 2, 0xe5, 42, 0, 5 };
        Assert.Equal(packed, GWGUI.MediaEngine.Containers.Atari.Msa.MsaRleEncoder.Pack(literal));
        Assert.Equal(literal, GWGUI.MediaEngine.Containers.Atari.Msa.MsaRleDecoder.Unpack(packed, 8, 0, 0));
        var invalid = damage switch {
            0 => new byte[] { 0xe5, 42 },
            1 => new byte[] { 0xe5, 42, 0, 0 },
            2 => new byte[] { 0xe5, 42, 0, 6 },
            3 => new byte[] { 1, 2 },
            _ => new byte[] { 0xe5, 42, 0, 5, 7 }
        };
        Assert.Throws<InvalidDataException>(() => GWGUI.MediaEngine.Containers.Atari.Msa.MsaRleDecoder.Unpack(invalid, 5, 0, 0));
    }
    public static async Task Atr(int size,int count,string format)
    {
        var length = size == 256 ? count * size - 384 : count * size;
        var data = new byte[length+16]; data[0]=0x96; data[1]=2;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(2),(ushort)(length/16));
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(4),(ushort)size);
        data[16]=42; data[16+384]=71; data[^1]=93;
        var reader = new GWGUI.MediaEngine.Containers.Atari.Atr.AtrReader((_,token)=>{token.ThrowIfCancellationRequested();return Task.FromResult(data);});
        var image = await reader.ReadAsync("virtual"); Assert.Equal(format,image.FormatId); Assert.Equal(count,image.BlockCount); Assert.Equal(length,image.Capacity);
        Assert.Equal(128,image.AvailableBlocks.Single(x=>x.LogicalBlock==0).Data.Count);
        Assert.Equal(size,image.AvailableBlocks.Single(x=>x.LogicalBlock==3).Data.Count);
        Assert.Equal(71,image.AvailableBlocks.Single(x=>x.LogicalBlock==3).Data[0]);
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles();
        var writer = new GWGUI.MediaEngine.Containers.Atari.Atr.AtrWriter(files);
        await writer.WriteAsync(image,"output",format); Assert.Equal(data,files.Files["output"]);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>writer.WriteAsync(image,"cancelled",format,new CancellationToken(true)));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>reader.ReadAsync("virtual",new CancellationToken(true)));
        data=data[..^1]; await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync("virtual"));
        data[0]=0; await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync("virtual"));
    }
    public static async Task Msa(bool compressed)
    {
        var data = new byte[368640]; for(var index=0;index<data.Length;index++) data[index] = compressed ? (byte)42 : (byte)(index%251);
        var image = await new AtariStReader((_,_)=>Task.FromResult(data)).ReadAsync("virtual.st");
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles();
        var writer = new GWGUI.MediaEngine.Containers.Atari.Msa.MsaWriter(files); await writer.WriteAsync(image,"output");
        var written = files.Files["output"];
        Assert.Equal(new byte[] {0x0e,0x0f,0,9,0,1,0,0,0,39},written.Take(10));
        Assert.Equal(compressed?490:368810,written.Length);
        if(compressed) Assert.Equal(new byte[] {0,4,0xe5,42,0x12,0},written.Skip(10).Take(6));
        else Assert.Equal(new byte[] {0x12,0,0,1,2,3},written.Skip(10).Take(6));
        var reader = new GWGUI.MediaEngine.Containers.Atari.Msa.MsaReader((_,token)=>{token.ThrowIfCancellationRequested();return Task.FromResult(written);});
        var decoded = await reader.ReadAsync("virtual"); Assert.Equal(40,decoded.Cylinders); Assert.Equal(2,decoded.Heads); Assert.Equal(9,decoded.SectorsPerTrack);
        Assert.Equal(data,decoded.AvailableBlocks.OrderBy(x=>x.LogicalBlock).SelectMany(x=>x.Data));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>writer.WriteAsync(image,"cancelled",new CancellationToken(true)));
        written=written[..^1]; await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync("virtual"));
        written[0]=0; await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync("virtual"));
    }
    public static async Task Read(string extension,int length,int cylinders,int heads,int sectors,int blockSize,bool bpb=false)
    {
        var data=new byte[length];data[0]=42;data[^1]=93;
        if (bpb) {
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(11), 512);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(19), (ushort)(length / 512));
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(24), (ushort)sectors);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(26), (ushort)heads);
        }
        var calls=0;
        var reader=new AtariStReader((path,token)=>{
            Assert.Equal("virtual"+extension,path);token.ThrowIfCancellationRequested();calls++;return Task.FromResult(data);
        });
        var image=await reader.ReadAsync("virtual"+extension);
        Assert.Equal(1,calls);
        Assert.Equal(cylinders,image.Cylinders);Assert.Equal(heads,image.Heads);
        Assert.Equal(sectors,image.SectorsPerTrack);Assert.Equal(blockSize,image.BlockSize);
        Assert.Equal(length,image.Capacity);Assert.Empty(image.MissingBlocks);
        var ordered=image.AvailableBlocks.OrderBy(b=>b.LogicalBlock).ToArray();
        Assert.Equal(42,ordered[0].Data[0]);Assert.Equal(93,ordered[^1].Data[^1]);
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles();
        var linear = new GWGUI.MediaEngine.Containers.Raw.LinearSectorImageWriter(files);
        var outputPath = "output" + extension;
        await new GWGUI.MediaEngine.Containers.Atari.St.AtariStWriter(linear).WriteAsync(image, outputPath);
        Assert.Equal(data, files.Files[outputPath]); Assert.Equal(outputPath, Assert.Single(files.Calls));
        using var cancelled=new CancellationTokenSource();cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>reader.ReadAsync("virtual"+extension,cancelled.Token));
        data=new byte[3];
        await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync("virtual"+extension));
    }
}
