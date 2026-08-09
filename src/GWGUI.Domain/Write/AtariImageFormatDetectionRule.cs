using System.Buffers.Binary;

namespace GWGUI.Domain.Write;

internal sealed class AtariImageFormatDetectionRule : IImageFormatDetectionRule
{
    public bool TryDetect(ImageFormatDetectionContext context, out DetectedImageFormat result)
    {
        var formatId = context.Extension switch
        {
            ".st" => DetectSt(context.KnownLength),
            ".msa" => DetectMsa(context),
            ".atr" => DetectAtr(context.KnownLength),
            _ => null
        };

        if (context.Extension is not (".st" or ".msa" or ".atr"))
        {
            result = null!;
            return false;
        }

        result = formatId is null
            ? context.Ambiguous("Detection.AtariUnknownSize")
            : context.Result(formatId, FormatConfidence.Certain, "Detection.AtariSize");
        return true;
    }

    private static string? DetectSt(long? length) => length switch
    {
        368640 => "atarist.360", 409600 => "atarist.400", 450560 => "atarist.440",
        737280 => "atarist.720", 819200 => "atarist.800", 829440 => "atarist.810",
        901120 => "atarist.880", 1474560 => "atarist.1440", _ => null
    };

    private static string? DetectAtr(long? length) => length switch
    {
        92176 => "atari.90", 133136 => "atari.130", 183952 => "atari.180", _ => null
    };

    private static string? DetectMsa(ImageFormatDetectionContext context)
    {
        try
        {
            using var stream = File.OpenRead(context.FilePath);
            Span<byte> header = stackalloc byte[10];
            if (stream.Read(header) != header.Length || BinaryPrimitives.ReadUInt16BigEndian(header) != 0x0e0f) return null;
            var sectorsPerTrack = BinaryPrimitives.ReadUInt16BigEndian(header[2..]);
            var heads = BinaryPrimitives.ReadUInt16BigEndian(header[4..]) + 1;
            var firstCylinder = BinaryPrimitives.ReadUInt16BigEndian(header[6..]);
            var lastCylinder = BinaryPrimitives.ReadUInt16BigEndian(header[8..]);
            if (sectorsPerTrack is < 1 or > 36 || heads is < 1 or > 2 || lastCylinder < firstCylinder) return null;
            var capacityKiB = checked((lastCylinder + 1) * heads * sectorsPerTrack / 2);
            var formatId = $"atarist.{capacityKiB}";
            return context.Catalog.Formats.Any(format => format.Id.Equals(formatId, StringComparison.OrdinalIgnoreCase))
                ? formatId
                : null;
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
        catch (OverflowException) { return null; }
    }
}
