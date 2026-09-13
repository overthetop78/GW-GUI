using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Formats.Floppy.Atr;

/// <summary>Regroupe la signature et les identifiants produits lors de la lecture d'un conteneur ATR.</summary>
internal static class AtrFormat
{
    /// <summary>Signature little-endian placée au début d'un conteneur ATR.</summary>
    public const ushort Signature = 0x0296;

    /// <summary>Identifiant d'une image Atari simple densité de 90 Kio.</summary>
    public const string SingleDensityFormatId = DiskImageFormatIds.Atari90;

    /// <summary>Identifiant d'une image Atari à densité améliorée de 130 Kio.</summary>
    public const string EnhancedDensityFormatId = DiskImageFormatIds.Atari130;

    /// <summary>Identifiant d'une image Atari étendue de 140 Kio.</summary>
    public const string ExtendedSingleDensityFormatId = DiskImageFormatIds.Atari140;

    /// <summary>Identifiant d'une image Atari double densité de 180 Kio.</summary>
    public const string DoubleDensityFormatId = DiskImageFormatIds.Atari180;

    /// <summary>Retourne l'identifiant correspondant à la taille et au nombre de secteurs lus.</summary>
    /// <param name="sectorSize">Taille nominale d'un secteur, en octets.</param>
    /// <param name="sectorCount">Nombre total de secteurs.</param>
    /// <returns>L'identifiant Atari connu ou un identifiant ATR décrivant la géométrie observée.</returns>
    public static string GetFormatId(int sectorSize, int sectorCount) => (sectorSize, sectorCount) switch
    {
        (AtrLayout.SingleDensitySectorSize, > 0 and < AtrLayout.StandardSectorCount) => SingleDensityFormatId,
        (AtrLayout.SingleDensitySectorSize, AtrLayout.StandardSectorCount) => SingleDensityFormatId,
        (AtrLayout.SingleDensitySectorSize, AtrLayout.EnhancedDensitySectorCount) => EnhancedDensityFormatId,
        (AtrLayout.SingleDensitySectorSize, AtrLayout.ExtendedSingleDensitySectorCount) => ExtendedSingleDensityFormatId,
        (AtrLayout.DoubleDensitySectorSize, AtrLayout.StandardSectorCount) => DoubleDensityFormatId,
        _ => DiskImageFormatIds.AtariAtr(sectorSize, sectorCount)
    };
}
