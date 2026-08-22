using GWGUI.App.Constants.Rendering.Emulation;
using GWGUI.App.Functions.Rendering.Emulation;
using GWGUI.Emulation;
using GWGUI.MediaEngine.Exploration.Scp;

namespace GWGUI.Tests;

public sealed class EmulationVideoPixelTests
{
    public static TheoryData<EmulationPixelFormat, ushort, byte, byte, byte> PackedColors => new()
    {
        { EmulationPixelFormat.Rgb1555, EmulationVideoPixelTestConstants.Rgb1555Red,
            EmulationVideoPixelTestConstants.FullChannel, EmulationVideoPixelTestConstants.EmptyChannel,
            EmulationVideoPixelTestConstants.EmptyChannel },
        { EmulationPixelFormat.Rgb1555, EmulationVideoPixelTestConstants.Rgb1555Green,
            EmulationVideoPixelTestConstants.EmptyChannel, EmulationVideoPixelTestConstants.FullChannel,
            EmulationVideoPixelTestConstants.EmptyChannel },
        { EmulationPixelFormat.Rgb1555, EmulationVideoPixelTestConstants.Blue,
            EmulationVideoPixelTestConstants.EmptyChannel, EmulationVideoPixelTestConstants.EmptyChannel,
            EmulationVideoPixelTestConstants.FullChannel },
        { EmulationPixelFormat.Rgb565, EmulationVideoPixelTestConstants.Rgb565Red,
            EmulationVideoPixelTestConstants.FullChannel, EmulationVideoPixelTestConstants.EmptyChannel,
            EmulationVideoPixelTestConstants.EmptyChannel },
        { EmulationPixelFormat.Rgb565, EmulationVideoPixelTestConstants.Rgb565Green,
            EmulationVideoPixelTestConstants.EmptyChannel, EmulationVideoPixelTestConstants.FullChannel,
            EmulationVideoPixelTestConstants.EmptyChannel },
        { EmulationPixelFormat.Rgb565, EmulationVideoPixelTestConstants.Blue,
            EmulationVideoPixelTestConstants.EmptyChannel, EmulationVideoPixelTestConstants.EmptyChannel,
            EmulationVideoPixelTestConstants.FullChannel }
    };

    [Theory]
    [MemberData(nameof(PackedColors))]
    public void PackedPixelFormats_AreConvertedToExactBgraChannels(EmulationPixelFormat format,
        ushort packed, byte red, byte green, byte blue)
    {
        var source = BitConverter.GetBytes(packed);
        var frame = new VideoFrame(source, EmulationVideoPixelTestConstants.SinglePixelDimension,
            EmulationVideoPixelTestConstants.SinglePixelDimension, source.Length, format,
            EmulationVideoPixelTestConstants.SquareAspectRatio, EmulationVideoPixelTestConstants.FirstSequence,
            TimeSpan.Zero);

        var result = EmulationVideoPixelFunctions.ToBgra32(frame);

        Assert.Equal(blue, result[EmulationVideoPixelConstants.BlueByteOffset]);
        Assert.Equal(green, result[EmulationVideoPixelConstants.GreenByteOffset]);
        Assert.Equal(red, result[EmulationVideoPixelConstants.RedByteOffset]);
        Assert.Equal(EmulationVideoPixelConstants.OpaqueAlpha,
            result[EmulationVideoPixelConstants.AlphaByteOffset]);
    }
}

internal static class EmulationVideoPixelTestConstants
{
    internal const int SinglePixelDimension = 1;
    internal const float SquareAspectRatio = 1f;
    internal const long FirstSequence = 1;
    internal const ushort Rgb1555Red = 0x7C00;
    internal const ushort Rgb1555Green = 0x03E0;
    internal const ushort Rgb565Red = 0xF800;
    internal const ushort Rgb565Green = 0x07E0;
    internal const ushort Blue = 0x001F;
    internal const byte FullChannel = 255;
    internal const byte EmptyChannel = 0;
}
