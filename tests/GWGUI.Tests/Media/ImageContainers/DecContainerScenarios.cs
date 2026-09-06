using GWGUI.MediaEngine.Containers.Dec.Rx02;
namespace GWGUI.Tests.Media.ImageContainers;
internal static class DecContainerScenarios
{
    public static async Task Read()
    {
        var bytes = new byte[77 * 26 * 256];
        Array.Fill(bytes, (byte)42, 26 * 256, 256);
        Array.Fill(bytes, (byte)93, 28 * 256, 256);
        var reader = new DecRx02Reader(); var image = await reader.ReadAsync(bytes.AsMemory());
        Assert.Equal(77, image.Cylinders); Assert.Equal(1, image.Heads); Assert.Equal(512, image.BlockSize);
        var block = image.AvailableBlocks.Single(block => block.LogicalBlock == 0);
        Assert.All(block.Data.Take(256), value => Assert.Equal(42, value));
        Assert.All(block.Data.Skip(256), value => Assert.Equal(93, value));
        Assert.Equal((1, 1), DecRx02SectorOrder.LogicalToPhysical(0));
        Assert.Equal((1, 3), DecRx02SectorOrder.LogicalToPhysical(1));
        Assert.Equal((1, 2), DecRx02SectorOrder.LogicalToPhysical(13));
        Assert.Equal((2, 7), DecRx02SectorOrder.LogicalToPhysical(26));
        Assert.Equal((0, 15), DecRx02SectorOrder.LogicalToPhysical(1976));
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles();
        await new DecRx02Writer(files).WriteAsync(image,"output.img"); Assert.Equal(bytes,files.Files["output.img"]);
        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(bytes.AsMemory(1)));
        using var source = new CancellationTokenSource(); source.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => reader.ReadAsync(bytes.AsMemory(), source.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new DecRx02Writer(files).WriteAsync(image,"output.img",source.Token));
    }
}
