using GWGUI.MediaEngine.Formats.Floppy.Adf;
namespace GWGUI.Tests.Media.ImageContainers;
internal static class AdfContainerScenarios
{
    public static async Task ReadAcornHighDensity()
    {
        var data = new byte[AcornAdfGeometry.HighDensityCapacity];
        data[0] = 42;
        data[^1] = 93;
        var reader = new AdfReader((_, token) => { token.ThrowIfCancellationRequested(); return Task.FromResult(data); });
        var image = await reader.ReadAsync("archimedes.adf");
        Assert.Equal(GWGUI.MediaEngine.Constants.DiskImageFormatIds.AcornAdfs1600, image.FormatId);
        Assert.Equal(80, image.Cylinders);
        Assert.Equal(2, image.Heads);
        Assert.Equal(10, image.SectorsPerTrack);
        Assert.Equal(1024, image.BlockSize);
        Assert.Equal(1600, image.AvailableBlocks.Count);
        Assert.Empty(image.MissingBlocks);
        var blocks = image.AvailableBlocks.OrderBy(block => block.LogicalBlock).ToArray();
        Assert.Equal(42, blocks[0].Data[0]);
        Assert.Equal(93, blocks[^1].Data[^1]);
    }

    public static async Task Read(string extension,int length,int cylinders,int heads,int sectors,int blockSize)
    {
        var data=new byte[length];data[0]=42;data[^1]=93;
        var calls=0;
        var reader=new AdfReader((path,token)=>{
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
        var linear = new GWGUI.MediaEngine.Formats.Floppy.Raw.LinearSectorImageWriter(files);
        var outputPath = "output" + extension;
        if (image.FormatId.StartsWith("amiga", StringComparison.OrdinalIgnoreCase)) await new AmigaAdfWriter(linear).WriteAsync(image, outputPath);
        else await new AcornAdfWriter(linear).WriteAsync(image, outputPath);
        Assert.Equal(data, files.Files[outputPath]); Assert.Equal(outputPath, Assert.Single(files.Calls));
        using var cancelled=new CancellationTokenSource();cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>reader.ReadAsync("virtual"+extension,cancelled.Token));
        data=new byte[3];
        await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync("virtual"+extension));
    }
}
