using GWGUI.Domain.Commands;
using GWGUI.Domain.Read;
using GWGUI.Domain.Formats;

namespace GWGUI.Domain.Write;

public enum FormatConfidence { Certain, Inferred, Ambiguous, Manual }

public sealed record DetectedImageFormat(string Extension, DiskFormat? Format, FormatConfidence Confidence, IReadOnlyList<DiskFormat> Candidates, string Explanation)
{
    public bool RequiresUserChoice => Confidence == FormatConfidence.Ambiguous || Format is null;
}

public sealed class ImageFormatDetector(IImageFormatCatalog catalog)
{
    public DetectedImageFormat Detect(string filePath, long? knownLength = null)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var candidates = catalog.Formats.Where(x => x.Extensions.Any(e => e.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase))).ToArray();
        if (extension == ".scp") return Result(extension, "raw.scp", FormatConfidence.Certain, candidates, "Capture brute SCP reconnue.");
        if (extension == ".adf" && knownLength is 901120 or 1802240)
            return Result(extension, knownLength == 901120 ? "amiga.amigados" : "amiga.amigadoshd", FormatConfidence.Certain, candidates, "Format Amiga reconnu par sa taille.");
        if (extension == ".adf" && knownLength == 819200)
            return Result(extension, "acorn.adfs.800", FormatConfidence.Certain, candidates, "Format Acorn reconnu par sa taille.");
        if (extension == ".st") return Result(extension, SizeFormat(knownLength, "atarist.720", "atarist.1440"), knownLength is 737280 or 1474560 ? FormatConfidence.Certain : FormatConfidence.Inferred, candidates, "Image Atari ST reconnue par son extension et sa taille.");
        if (extension == ".msa") return Result(extension, "atarist.720", FormatConfidence.Inferred, candidates, "Conteneur Atari MSA reconnu; géométrie à confirmer si nécessaire.");
        if (extension is ".ima" or ".img")
        {
            var id = knownLength switch { 368640 => "ibm.360", 737280 => "ibm.720", 1474560 => "ibm.1440", _ => null };
            return id is null
                ? new(extension, null, FormatConfidence.Ambiguous, candidates, "L’extension seule ne permet pas de choisir la géométrie IBM PC.")
                : Result(extension, id, FormatConfidence.Certain, candidates, "Géométrie IBM PC reconnue par la taille.");
        }
        if (candidates.Length == 1) return new(extension, candidates[0], FormatConfidence.Inferred, candidates, "Format déduit de l’extension.");
        return new(extension, null, FormatConfidence.Ambiguous, candidates, "Plusieurs formats sont possibles; choisissez explicitement le format.");
    }

    private DetectedImageFormat Result(string extension, string? id, FormatConfidence confidence, IReadOnlyList<DiskFormat> candidates, string explanation) =>
        new(extension, catalog.Formats.FirstOrDefault(x => x.Id == id), id is null ? FormatConfidence.Ambiguous : confidence, candidates, explanation);

    private static string? SizeFormat(long? length, string low, string high) => length switch { 737280 => low, 1474560 => high, _ => low };
}

public sealed record WriteRequest(string GwExecutable, string SourcePath, string? FormatId, IReadOnlyList<EnabledOption> Options, bool DisableVerify = false, string? Device = null, string? Drive = null, string? ExpertArguments = null);

public static class WriteCommandBuilder
{
    public static GwCommand Build(WriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SourcePath)) throw new ArgumentException("A source image is required.");
        var arguments = new List<string>();
        Add(arguments, "--device", request.Device); Add(arguments, "--drive", request.Drive); Add(arguments, "--format", request.FormatId);
        if (request.DisableVerify) arguments.Add("--no-verify");
        foreach (var option in request.Options) { arguments.Add(option.Argument); if (!string.IsNullOrWhiteSpace(option.Value)) arguments.Add(option.Value); }
        if (!string.IsNullOrWhiteSpace(request.ExpertArguments)) arguments.AddRange(CommandLineTokenizer.Tokenize(request.ExpertArguments));
        arguments.Add(request.SourcePath);
        return new GwCommand(request.GwExecutable, "write", arguments);
    }

    private static void Add(List<string> values, string name, string? value) { if (!string.IsNullOrWhiteSpace(value)) { values.Add(name); values.Add(value); } }
}
