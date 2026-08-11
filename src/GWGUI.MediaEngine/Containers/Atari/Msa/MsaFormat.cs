using GWGUI.MediaEngine.Definitions;

namespace GWGUI.MediaEngine.Containers.Atari.Msa;

/// <summary>Définit l'identification et l'encodage d'un conteneur MSA.</summary>
internal static class MsaFormat
{
    /// <summary>Signature big-endian du conteneur.</summary>
    public const ushort Signature = 0x0E0F;
    /// <summary>Marqueur introduisant une répétition RLE.</summary>
    public const byte RleMarker = 0xE5;
    /// <summary>Construit l'identifiant Atari ST correspondant à la capacité reconstruite.</summary>
    public static string FormatId(long capacity) => DiskImageFormatIds.AtariStFromCapacity(capacity);
}
