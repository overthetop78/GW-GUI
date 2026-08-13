namespace GWGUI.Domain.Conversion;

/// <summary>Détermine les garanties associées à chaque niveau de fidélité.</summary>
public static class ConversionFidelity
{
    public static ConversionFidelityLevel ForRebuiltOutput(string extension) => extension.ToLowerInvariant() switch
    {
        ".scp" or ".hfe" => ConversionFidelityLevel.ReconstructedTracks,
        _ => ConversionFidelityLevel.SectorData
    };

    public static bool PreservesOriginalProtection(ConversionFidelityLevel level) => level == ConversionFidelityLevel.PreservedFlux;
}
