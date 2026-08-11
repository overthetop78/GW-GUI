using GWGUI.MediaEngine.Definitions;

namespace GWGUI.MediaEngine.Containers.Atari.Msa;

internal static class MsaFormat
{
    public const ushort Signature = 0x0E0F;
    public const byte RleMarker = 0xE5;
    public static string FormatId(long capacity) => DiskImageFormatIds.AtariStFromCapacity(capacity);
}
