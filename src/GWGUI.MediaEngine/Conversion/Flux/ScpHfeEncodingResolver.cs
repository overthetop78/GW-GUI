using GWGUI.MediaEngine.Containers.Hfe;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Conversion.Flux;

/// <summary>Associe les types SCP publics aux modes d'encodage représentables par HFE v1.</summary>
internal static class ScpHfeEncodingResolver
{
    public static byte Resolve(byte diskType) => (ScpDiskType)diskType switch
    {
        ScpDiskType.Amiga or ScpDiskType.AmigaHighDensity => HfeFormat.AmigaMfmEncoding,
        ScpDiskType.Atari8BitSingleDensity => HfeFormat.IsoFmEncoding,
        ScpDiskType.Atari8BitDoubleDensity or
        ScpDiskType.Atari8BitEnhancedDensity or
        ScpDiskType.AtariStSingleSided or
        ScpDiskType.AtariStDoubleSided or
        ScpDiskType.IbmPc360 or
        ScpDiskType.IbmPc720 or
        ScpDiskType.IbmPc1200 or
        ScpDiskType.IbmPc1440 or
        ScpDiskType.AmstradCpc or
        ScpDiskType.Other320 or
        ScpDiskType.Other1200 or
        ScpDiskType.Other720 or
        ScpDiskType.Other1440 => HfeFormat.IsoMfmEncoding,
        _ => throw new NotSupportedException(
            $"Le type SCP 0x{diskType:X2} ne possède pas de mode HFE v1 sans perte.")
    };
}
