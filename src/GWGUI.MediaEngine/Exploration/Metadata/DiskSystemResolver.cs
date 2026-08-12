using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Recognition.Definitions;

namespace GWGUI.MediaEngine.Exploration.Metadata;

/// <summary>Résout un identifiant de format en identifiant technique de système.</summary>
internal sealed class DiskSystemResolver
{
    /// <summary>Résout le système en testant les familles les plus précises avant leurs familles voisines.</summary>
    /// <param name="formatId">Identifiant technique du format d'image.</param>
    /// <returns>Identifiant technique du système, ou <see langword="null"/> si sa famille est inconnue.</returns>
    public string? ResolveId(string formatId)
    {
        if (formatId.StartsWith(DiskImageFormatIds.AppleMacPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.MacPrefix, StringComparison.OrdinalIgnoreCase)) return DiskSystemIds.Macintosh;
        if (formatId.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith("lisa.", StringComparison.OrdinalIgnoreCase)) return DiskSystemIds.Lisa;
        if (formatId.StartsWith(DiskImageFormatIds.AppleIIPrefix, StringComparison.OrdinalIgnoreCase)) return DiskSystemIds.AppleII;
        if (formatId.StartsWith(DiskImageFormatIds.AppleIIIPrefix, StringComparison.OrdinalIgnoreCase)) return DiskSystemIds.AppleIII;
        if (formatId.StartsWith(DiskImageFormatIds.AmigaPrefix, StringComparison.OrdinalIgnoreCase)) return DiskSystemIds.Amiga;
        if (formatId.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase)) return DiskSystemIds.AtariSt;
        if (formatId.StartsWith(DiskImageFormatIds.AtariPrefix, StringComparison.OrdinalIgnoreCase)) return DiskSystemIds.Atari8Bit;
        if (formatId.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase)) return DiskSystemIds.IbmPc;
        if (formatId.StartsWith(DiskImageFormatIds.Commodore900Prefix, StringComparison.OrdinalIgnoreCase)) return DiskSystemIds.Coherent;
        if (formatId.StartsWith(DiskImageFormatIds.CommodorePrefix, StringComparison.OrdinalIgnoreCase)) return DiskSystemIds.Commodore;
        if (formatId.StartsWith(DiskImageFormatIds.AmstradPrefix, StringComparison.OrdinalIgnoreCase)) return DiskSystemIds.Amstrad;
        if (formatId.StartsWith(DiskImageFormatIds.AcornAdfsPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.AcornDfsPrefix, StringComparison.OrdinalIgnoreCase)) return DiskSystemIds.AcornBbc;
        if (formatId.StartsWith(DiskImageFormatIds.EpsonQx10Prefix, StringComparison.OrdinalIgnoreCase)) return DiskSystemIds.EpsonQx10;
        if (formatId.StartsWith(DiskImageFormatIds.MsxPrefix, StringComparison.OrdinalIgnoreCase)) return DiskSystemIds.Msx;
        if (formatId.StartsWith(DiskImageFormatIds.DecPrefix, StringComparison.OrdinalIgnoreCase)) return DiskSystemIds.Dec;
        if (formatId.StartsWith(DiskImageFormatIds.UcsdPrefix, StringComparison.OrdinalIgnoreCase)) return DiskSystemIds.Ucsd;
        return null;
    }
}
