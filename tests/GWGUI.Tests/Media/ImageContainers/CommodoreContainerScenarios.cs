using GWGUI.MediaEngine.Containers.Commodore.D64;
using GWGUI.MediaEngine.Containers.Commodore.D71;
using GWGUI.MediaEngine.Containers.Commodore.D81;
using GWGUI.MediaEngine.SectorImages;
namespace GWGUI.Tests.Media.ImageContainers;
internal static class CommodoreContainerScenarios
{
    public static async Task Diagnostics(int heads, int tracks, bool map)
    {
        var perSide = tracks == 35 ? 683 : 768;
        var count = perSide * heads; var length = count * 256;
        var data = new byte[length + (map ? count : 0)];
        data[0] = 42; data[length - 1] = 93;
        if (map) {
            Array.Fill(data, (byte)1, length, count);
            data[length + 1] = 2; data[length + count - 1] = 9;
        }
        Task<byte[]> Load(string _, CancellationToken token) { token.ThrowIfCancellationRequested(); return Task.FromResult(data); }
        Func<string, CancellationToken, Task<SectorImage>> read = heads == 1 ? new D64Reader(Load).ReadAsync : new D71Reader(Load).ReadAsync;
        var image = await read("virtual", default);
        Assert.Equal(tracks, image.Cylinders); Assert.Equal(heads, image.Heads); Assert.Equal(count, image.BlockCount); Assert.Equal(length, image.Capacity);
        var blocks = image.AvailableBlocks.OrderBy(block => block.LogicalBlock).ToArray();
        Assert.Equal(new SectorAddress(0, 0, 0), blocks[0].Address);
        Assert.Equal(new SectorAddress(tracks - 1, heads - 1, 16), blocks[^1].Address);
        if (heads == 2) Assert.Equal(new SectorAddress(0, 1, 0), blocks[perSide].Address);
        Assert.True(blocks[0].IntegrityValid);
        Assert.Equal(!map, blocks[1].IntegrityValid); Assert.Equal(!map, blocks[^1].IntegrityValid);
        Assert.Equal(map ? (byte?)2 : null, blocks[1].DiagnosticCode);
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles();
        var writer = new GWGUI.MediaEngine.Containers.Commodore.CommodoreDosContainerWriter(files);
        await writer.WriteAsync(image, "output", map ? GWGUI.MediaEngine.Containers.Commodore.CommodoreDosErrorMapMode.Preserve : GWGUI.MediaEngine.Containers.Commodore.CommodoreDosErrorMapMode.None);
        Assert.Equal(data, files.Files["output"]);
        await writer.WriteAsync(image, "without-map"); Assert.Equal(data.Take(length), files.Files["without-map"]);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => writer.WriteAsync(image, "output", cancellationToken: new CancellationToken(true)));
        Assert.Equal(data, files.Files["output"]);
        var incomplete = new SectorImage(image.FormatId, 256, tracks, heads, 21, blocks.Skip(1), capacity: length, logicalBlockCount: count);
        await Assert.ThrowsAsync<InvalidDataException>(() => writer.WriteAsync(incomplete, "output"));
        Assert.Equal(data, files.Files["output"]);
        data = data[..^1]; await Assert.ThrowsAsync<InvalidDataException>(() => read("virtual", default));
    }
    public static async Task Read(int kind,int length,int count,int heads)
    {
        var data=new byte[length];data[0]=42;data[^1]=93;
        Task<byte[]> Source(string path,CancellationToken token){Assert.Equal("virtual",path);token.ThrowIfCancellationRequested();return Task.FromResult(data);}
        Func<string,CancellationToken,Task<SectorImage>> read=kind switch
        {
            64=>new D64Reader(Source).ReadAsync,
            71=>new D71Reader(Source).ReadAsync,
            _=>new D81Reader(Source).ReadAsync
        };
        var image=await read("virtual",default);
        Assert.Equal(count,image.BlockCount);Assert.Equal(heads,image.Heads);
        Assert.Equal(256,image.BlockSize);Assert.Equal(length,image.Capacity);Assert.Empty(image.MissingBlocks);
        Assert.Equal(42,image.AvailableBlocks.Single(b=>b.LogicalBlock==0).Data[0]);
        Assert.Equal(93,image.AvailableBlocks.Single(b=>b.LogicalBlock==count-1).Data[^1]);
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles();
        if(kind==81) await new D81Writer(new GWGUI.MediaEngine.Containers.Raw.LinearSectorImageWriter(files)).WriteAsync(image,"output");
        else await new GWGUI.MediaEngine.Containers.Commodore.CommodoreDosContainerWriter(files).WriteAsync(image,"output");
        Assert.Equal(data,files.Files["output"]);
        data=new byte[3];
        await Assert.ThrowsAsync<InvalidDataException>(()=>read("virtual",default));
    }
}
