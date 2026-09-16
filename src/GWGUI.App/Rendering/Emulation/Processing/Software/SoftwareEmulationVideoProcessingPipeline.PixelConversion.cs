using GWGUI.App.Constants.Rendering.Emulation;
using GWGUI.App.Functions.Rendering.Emulation;
using GWGUI.App.Interfaces.Rendering.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal sealed partial class SoftwareEmulationVideoProcessingPipeline : IEmulationVideoProcessingPipeline
{
    private static void Validate(VideoFrame frame, EmulationVideoProcessingSize sourceSize,
        EmulationVideoProcessingSize outputSize)
    {
        if (sourceSize.Width != frame.Width || sourceSize.Height != frame.Height)
            throw new ArgumentException(nameof(sourceSize));
        if (outputSize.Width <= 0 || outputSize.Height <= 0)
            throw new ArgumentOutOfRangeException(nameof(outputSize));
    }

    private static float[] ToLinear(byte[] pixels, int width, int height)
    {
        var result = new float[checked(width * height * 3)];
        for (var index = 0; index < width * height; index++)
        {
            var pixel = index * EmulationVideoPixelConstants.BytesPerBgraPixel;
            var color = index * 3;
            result[color] = SrgbToLinear(
                pixels[pixel + EmulationVideoPixelConstants.RedByteOffset]
                / SoftwareVideoProcessingConstants.MaximumColorComponent);
            result[color + 1] = SrgbToLinear(
                pixels[pixel + EmulationVideoPixelConstants.GreenByteOffset]
                / SoftwareVideoProcessingConstants.MaximumColorComponent);
            result[color + 2] = SrgbToLinear(
                pixels[pixel + EmulationVideoPixelConstants.BlueByteOffset]
                / SoftwareVideoProcessingConstants.MaximumColorComponent);
        }
        return result;
    }

    private static byte[] ToSrgb(float[] colors, int width, int height)
    {
        var output = new byte[checked(width * height
            * EmulationVideoPixelConstants.BytesPerBgraPixel)];
        for (var index = 0; index < width * height; index++)
        {
            var color = index * 3;
            var pixel = index * EmulationVideoPixelConstants.BytesPerBgraPixel;
            output[pixel + EmulationVideoPixelConstants.RedByteOffset] = ToByte(colors[color]);
            output[pixel + EmulationVideoPixelConstants.GreenByteOffset] = ToByte(colors[color + 1]);
            output[pixel + EmulationVideoPixelConstants.BlueByteOffset] = ToByte(colors[color + 2]);
            output[pixel + EmulationVideoPixelConstants.AlphaByteOffset] =
                EmulationVideoPixelConstants.OpaqueAlpha;
        }
        return output;
    }

    private static byte ToByte(float linear) => (byte)Math.Clamp(
        (int)MathF.Round(LinearToSrgb(linear)
            * SoftwareVideoProcessingConstants.MaximumColorComponent),
        SoftwareVideoProcessingConstants.MinimumColorByte,
        SoftwareVideoProcessingConstants.MaximumColorByte);
}
