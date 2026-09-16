namespace GWGUI.App.Constants.Rendering.Emulation;

internal static class OpenGlVideoConstants
{
    internal const uint VertexShader = 0x8B31;
    internal const uint FragmentShader = 0x8B30;
    internal const uint CompileStatus = 0x8B81;
    internal const uint LinkStatus = 0x8B82;
    internal const int InfoLogCapacity = 4_096;
    internal const int SourceTextureUnit = 0;
    internal const int HistoryTextureUnit = 1;
    internal const int InvalidAddressOne = 1;
    internal const int InvalidAddressTwo = 2;
    internal const int InvalidAddressThree = 3;
    internal const int InvalidAddressMinusOne = -1;
    internal const int ShaderSourceCount = 1;
    internal const int ShaderSourceTerminatorByteCount = 1;
    internal const long ShaderSequenceCycle = 4_096;
    internal const float PercentageDivisor = 100f;
    internal const float BrightnessUniformDivisor = 20f;
    internal const float ContrastExponentDivisor = 5f;
    internal const float SaturationUniformDivisor = 10f;
    internal const float SharpnessUniformDivisor = 10f;
    internal const uint PixelFormatDrawToWindow = 4;
    internal const uint PixelFormatSupportOpenGl = 32;
    internal const uint PixelFormatDoubleBuffer = 1;
    internal const byte PixelTypeRgba = 0;
    internal const byte MainPlane = 0;
    internal const uint ColorBufferBit = 0x4000;
    internal const uint Bgra = 0x80E1;
    internal const uint UnsignedByte = 0x1401;
    internal const uint Texture2D = 0x0DE1;
    internal const uint Texture0 = 0x84C0;
    internal const uint Texture1 = 0x84C1;
    internal const uint TextureMinFilter = 0x2801;
    internal const uint TextureMagFilter = 0x2800;
    internal const uint TextureWrapS = 0x2802;
    internal const uint TextureWrapT = 0x2803;
    internal const uint Nearest = 0x2600;
    internal const uint Linear = 0x2601;
    internal const uint Clamp = 0x2900;
    internal const uint Rgba = 0x1908;
    internal const uint Quads = 0x0007;
}
