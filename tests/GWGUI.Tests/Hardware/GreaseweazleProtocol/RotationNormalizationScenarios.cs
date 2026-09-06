using GWGUI.Infrastructure.Hardware.Greaseweazle;
namespace GWGUI.Tests.Hardware.GreaseweazleProtocol;
internal static class RotationNormalizationScenarios
{
    public static void Hard(bool inconsistent)
    {
        uint[] intervals=inconsistent?[50,100,100,100,40,40,100,100,40,40]:[50,100,100,100,40,40,100,100,100,40,40];
        var capture=new GreaseweazleFluxCapture([],intervals,24000000,[]);
        if(inconsistent) { Assert.Throws<InvalidDataException>(()=>GreaseweazleFluxIndexNormalizer.FromHardSectorIndexes(capture,2)); return; }
        var result=GreaseweazleFluxIndexNormalizer.FromHardSectorIndexes(capture,2);
        Assert.Equal(50u,result.InitialIndexTicks); Assert.Equal(new uint[]{380,380},result.RevolutionTicks); Assert.Equal(new[]{4,4},result.HardSectorCounts);
        Assert.Throws<InvalidDataException>(()=>GreaseweazleFluxIndexNormalizer.FromHardSectorIndexes(capture,3));
        Assert.Equal(12000u,GreaseweazleFluxIndexNormalizer.FromFakeIndex(capture,1,4800000).InitialIndexTicks);
    }
    private static GreaseweazleFluxCapture Capture()=>new(new uint[]{1,2},new uint[]{50,100,110,120},1000,[]);
    public static void Physical()
    {
        var result=GreaseweazleFluxIndexNormalizer.FromPhysicalIndexes(Capture(),2);
        Assert.Equal(50u,result.InitialIndexTicks);
        Assert.Equal(new uint[]{100,110},result.RevolutionTicks);
        Assert.Empty(result.HardSectorCounts);
    }
    public static void Fake()
    {
        var result=GreaseweazleFluxIndexNormalizer.FromFakeIndex(Capture(),3,200);
        Assert.Equal(new uint[]{200,200,200},result.RevolutionTicks);
        Assert.True(result.InitialIndexTicks>0);
    }
    public static void Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(()=>GreaseweazleFluxIndexNormalizer.FromPhysicalIndexes(Capture(),0));
        Assert.Throws<InvalidDataException>(()=>GreaseweazleFluxIndexNormalizer.FromPhysicalIndexes(Capture(),4));
        Assert.Throws<ArgumentOutOfRangeException>(()=>GreaseweazleFluxIndexNormalizer.FromFakeIndex(Capture(),1,0));
        Assert.Throws<InvalidDataException>(()=>GreaseweazleFluxIndexNormalizer.FromHardSectorIndexes(Capture(),1));
    }
}
