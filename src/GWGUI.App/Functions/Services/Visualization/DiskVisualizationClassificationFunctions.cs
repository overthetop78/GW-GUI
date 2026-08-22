using GWGUI.App.Contracts.Services.Visualization;
using GWGUI.App.Enums.Rendering.Scp;
namespace GWGUI.App.Functions.Services.Visualization;

public static class DiskVisualizationClassificationFunctions
{
    private static readonly IReadOnlyDictionary<string, string> DecoderByMachine =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Apple II"] = "apple2.gcr",
            ["Apple Macintosh"] = "applemac.gcr",
            ["Apple Lisa"] = "applelisa.fileware.gcr",
            ["Amiga"] = "amiga.mfm",
            ["Commodore"] = "commodore.gcr",
            ["DEC"] = "dec.rx02"
        };

    private static readonly IReadOnlySet<string> ThreeHalfMachines =
        new HashSet<string>(["Atari ST", "Amiga", "IBM PC", "Apple Macintosh", "MSX"], StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlySet<string> FiveQuarterMachines =
        new HashSet<string>(["Apple II", "Commodore", "Acorn", "Acorn / BBC Micro"], StringComparer.OrdinalIgnoreCase);

    public static DiskVisualizationClassification Resolve(
        string? machine,
        string? formatId,
        string? protectionId,
        bool automaticDetection)
    {
        var decoderId = protectionId ?? ResolveDecoder(machine, automaticDetection);
        return new(decoderId, ResolveMediaCategory(machine, formatId));
    }

    private static string? ResolveDecoder(string? machine, bool automaticDetection)
    {
        if (automaticDetection && string.IsNullOrWhiteSpace(machine)) return null;
        return machine is not null && DecoderByMachine.TryGetValue(machine, out var decoder)
            ? decoder
            : "iso.mfm";
    }

    private static DiskMediaCategory ResolveMediaCategory(string? machine, string? formatId)
    {
        if (string.Equals(machine, "Amstrad", StringComparison.OrdinalIgnoreCase)) return DiskMediaCategory.ThreeInch;
        if (string.Equals(machine, "DEC", StringComparison.OrdinalIgnoreCase)) return DiskMediaCategory.EightInch;

        var id = formatId?.ToLowerInvariant() ?? string.Empty;
        if (machine is not null && ThreeHalfMachines.Contains(machine))
            return IsHighDensity(id) ? DiskMediaCategory.ThreeHalfHd : DiskMediaCategory.ThreeHalfDd;
        if (machine is not null && FiveQuarterMachines.Contains(machine))
            return id.Contains("hd", StringComparison.Ordinal) ? DiskMediaCategory.FiveQuarterHd : DiskMediaCategory.FiveQuarterDd;
        return DiskMediaCategory.Unknown;
    }

    private static bool IsHighDensity(string formatId) =>
        formatId.Contains("1440", StringComparison.Ordinal)
        || formatId.Contains("2880", StringComparison.Ordinal)
        || formatId.Contains("_hd", StringComparison.Ordinal);
}
