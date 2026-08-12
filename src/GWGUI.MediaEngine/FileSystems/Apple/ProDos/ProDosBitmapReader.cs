using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Compte les blocs libres dans les blocs bitmap ProDOS validés.</summary>
internal static class ProDosBitmapReader
{
    /// <summary>Lit le bitmap jusqu'au total de blocs annoncé.</summary>
    public static ProDosBitmapResult Read(SectorImage image, int bitmapStart, int totalBlocks, ICollection<string> warnings)
    {
        var freeBlocks = 0;
        var valid = true;
        for (var block = 0; block < totalBlocks; block++)
        {
            var mapBlockNumber = bitmapStart + block / ProDosFileSystemLayout.BlocksPerBitmapBlock;
            if (!image.TryGetBlock(mapBlockNumber, out var bitmap) || bitmap.Data.Count != ProDosFileSystemLayout.BlockSize)
            {
                if (block % ProDosFileSystemLayout.BlocksPerBitmapBlock == 0) warnings.Add(ProDosFileSystemExceptions.InvalidBitmapBlock(mapBlockNumber));
                valid = false;
                continue;
            }
            var bit = block % ProDosFileSystemLayout.BlocksPerBitmapBlock;
            if ((bitmap.Data[bit / ProDosFileSystemLayout.BitsPerByte] & (ProDosFileSystemLayout.BitmapHighBitMask >> (bit & (ProDosFileSystemLayout.BitsPerByte - 1)))) != 0) freeBlocks++;
        }
        return new(freeBlocks, valid);
    }
}
