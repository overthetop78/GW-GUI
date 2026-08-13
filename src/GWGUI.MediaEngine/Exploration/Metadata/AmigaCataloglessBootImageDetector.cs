using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Metadata;

/// <summary>Reconnaît une image Amiga complète amorçable dont les données sont chargées directement par secteurs.</summary>
internal static class AmigaCataloglessBootImageDetector
{
    private const int BootBlockLength = 1024;
    private const int MinimumBootPayloadBytes = 64;
    private const int MinimumOccupiedBlockPercentage = 50;

    /// <summary>Vérifie la structure générale sans dépendre d'un jeu, d'un crack ou d'un nom présent dans l'image.</summary>
    public static bool IsMatch(SectorImage image, ReadOnlySpan<byte> bytes)
    {
        if (!image.FormatId.StartsWith(DiskImageFormatIds.AmigaPrefix, StringComparison.OrdinalIgnoreCase)
            || image.AvailableBlocks.Count != image.BlockCount
            || image.BlockSize <= 0
            || bytes.Length < BootBlockLength)
        {
            return false;
        }

        var bootPayloadBytes = 0;
        for (var offset = 12; offset < BootBlockLength; offset++)
        {
            if (bytes[offset] != 0) bootPayloadBytes++;
        }
        if (bootPayloadBytes < MinimumBootPayloadBytes) return false;

        var occupiedBlocks = 0;
        foreach (var block in image.AvailableBlocks)
        {
            if (block.Data.Any(value => value != 0)) occupiedBlocks++;
        }
        return occupiedBlocks * 100 >= image.BlockCount * MinimumOccupiedBlockPercentage;
    }
}
