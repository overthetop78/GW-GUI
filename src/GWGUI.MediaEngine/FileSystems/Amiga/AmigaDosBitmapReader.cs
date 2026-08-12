using System.Numerics;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Amiga;

/// <summary>Compte les blocs libres décrits par les bitmaps AmigaDOS.</summary>
public static class AmigaDosBitmapReader
{
    /// <summary>Compte les bits libres et ajoute les avertissements des bitmaps absents ou invalides.</summary>
    public static int CountFreeBlocks(SectorImage image, ReadOnlySpan<byte> root, ICollection<string> warnings)
    {
        var count = 0;
        for (var pointer = 0; pointer < AmigaDosLayout.MaximumBitmapPointerCount; pointer++)
        {
            var bitmapBlock = BigEndianInt32.Read(root, AmigaDosLayout.BitmapPointersOffset + pointer * AmigaDosLayout.WordSize);
            if (bitmapBlock == 0) break;
            if (!image.TryGetBlock(bitmapBlock, out var sector))
            {
                warnings.Add(AmigaDosWarnings.MissingBitmap(bitmapBlock));
                continue;
            }
            var bitmap = sector.Data.ToArray();
            if (!AmigaDosChecksum.IsValid(bitmap)) warnings.Add(AmigaDosWarnings.InvalidBitmapChecksum(bitmapBlock));
            for (var offset = AmigaDosLayout.BitmapDataOffset; offset < AmigaDosLayout.BlockSize; offset += AmigaDosLayout.WordSize) count += BitOperations.PopCount(BigEndianInt32.ReadUnsigned(bitmap, offset));
        }
        return Math.Min(count, image.BlockCount);
    }
}
