using GWGUI.MediaEngine.Containers.Cp2;
namespace GWGUI.Tests.Media.ImageContainers;
internal static class Cp2ContainerScenarios
{
    public static async Task Ordering(int damage)
    {
        var bytes=new byte[1446]; "SOFTWARE PIRATES"u8.CopyTo(bytes); bytes[30]=0x84; bytes[31]=1; bytes[34]=2;
        bytes[41]=1; bytes[42]=2; bytes[44]=20;
        bytes[57]=2; bytes[58]=2; bytes[60]=10;
        Array.Fill(bytes,(byte)42,422,512); Array.Fill(bytes,(byte)93,934,512);
        if(damage==1) bytes[0]=0;
        if(damage==2) bytes[30]=0;
        if(damage==3) bytes[34]=24;
        if(damage==4) { bytes[33]=2; bytes[40]=2; bytes[56]=2; }
        if(damage==5) { bytes[41]=0; bytes[57]=0; }
        var reader=new Cp2Reader();
        if(damage!=0) { await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync(bytes.AsMemory())); return; }
        var image=await reader.ReadAsync(bytes.AsMemory()); Assert.Equal(2,image.BlockCount);
        Assert.Equal(Enumerable.Repeat((byte)93,512),image.AvailableBlocks.Single(b=>b.LogicalBlock==0).Data);
        Assert.Equal(Enumerable.Repeat((byte)42,512),image.AvailableBlocks.Single(b=>b.LogicalBlock==1).Data);
    }

    public static async Task Read()
    {
        var bytes = new byte[934]; "SOFTWARE PIRATES"u8.CopyTo(bytes);
        bytes[30] = 0x84; bytes[31] = 1; bytes[34] = 1;
        bytes[41] = 1; bytes[42] = 2;
        bytes[422] = 42; bytes[^1] = 93;
        var reader = new Cp2Reader(); var result = await reader.ReadAsync(bytes.AsMemory());
        var block = Assert.Single(result.AvailableBlocks);
        Assert.Equal(512, block.Data.Count); Assert.Equal(42, block.Data[0]); Assert.Equal(93, block.Data[^1]);
        Assert.Equal(1, block.Address.Number); Assert.Equal(0, block.Address.Cylinder);
        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(bytes.AsMemory(0, bytes.Length - 1)));
        using var source = new CancellationTokenSource(); source.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => reader.ReadAsync(bytes.AsMemory(), source.Token));
    }
}
