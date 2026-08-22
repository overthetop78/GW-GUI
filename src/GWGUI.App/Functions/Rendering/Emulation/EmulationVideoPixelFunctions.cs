using GWGUI.App.Constants.Rendering.Emulation;
using GWGUI.Emulation;

namespace GWGUI.App.Functions.Rendering.Emulation;

internal static class EmulationVideoPixelFunctions
{
    internal static byte[] ToBgra32(VideoFrame frame)
    {
        var pitch = checked(frame.Width * EmulationVideoPixelConstants.BytesPerBgraPixel);
        var source = frame.Pixels.Span;
        var destination = GC.AllocateUninitializedArray<byte>(checked(pitch * frame.Height));
        for (var y = 0; y < frame.Height; y++)
        {
            var sourceRow = source.Slice(checked(y * frame.Pitch), frame.Pitch);
            var destinationRow = destination.AsSpan(checked(y * pitch), pitch);
            if (frame.PixelFormat == EmulationPixelFormat.Xrgb8888)
            {
                sourceRow[..Math.Min(sourceRow.Length, pitch)].CopyTo(destinationRow);
                for (var x = 0; x < frame.Width; x++)
                    destinationRow[x * EmulationVideoPixelConstants.BytesPerBgraPixel +
                        EmulationVideoPixelConstants.AlphaByteOffset] = EmulationVideoPixelConstants.OpaqueAlpha;
                continue;
            }
            for (var x = 0; x < frame.Width; x++)
            {
                var sourceOffset = x * EmulationVideoPixelConstants.BytesPerPackedPixel;
                var value = sourceRow[sourceOffset] |
                    sourceRow[sourceOffset + EmulationVideoPixelConstants.SecondPackedByteOffset] <<
                    EmulationVideoPixelConstants.HighByteBitShift;
                var offset = x * EmulationVideoPixelConstants.BytesPerBgraPixel;
                destinationRow[offset + EmulationVideoPixelConstants.BlueByteOffset] = (byte)(
                    (value & EmulationVideoPixelConstants.FiveBitMask) * EmulationVideoPixelConstants.OpaqueAlpha /
                    EmulationVideoPixelConstants.FiveBitMaximum);
                if (frame.PixelFormat == EmulationPixelFormat.Rgb1555)
                {
                    destinationRow[offset + EmulationVideoPixelConstants.GreenByteOffset] = (byte)(
                        ((value >> EmulationVideoPixelConstants.GreenBitShift) &
                         EmulationVideoPixelConstants.FiveBitMask) * EmulationVideoPixelConstants.OpaqueAlpha /
                        EmulationVideoPixelConstants.FiveBitMaximum);
                    destinationRow[offset + EmulationVideoPixelConstants.RedByteOffset] = (byte)(
                        ((value >> EmulationVideoPixelConstants.Rgb1555RedBitShift) &
                         EmulationVideoPixelConstants.FiveBitMask) * EmulationVideoPixelConstants.OpaqueAlpha /
                        EmulationVideoPixelConstants.FiveBitMaximum);
                }
                else
                {
                    destinationRow[offset + EmulationVideoPixelConstants.GreenByteOffset] = (byte)(
                        ((value >> EmulationVideoPixelConstants.GreenBitShift) &
                         EmulationVideoPixelConstants.SixBitMask) * EmulationVideoPixelConstants.OpaqueAlpha /
                        EmulationVideoPixelConstants.SixBitMaximum);
                    destinationRow[offset + EmulationVideoPixelConstants.RedByteOffset] = (byte)(
                        ((value >> EmulationVideoPixelConstants.Rgb565RedBitShift) &
                         EmulationVideoPixelConstants.FiveBitMask) * EmulationVideoPixelConstants.OpaqueAlpha /
                        EmulationVideoPixelConstants.FiveBitMaximum);
                }
                destinationRow[offset + EmulationVideoPixelConstants.AlphaByteOffset] =
                    EmulationVideoPixelConstants.OpaqueAlpha;
            }
        }
        return destination;
    }
}
