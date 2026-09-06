using GWGUI.MediaEngine.Conversion.Apple;
using GWGUI.MediaEngine.Conversion.Ibm;
using GWGUI.MediaEngine.Conversion.Scp;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Containers.Raw;
using GWGUI.MediaEngine.Containers.Ibm.Raw;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Media.Conversion;
internal static class ConversionContentScenarios
{
    public static void AppleOrder()
    {
        var input=new byte[8192]; for(var sector=0;sector<32;sector++) input.AsSpan(sector*256,256).Fill((byte)sector);
        var result=AppleIISectorOrderConverter.DosToProDos(input);
        byte[] expected=[0,14,13,12,11,10,9,8,7,6,5,4,3,2,1,15];
        for(var sector=0;sector<32;sector++) Assert.All(result.AsSpan(sector*256,256).ToArray(),value=>Assert.Equal((byte)(expected[sector%16]+sector/16*16),value));
        Assert.Equal(input,AppleIISectorOrderConverter.ProDosToDos(result)); Assert.Equal(1,input[256]);
        Assert.Throws<InvalidDataException>(()=>AppleIISectorOrderConverter.DosToProDos(new byte[4095]));
        Assert.Throws<ArgumentOutOfRangeException>(()=>AppleIISectorOrderConverter.ProDosToPhysicalSector(16));
    }
    public static async Task RawConversion()
    {
        var input=new byte[163840]; input[0]=42; input[513]=93; input[^1]=71;
        var files=new MemoryImageFiles(); var reads=0;
        var service=new IbmRawConversionService(null!,new IbmRawImageReader((path,token)=>{Assert.Equal("source.ima",path); token.ThrowIfCancellationRequested(); reads++; return Task.FromResult(input);}),new IbmRawImageWriter(new LinearSectorImageWriter(files)));
        await service.ConvertAsync("source.ima","output.img","ibm.160"); Assert.Equal(input,files.Files["output.img"]); Assert.Equal(1,reads);
        await Assert.ThrowsAsync<InvalidDataException>(()=>service.ConvertAsync("source.ima","invalid.img","ibm.720")); Assert.False(files.Files.ContainsKey("invalid.img"));
        using var cancellation=new CancellationTokenSource(); cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>service.ConvertAsync("source.ima","cancelled.img","ibm.160",cancellation.Token)); Assert.False(files.Files.ContainsKey("cancelled.img"));
    }
    public static void SectorToFlux()
    {
        var payload=Enumerable.Range(0,512).Select(i=>(byte)(i%251)).ToArray();
        var image=new SectorImage("ibm.160",512,40,1,8,[new(0,new(0,0,1),payload)]);
        var converter=new SectorImageScpConversionService(new SectorImageTrackEncoder(),new ScpEncodedTrackFluxService(),new ScpWriter());
        Assert.True(converter.CanCreate(image)); var result=converter.Create(image); var track=Assert.Single(result.Tracks); Assert.Equal(0,track.Cylinder); Assert.Equal(0,track.Head);
        var revolution=Assert.Single(track.Revolutions); Assert.Equal(8000000u,revolution.IndexTimeTicks);
        var sector=Assert.Single(new IsoMfmDecoder().Decode(revolution.Flux).Sectors); Assert.Equal(1,sector.Number); Assert.True(sector.IntegrityValid); Assert.Equal(payload,sector.Data);
        using var cancellation=new CancellationTokenSource(); cancellation.Cancel(); Assert.ThrowsAny<OperationCanceledException>(()=>converter.Create(image,cancellation.Token));
    }
}
