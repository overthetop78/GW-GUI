using System.Buffers.Binary;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Read;
using GWGUI.Domain.Formats;

namespace GWGUI.Domain.Write;

public enum FormatConfidence { Certain, Inferred, Ambiguous, Manual }

public sealed record DetectedImageFormat(string Extension, DiskFormat? Format, FormatConfidence Confidence, IReadOnlyList<DiskFormat> Candidates, string ExplanationKey)
{
    public bool RequiresUserChoice => Confidence == FormatConfidence.Ambiguous || Format is null;
}

public sealed class ImageFormatDetector(IImageFormatCatalog catalog)
{
    public DetectedImageFormat Detect(string filePath, long? knownLength = null)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var candidates = catalog.Formats.Where(x => x.Extensions.Any(e => e.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase))).ToArray();
        if (extension == ".scp") return Result(extension, "raw.scp", FormatConfidence.Certain, candidates, "Detection.RawScp");
        if (extension == ".adf" && knownLength is 901120 or 1802240)
            return Result(extension, knownLength == 901120 ? "amiga.amigados" : "amiga.amigados_hd", FormatConfidence.Certain, candidates, "Detection.AmigaSize");
        if (extension == ".adf" && knownLength == 819200)
            return Result(extension, "acorn.adfs.800", FormatConfidence.Certain, candidates, "Detection.AcornSize");
        if (extension == ".st")
        {
            var id = knownLength switch { 368640 => "atarist.360", 409600 => "atarist.400", 450560 => "atarist.440", 737280 => "atarist.720", 819200 => "atarist.800", 901120 => "atarist.880", _ => null };
            return id is null ? new(extension, null, FormatConfidence.Ambiguous, candidates, "Detection.AtariUnknownSize") : Result(extension, id, FormatConfidence.Certain, candidates, "Detection.AtariSize");
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
            return id is null ? new(extension, null, FormatConfidence.Ambiguous, candidates, "Detection.AtariUnknownSize") : Result(extension, id, FormatConfidence.Certain, candidates, "Detection.AtariSize");
        }
        if (extension is ".ima" or ".img")
        {
            var id = knownLength switch { 163840 => "ibm.160", 184320 => "ibm.180", 327680 => "ibm.320", 368640 => "ibm.360", 737280 => "ibm.720", 819200 => "ibm.800", 1228800 => "ibm.1200", 1474560 => "ibm.1440", 1720320 => "ibm.1680", 2949120 => "ibm.2880", _ => null };
            return id is null
                ? new(extension, null, FormatConfidence.Ambiguous, candidates, "Detection.IbmAmbiguous")
                : Result(extension, id, FormatConfidence.Certain, candidates, "Detection.IbmSize");
        }
        if (candidates.Length == 1) return new(extension, candidates[0], FormatConfidence.Inferred, candidates, "Detection.ExtensionInferred");
        return new(extension, null, FormatConfidence.Ambiguous, candidates, "Detection.Multiple");
    }

    private DetectedImageFormat Result(string extension, string? id, FormatConfidence confidence, IReadOnlyList<DiskFormat> candidates, string explanation) =>
        new(extension, catalog.Formats.FirstOrDefault(x => x.Id == id), id is null ? FormatConfidence.Ambiguous : confidence, candidates, explanation);

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

public sealed record WriteRequest(string GwExecutable, string SourcePath, string? FormatId, IReadOnlyList<EnabledOption> Options, bool DisableVerify = false, string? Device = null, string? Drive = null, string? ExpertArguments = null);

public static class WriteCommandBuilder
{
    public static GwCommand Build(WriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SourcePath)) throw new ArgumentException("A source image is required.");
        GwOptionValidator.Validate(request.Options);
        var arguments = new List<string>();
        Add(arguments, "--device", request.Device); Add(arguments, "--drive", request.Drive); Add(arguments, "--format", GwFormatArgument.FromCatalogId(request.FormatId));
        if (request.DisableVerify) arguments.Add("--no-verify");
        foreach (var option in request.Options) { arguments.Add(option.Argument); if (!string.IsNullOrWhiteSpace(option.Value)) arguments.Add(option.Value); }
        if (!string.IsNullOrWhiteSpace(request.ExpertArguments)) arguments.AddRange(CommandLineTokenizer.Tokenize(request.ExpertArguments));
        arguments.Add(request.SourcePath);
        return new GwCommand(request.GwExecutable, "write", arguments);
    }

    private static void Add(List<string> values, string name, string? value) { if (!string.IsNullOrWhiteSpace(value)) { values.Add(name); values.Add(value); } }
}
