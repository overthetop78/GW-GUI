using GWGUI.MediaEngine.Containers.Coherent;
namespace GWGUI.Tests.Media.ImageContainers;
internal static class CoherentContainerScenarios
{
    public static async Task Read()
    {
        var bytes = new byte[1536]; bytes[516] = 3;
        "noname"u8.CopyTo(bytes.AsSpan(996)); "nopack"u8.CopyTo(bytes.AsSpan(1002)); bytes[1024] = 42;
        var reader = new CoherentRawImageReader(); var image = await reader.ReadAsync(bytes.AsMemory());
        Assert.Equal(3, image.AvailableBlocks.Count()); Assert.Equal(512, image.BlockSize);
        Assert.Equal(42, image.AvailableBlocks.Single(block => block.LogicalBlock == 2).Data[0]);
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles();
        await new CoherentRawImageWriter(files).WriteAsync(image,"output.bin"); Assert.Equal(bytes,files.Files["output.bin"]);
        using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>new CoherentRawImageWriter(files).WriteAsync(image,"output.bin",cancelled.Token));
        bytes[516] = 4;
        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(bytes.AsMemory()));
        bytes[516] = 3; bytes[996] = 0;
        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(bytes.AsMemory()));
    }
}
