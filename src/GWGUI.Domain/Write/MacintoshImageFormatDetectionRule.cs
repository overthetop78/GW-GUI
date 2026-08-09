using System.Buffers.Binary;

namespace GWGUI.Domain.Write;

internal sealed class MacintoshImageFormatDetectionRule : IImageFormatDetectionRule
{
    public bool TryDetect(ImageFormatDetectionContext context, out DetectedImageFormat result)
    {
        if (context.Extension is ".image" or ".dc42")
        {
            var formatId = context.KnownLength switch
            {
                409684 or 419284 => "mac.400",
                819284 or 838484 => "mac.800",
                1474644 or 1491844 => "mac.1440",
                _ => null
            };
            result = formatId is null
                ? context.Ambiguous("Detection.AppleContainer")
                : context.Result(formatId, FormatConfidence.Inferred, "Detection.AppleContainer");
            return true;
        }

        if (context.Extension == ".img" && TryDetectRawImage(context.FilePath, context.KnownLength, out var rawFormatId))
        {
            result = context.Result(rawFormatId, FormatConfidence.Certain, "Detection.AppleContainer");
            return true;
        }

        result = null!;
        return false;
    }

    private static bool TryDetectRawImage(string filePath, long? knownLength, out string? formatId)
    {
        formatId = knownLength switch { 409_600 => "mac.400", 819_200 => "mac.800", 1_474_560 => "mac.1440", _ => null };
        if (formatId is null) return false;
        try
        {
            using var stream = File.OpenRead(filePath);
            if (stream.Length < 1026) return false;
            stream.Position = 1024;
            Span<byte> signature = stackalloc byte[2];
            return stream.Read(signature) == 2 && BinaryPrimitives.ReadUInt16BigEndian(signature) is 0xd2d7 or 0x4244;
        }
        catch (IOException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
    }
}
