using GWGUI.Infrastructure.Hardware.Greaseweazle;
namespace GWGUI.Tests.Hardware.GreaseweazleProtocol;
internal static class FluxEncodingScenarios
{
    public static void Encode()
    {
        var result=GreaseweazleFluxStreamEncoder.Encode(new uint[]{0,1,249,250,1524,1525,3600,3601},24000000);
        byte[] expected=[1,249,250,1,254,255,255,2,249,19,1,1,249,255,2,47,53,1,1,249,255,2,35,57,1,1,255,3,61,1,1,1,255,2,207,33,1,1,249,0];
        Assert.Equal(expected,result);
        Assert.Throws<ArgumentOutOfRangeException>(()=>GreaseweazleFluxStreamEncoder.Encode(new uint[]{1},0));
        Assert.Throws<ArgumentOutOfRangeException>(()=>GreaseweazleFluxStreamEncoder.Encode(new uint[]{1u<<28},24000000));
    }
    public static void IndexAndSpace()
    {
        byte[] bytes=[10,255,1,41,1,1,1,255,2,201,1,1,1,30,255,1,121,1,1,1,0];
        var result=GreaseweazleFluxStreamDecoder.Decode(bytes,24000000);
        Assert.Equal(new uint[]{10,130},result.FluxIntervals); Assert.Equal(new uint[]{30,170},result.IndexIntervals);
        Assert.Equal(bytes,result.RawStream);
    }
    public static void Decode()
    {
        var result=GreaseweazleFluxStreamDecoder.Decode(new byte[]{1,249,250,1,254,255,0},24000000);
        Assert.Equal(new uint[]{1,249,250,1524},result.FluxIntervals);
        Assert.Empty(result.IndexIntervals);
        Assert.Equal(24000000u,result.SampleFrequency);
        Assert.Throws<ArgumentOutOfRangeException>(()=>GreaseweazleFluxStreamDecoder.Decode(new byte[]{0},0));
    }
    public static void Invalid(byte[] bytes)=>Assert.Throws<InvalidDataException>(()=>GreaseweazleFluxStreamDecoder.Decode(bytes,24000000));
}
