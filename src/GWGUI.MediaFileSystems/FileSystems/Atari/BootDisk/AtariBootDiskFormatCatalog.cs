using System.Collections.Frozen;
using GWGUI.MediaFileSystems.Constants;

namespace GWGUI.MediaFileSystems.FileSystems.Atari.BootDisk;

/// <summary>Formats d'images Atari 8 bits pouvant contenir un programme de démarrage direct.</summary>
internal static class AtariBootDiskFormatCatalog
{
    public static IReadOnlySet<string> FormatIds { get; } = new[]
    {
        MediaImageFormatIds.Atari90,
        MediaImageFormatIds.Atari130,
        MediaImageFormatIds.Atari140,
        MediaImageFormatIds.Atari180,
        MediaImageFormatIds.AtariAtx,
        MediaImageFormatIds.AtariXfd90,
        MediaImageFormatIds.AtariXfd130,
        MediaImageFormatIds.AtariXfd140,
        MediaImageFormatIds.AtariXfd180
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
}
