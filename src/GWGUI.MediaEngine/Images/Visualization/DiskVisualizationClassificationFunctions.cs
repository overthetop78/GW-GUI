using GWGUI.MediaEngine.Contracts.Visualization;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Enums;

namespace GWGUI.MediaEngine.Images.Visualization;

public static class DiskVisualizationClassificationFunctions
{
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
        return machine is not null && DiskVisualizationDefaults.DecoderByMachine.TryGetValue(machine, out var decoder)
            ? decoder
            : DiskVisualizationDefaults.IsoMfmDecoder;
    }

    private static DiskMediaCategory ResolveMediaCategory(string? machine, string? formatId)
    {
        if (string.Equals(machine, DiskVisualizationDefaults.Amstrad, StringComparison.OrdinalIgnoreCase)) return DiskMediaCategory.ThreeInch;
        if (string.Equals(machine, DiskVisualizationDefaults.Dec, StringComparison.OrdinalIgnoreCase)) return DiskMediaCategory.EightInch;

        var id = formatId?.ToLowerInvariant() ?? string.Empty;
        if (machine is not null && DiskVisualizationDefaults.ThreeHalfMachines.Contains(machine))
            return IsHighDensity(id) ? DiskMediaCategory.ThreeHalfHd : DiskMediaCategory.ThreeHalfDd;
        if (machine is not null && DiskVisualizationDefaults.FiveQuarterMachines.Contains(machine))
            return id.Contains(DiskVisualizationDefaults.HighDensityMarker, StringComparison.Ordinal) ? DiskMediaCategory.FiveQuarterHd : DiskMediaCategory.FiveQuarterDd;
        return DiskMediaCategory.Unknown;
    }

    private static bool IsHighDensity(string formatId) =>
        formatId.Contains(DiskVisualizationDefaults.HighDensity1440, StringComparison.Ordinal)
        || formatId.Contains(DiskVisualizationDefaults.ExtendedDensity2880, StringComparison.Ordinal)
        || formatId.Contains(DiskVisualizationDefaults.HighDensitySuffix, StringComparison.Ordinal);
}
