namespace GWGUI.Domain.Conversion;

/// <summary>Détermine les garanties associées à chaque niveau de fidélité.</summary>
public static class ConversionFidelity
{
    public static ConversionFidelityLevel ForConversion(string sourceExtension, string targetExtension)
    {
        var source = Normalize(sourceExtension);
        var target = Normalize(targetExtension);
        if (source == target && source is ".scp" or ".hfe")
            return ConversionFidelityLevel.PreservedFlux;
        if (source == ".hfe" && target == ".scp")
            return ConversionFidelityLevel.PreservedFlux;
        return ForRebuiltOutput(target);
    }

    public static ConversionFidelityLevel ForRebuiltOutput(string extension) => extension.ToLowerInvariant() switch
    {
        ".scp" or ".hfe" => ConversionFidelityLevel.ReconstructedTracks,
        _ => ConversionFidelityLevel.SectorData
    };

    public static bool PreservesOriginalProtection(ConversionFidelityLevel level) => level == ConversionFidelityLevel.PreservedFlux;

    private static string Normalize(string extension) => extension.StartsWith('.')
        ? extension.ToLowerInvariant()
        : "." + extension.ToLowerInvariant();
}
