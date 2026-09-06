using GWGUI.MediaEngine.Containers.Ucsd.Raw;
namespace GWGUI.Tests.Media.ImageContainers;
internal static class UcsdContainerScenarios
{
    public static async Task Read(string extension,int length,int cylinders,int heads,int sectors,int blockSize)
    {
        var data=new byte[length];data[0]=42;data[^1]=93;
        var calls=0;
        var reader=new UcsdRawImageReader((path,token)=>{
            Assert.Equal("virtual"+extension,path);token.ThrowIfCancellationRequested();calls++;return Task.FromResult(data);
        });
        var image=await reader.ReadAsync("virtual"+extension);
        Assert.Equal(1,calls);
        Assert.Equal(cylinders,image.Cylinders);Assert.Equal(heads,image.Heads);
        Assert.Equal(sectors,image.SectorsPerTrack);Assert.Equal(blockSize,image.BlockSize);
        Assert.Equal(length,image.Capacity);Assert.Empty(image.MissingBlocks);
        var ordered=image.AvailableBlocks.OrderBy(b=>b.LogicalBlock).ToArray();
        Assert.Equal(42,ordered[0].Data[0]);Assert.Equal(93,ordered[^1].Data[^1]);
        using var cancelled=new CancellationTokenSource();cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>reader.ReadAsync("virtual"+extension,cancelled.Token));
        data=new byte[3];
        await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync("virtual"+extension));
    }
}
