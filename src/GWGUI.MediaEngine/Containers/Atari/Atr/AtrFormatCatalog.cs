using GWGUI.MediaEngine.Definitions;

namespace GWGUI.MediaEngine.Containers.Atari.Atr;

/// <summary>Centralise les trois géométries ATR prises en charge en écriture.</summary>
internal static class AtrFormatCatalog
{
    /// <summary>Profils ATR connus indexés par identifiant.</summary>
    private static readonly IReadOnlyDictionary<string, AtrFormatProfile> Profiles = new Dictionary<string, AtrFormatProfile>(StringComparer.OrdinalIgnoreCase)
    {
        [DiskImageFormatIds.Atari90] = new(DiskImageFormatIds.Atari90, AtrLayout.SingleDensitySectorSize, AtrLayout.StandardSectorCount),
        [DiskImageFormatIds.Atari130] = new(DiskImageFormatIds.Atari130, AtrLayout.SingleDensitySectorSize, AtrLayout.EnhancedDensitySectorCount),
        [DiskImageFormatIds.Atari180] = new(DiskImageFormatIds.Atari180, AtrLayout.DoubleDensitySectorSize, AtrLayout.StandardSectorCount)
    };

    /// <summary>Résout un profil ATR catalogué.</summary>
    public static bool TryGet(string formatId, out AtrFormatProfile profile) => Profiles.TryGetValue(formatId, out profile!);
}
