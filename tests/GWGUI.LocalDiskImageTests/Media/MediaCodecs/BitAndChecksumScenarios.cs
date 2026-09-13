using GWGUI.MediaEngine.Primitives;
using System.Text;
namespace GWGUI.Tests.Media.MediaCodecs;
internal static class BitAndChecksumScenarios
{
    public static void Crc()
    {
        var data=Encoding.ASCII.GetBytes("123456789");
        Assert.Equal((ushort)0x29b1,Crc16Calculator.Compute(data));
        var encoded=Crc16Calculator.Append(data);
        Assert.Equal(new byte[]{0x29,0xb1},encoded[^2..]);
        Assert.Equal(data,encoded[..^2]);
        Assert.Equal((ushort)0,Crc16Calculator.Compute(encoded));
        data[0]^=1;
        Assert.NotEqual((ushort)0x29b1,Crc16Calculator.Compute(data));
    }
    public static void Reverse(byte input,byte expected)=>Assert.Equal(expected,BitPrimitives.ReverseBits(input));
}
