using GWGUI.MediaEngine.Containers.ImageDisk;
namespace GWGUI.Tests.Media.ImageContainers;
internal static class ImageDiskContainerScenarios
{
    public static async Task Mode(int mode)
    {
        var source = new byte[] {73,77,68,26,(byte)mode,0,0,1,0,7,2,42};
        var image = ImdReader.ReadDetailed(source); Assert.Equal(mode,(int)image.Tracks[0].Mode);
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles(); await new ImdWriter(files).WriteAsync(image,"output");
        Assert.Equal(source,files.Files["output"]);
    }
    public static async Task Write(int type)
    {
        var image = ImdReader.ReadDetailed(new byte[] {73,77,68,26,0,0,0,1,0,7,2,42});
        var sector = image.Tracks[0].Sectors[0] with { RecordType = (ImdSectorRecordType)type };
        image = image with { Tracks = [image.Tracks[0] with { Sectors = [sector] }] };
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles(); var writer = new ImdWriter(files);
        await writer.WriteAsync(image,"output"); var bytes = files.Files["output"];
        Assert.Equal(new byte[] {73,77,68,26,0,0,0,1,0,7,(byte)type},bytes.Take(11));
        Assert.Equal(type == 0 ? 11 : type%2 == 0 ? 12 : 139,bytes.Length);
        if(type != 0) Assert.All(bytes.Skip(11),value=>Assert.Equal(42,value));
        var result = Assert.Single(Assert.Single(ImdReader.ReadDetailed(bytes).Tracks).Sectors);
        Assert.Equal((ImdSectorRecordType)type,result.RecordType); Assert.Equal(128,result.Size);
        Assert.All(result.Data,value=>Assert.Equal(type==0?0:42,value));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>writer.WriteAsync(image,"cancelled",new CancellationToken(true)));
        var invalid = image with { Tracks = [image.Tracks[0] with { Sectors = [sector with { Data = new byte[3] }] }] };
        await Assert.ThrowsAsync<InvalidDataException>(()=>writer.WriteAsync(invalid,"invalid"));
        Assert.Single(files.Files);
    }
    public static async Task Maps()
    {
        var image = ImdReader.ReadDetailed(new byte[] {73,77,68,26,0,0,0,1,0,7,2,42});
        var sector = image.Tracks[0].Sectors[0] with { Cylinder = 2,Head = 1,Size = 129,Data = Enumerable.Repeat((byte)42,129).ToArray() };
        image = image with { Tracks = [image.Tracks[0] with { Sectors = [sector] }] };
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles(); await new ImdWriter(files).WriteAsync(image,"output");
        Assert.Equal(new byte[] {73,77,68,26,0,0,192,1,255,7,2,1,129,0,2,42},files.Files["output"]);
        var decoded = Assert.Single(Assert.Single(ImdReader.ReadDetailed(files.Files["output"]).Tracks).Sectors);
        Assert.Equal(2,decoded.Cylinder); Assert.Equal(1,decoded.Head); Assert.Equal(129,decoded.Size);
        Assert.Equal(sector.Data,decoded.Data);
    }
    public static void Compressed()
    {
        byte[] bytes=[73,77,68,26,0,0,0,1,0,7,2,42];
        var image=ImdReader.ReadDetailed(bytes);
        var sector=Assert.Single(Assert.Single(image.Tracks).Sectors);
        Assert.Equal(7,sector.Number);
        Assert.Equal(128,sector.Size);
        Assert.Equal(128,sector.Data.Count);
        Assert.All(sector.Data,value=>Assert.Equal(42,value));
        Assert.Equal(0,sector.Head);
        Assert.Equal(0,sector.Cylinder);
    }
    public static void Invalid(byte[] bytes)=>Assert.Throws<InvalidDataException>(()=>ImdReader.ReadDetailed(bytes));
}
