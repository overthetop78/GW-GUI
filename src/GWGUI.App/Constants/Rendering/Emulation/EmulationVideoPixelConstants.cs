namespace GWGUI.App.Constants.Rendering.Emulation;

internal static class EmulationVideoPixelConstants
{
    internal const int BytesPerBgraPixel = 4;
    internal const int BytesPerPackedPixel = 2;
    internal const int SecondPackedByteOffset = 1;
    internal const int BlueByteOffset = 0;
    internal const int GreenByteOffset = 1;
    internal const int RedByteOffset = 2;
    internal const int AlphaByteOffset = 3;
    internal const int HighByteBitShift = 8;
    internal const int GreenBitShift = 5;
    internal const int Rgb1555RedBitShift = 10;
    internal const int Rgb565RedBitShift = 11;
    internal const int FiveBitMask = 0x1f;
    internal const int SixBitMask = 0x3f;
    internal const int FiveBitMaximum = 31;
    internal const int SixBitMaximum = 63;
    internal const byte OpaqueAlpha = 255;
}
