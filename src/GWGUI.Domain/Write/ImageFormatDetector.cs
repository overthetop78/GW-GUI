using System.Buffers.Binary;
using GWGUI.Domain.Formats;

namespace GWGUI.Domain.Write;

public sealed class ImageFormatDetector(IImageFormatCatalog catalog)
{
    public DetectedImageFormat Detect(string filePath, long? knownLength = null)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var candidates = catalog.Formats
            .Where(format => format.Extensions.Any(item => item.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        if (extension == ".scp") return Result(extension, "raw.scp", FormatConfidence.Certain, candidates, "Detection.RawScp");
        if (extension == ".adf" && knownLength is 901120 or 1802240)
            return Result(extension, knownLength == 901120 ? "amiga.amigados" : "amiga.amigados_hd", FormatConfidence.Certain, candidates, "Detection.AmigaSize");
        if (extension == ".adf" && knownLength is 819200 or 820224)
            return Result(extension, "acorn.adfs.800", FormatConfidence.Certain, candidates, "Detection.AcornSize");

        if (extension == ".st")
        {
            var id = knownLength switch
            {
                368640 => "atarist.360", 409600 => "atarist.400", 450560 => "atarist.440",
                737280 => "atarist.720", 819200 => "atarist.800", 829440 => "atarist.810",
                901120 => "atarist.880", 1474560 => "atarist.1440", _ => null
            };
            return id is null
                ? new(extension, null, FormatConfidence.Ambiguous, candidates, "Detection.AtariUnknownSize")
                : Result(extension, id, FormatConfidence.Certain, candidates, "Detection.AtariSize");
        }

        if (extension == ".msa")
        {
            var id = TryDetectMsaFormat(filePath);
            return id is null
                ? new(extension, null, FormatConfidence.Ambiguous, candidates, "Detection.AtariUnknownSize")
                : Result(extension, id, FormatConfidence.Certain, candidates, "Detection.AtariSize");
        }

        if (extension == ".atr")
        {
            var id = knownLength switch { 92176 => "atari.90", 133136 => "atari.130", 183952 => "atari.180", _ => null };
            return id is null
                ? new(extension, null, FormatConfidence.Ambiguous, candidates, "Detection.AtariUnknownSize")
                : Result(extension, id, FormatConfidence.Certain, candidates, "Detection.AtariSize");
        }

        if (extension == ".d13") return Result(extension, "apple2.appledos.113", FormatConfidence.Certain, candidates, "Detection.AppleDosOrder");
        if (extension == ".do") return Result(extension, "apple2.appledos.140", FormatConfidence.Certain, candidates, "Detection.AppleDosOrder");
        if (extension == ".po") return Result(extension, "apple2.prodos.140", FormatConfidence.Certain, candidates, "Detection.AppleProDosOrder");
        if (extension == ".2mg") return new(extension, null, FormatConfidence.Ambiguous, candidates, "Detection.AppleContainer");

        if (extension is ".image" or ".dc42")
        {
            var id = knownLength switch
            {
                409684 or 419284 => "mac.400", 819284 or 838484 => "mac.800",
                1474644 or 1491844 => "mac.1440", _ => null
            };
            return id is null
                ? new(extension, null, FormatConfidence.Ambiguous, candidates, "Detection.AppleContainer")
                : Result(extension, id, FormatConfidence.Inferred, candidates, "Detection.AppleContainer");
        }

        if (extension == ".img" && TryDetectMacRawImage(filePath, knownLength, out var macId))
            return Result(extension, macId, FormatConfidence.Certain, candidates, "Detection.AppleContainer");

        if (extension is ".ima" or ".img")
        {
            var id = knownLength switch
            {
                163840 => "ibm.160", 184320 => "ibm.180", 327680 => "ibm.320", 368640 => "ibm.360",
                737280 => "ibm.720", 819200 => "ibm.800", 1228800 => "ibm.1200", 1474560 => "ibm.1440",
                1720320 => "ibm.1680", 2949120 => "ibm.2880", _ => null
            };
            return id is null
                ? new(extension, null, FormatConfidence.Ambiguous, candidates, "Detection.IbmAmbiguous")
                : Result(extension, id, FormatConfidence.Certain, candidates, "Detection.IbmSize");
        }

        if (candidates.Length == 1)
            return new(extension, candidates[0], FormatConfidence.Inferred, candidates, "Detection.ExtensionInferred");
        return new(extension, null, FormatConfidence.Ambiguous, candidates, "Detection.Multiple");
    }

    private static bool TryDetectMacRawImage(string filePath, long? knownLength, out string? formatId)
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

    private DetectedImageFormat Result(
        string extension,
        string? id,
        FormatConfidence confidence,
        IReadOnlyList<DiskFormat> candidates,
        string explanation) =>
        new(extension, catalog.Formats.FirstOrDefault(format => format.Id == id),
            id is null ? FormatConfidence.Ambiguous : confidence, candidates, explanation);

    private string? TryDetectMsaFormat(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            Span<byte> header = stackalloc byte[10];
            if (stream.Read(header) != header.Length || BinaryPrimitives.ReadUInt16BigEndian(header) != 0x0e0f) return null;
            var sectorsPerTrack = BinaryPrimitives.ReadUInt16BigEndian(header[2..]);
            var heads = BinaryPrimitives.ReadUInt16BigEndian(header[4..]) + 1;
            var firstCylinder = BinaryPrimitives.ReadUInt16BigEndian(header[6..]);
            var lastCylinder = BinaryPrimitives.ReadUInt16BigEndian(header[8..]);
            if (sectorsPerTrack is < 1 or > 36 || heads is < 1 or > 2 || lastCylinder < firstCylinder) return null;
            var capacityKiB = checked((lastCylinder + 1) * heads * sectorsPerTrack / 2);
            var id = $"atarist.{capacityKiB}";
            return catalog.Formats.Any(format => format.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) ? id : null;
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
        catch (OverflowException) { return null; }
    }
}
