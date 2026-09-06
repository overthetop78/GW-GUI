using GWGUI.MediaEngine.Containers.Ibm.Raw;
namespace GWGUI.Tests.Media.ImageContainers;
internal static class IbmContainerScenarios
{
    public static async Task Bpb(bool largeCount, bool valid)
    {
        var data = new byte[368640];
        void Word(int offset, ushort value) => System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(offset), value);
        Word(11, 512); Word(24, 9); Word(26, 1);
        if (largeCount) System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(32), valid ? 720u : 718u);
        else Word(19, valid ? (ushort)720 : (ushort)718);
        var image = await new IbmRawImageReader((_, _) => Task.FromResult(data)).ReadAsync("virtual");
        Assert.Equal(valid ? 80 : 40, image.Cylinders); Assert.Equal(valid ? 1 : 2, image.Heads);
        Assert.Equal(9, image.SectorsPerTrack); Assert.Equal(720, image.BlockCount);
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles();
        var writer = new IbmRawImageWriter(new GWGUI.MediaEngine.Containers.Raw.LinearSectorImageWriter(files));
        if (valid) { await Assert.ThrowsAsync<InvalidDataException>(() => writer.WriteAsync(image, "output", "ibm.360")); Assert.Empty(files.Files); }
        else { await writer.WriteAsync(image, "output", "ibm.360"); Assert.Equal(data, files.Files["output"]); }
    }
    public static async Task Dmf()
    {
        var data = new byte[1720320]; data[0] = 42; data[^1] = 93;
        var image = await new IbmRawImageReader((_, _) => Task.FromResult(data)).ReadAsync("virtual");
        Assert.Equal("ibm.1680", image.FormatId);
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles();
        var writer = new IbmRawImageWriter(new GWGUI.MediaEngine.Containers.Raw.LinearSectorImageWriter(files));
        await writer.WriteAsync(image, "output", "ibm.dmf"); Assert.Equal(data, files.Files["output"]);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => writer.WriteAsync(image, "output", "ibm.dmf", new CancellationToken(true)));
        Assert.Equal(data, files.Files["output"]);
        await Assert.ThrowsAsync<InvalidDataException>(() => writer.WriteAsync(image, "output", "unsupported"));
    }
    public static async Task Read(string extension,int length,int cylinders,int heads,int sectors,int blockSize)
    {
        var data=new byte[length];data[0]=42;data[^1]=93;
        var calls=0;
        var reader=new IbmRawImageReader((path,token)=>{
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
        await new IbmRawImageWriter(linear).WriteAsync(image, outputPath, image.FormatId);
        Assert.Equal(data, files.Files[outputPath]); Assert.Equal(outputPath, Assert.Single(files.Calls));
        using var cancelled=new CancellationTokenSource();cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>reader.ReadAsync("virtual"+extension,cancelled.Token));
        data=new byte[3];
        await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync("virtual"+extension));
    }
}
