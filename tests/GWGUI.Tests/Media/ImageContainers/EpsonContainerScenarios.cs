using GWGUI.MediaEngine.Containers.Epson.Raw;
using GWGUI.MediaEngine.Geometries.Epson;
namespace GWGUI.Tests.Media.ImageContainers;
internal static class EpsonContainerScenarios
{
    public static async Task Variant(int kind,int capacity,int blocks,int cylinders,int heads,int lastSize,int lastSector)
    {
        var geometry = kind switch { 0=>EpsonQx10GeometryCatalog.Geometry320,1=>EpsonQx10GeometryCatalog.Geometry400,2=>EpsonQx10GeometryCatalog.GeometryBooter,3=>EpsonQx10GeometryCatalog.Geometry399,4=>EpsonQx10GeometryCatalog.Geometry396,_=>EpsonQx10GeometryCatalog.GeometryLogo };
        var data = new byte[capacity]; data[0]=42; data[^1]=93;
        var reader = new EpsonQx10RawImageReader((_,token)=>{token.ThrowIfCancellationRequested();return Task.FromResult(data);});
        var image = await reader.ReadAsync("virtual",geometry.FormatId);
        Assert.Equal(blocks,image.BlockCount); Assert.Equal(cylinders,image.Cylinders); Assert.Equal(heads,image.Heads); Assert.Equal(capacity,image.Capacity);
        var last = image.AvailableBlocks.Single(x=>x.LogicalBlock==blocks-1);
        Assert.Equal(new GWGUI.MediaEngine.SectorImages.SectorAddress(cylinders-1,heads-1,lastSector),last.Address);
        Assert.Equal(lastSize,last.Data.Count); Assert.Equal(93,last.Data[^1]);
        if(kind==5)
        {
            Assert.DoesNotContain(image.AvailableBlocks,x=>x.Address.Cylinder is 3 or 7);
            Assert.Equal(2,image.AvailableBlocks.First(x=>x.Address.Cylinder==5).Address.Number);
        }
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles(); var writer = new EpsonQx10RawImageWriter(files);
        await writer.WriteAsync(image,"output",geometry.FormatId); Assert.Equal(data,files.Files["output"]);
        var missing = new GWGUI.MediaEngine.SectorImages.SectorImage(image.FormatId,image.BlockSize,image.Cylinders,image.Heads,image.SectorsPerTrack,image.AvailableBlocks.Skip(1).ToArray(),allowVariableBlockSize:true,capacity:capacity,logicalBlockCount:blocks);
        await Assert.ThrowsAsync<InvalidDataException>(()=>writer.WriteAsync(missing,"output",geometry.FormatId)); Assert.Equal(data,files.Files["output"]);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>reader.ReadAsync("virtual",geometry.FormatId,new CancellationToken(true)));
        data=data[..^1]; await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync("virtual",geometry.FormatId));
    }
    public static async Task Read()
    {
        var data=new byte[327680];data[0]=42;data[^1]=93;
        var reader=new EpsonQx10RawImageReader((_,_)=>Task.FromResult(data));
        var image=await reader.ReadAsync("virtual",EpsonQx10GeometryCatalog.Geometry320.FormatId);
        Assert.Equal(40,image.Cylinders);Assert.Equal(2,image.Heads);
        Assert.Equal(16,image.SectorsPerTrack);Assert.Equal(256,image.BlockSize);
        Assert.Equal(1280,image.BlockCount);Assert.Empty(image.MissingBlocks);
        Assert.Equal(42,image.AvailableBlocks.Single(b=>b.LogicalBlock==0).Data[0]);
        Assert.Equal(93,image.AvailableBlocks.Single(b=>b.LogicalBlock==1279).Data[^1]);
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles();
        var writer = new EpsonQx10RawImageWriter(files);
        await writer.WriteAsync(image,"output",image.FormatId); Assert.Equal(data,files.Files["output"]);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>writer.WriteAsync(image,"cancelled",image.FormatId,new CancellationToken(true)));
        Assert.False(files.Files.ContainsKey("cancelled"));
        data=new byte[3];
        await Assert.ThrowsAsync<InvalidDataException>(()=>reader.ReadAsync("virtual",EpsonQx10GeometryCatalog.Geometry320.FormatId));
    }
}
