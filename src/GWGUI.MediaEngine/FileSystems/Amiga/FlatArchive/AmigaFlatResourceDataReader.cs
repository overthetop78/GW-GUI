using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Amiga.FlatArchive;

/// <summary>Lit une plage logique à travers les blocs disponibles et conserve ses anomalies physiques.</summary>
internal static class AmigaFlatResourceDataReader
{
    public static AmigaFlatResourceReadResult Read(SectorImage image, long offset, long length)
    {
        if (offset < 0 || length < 0 || offset + length > image.Capacity || length > int.MaxValue)
            throw AmigaFlatResourceArchiveExceptions.RangeOutsideImage(offset, length, image.Capacity);
        var bytes = new byte[(int)length];
        var missing = new List<int>();
        var invalid = new List<int>();
        var destination = 0;
        while (destination < bytes.Length)
        {
            var absolute = offset + destination;
            var blockNumber = checked((int)(absolute / image.BlockSize));
            var withinBlock = checked((int)(absolute % image.BlockSize));
            var count = Math.Min(bytes.Length - destination, image.BlockSize - withinBlock);
            if (!image.TryGetBlock(blockNumber, out var block)) missing.Add(blockNumber);
            else
            {
                if (block.IntegrityValid != true) invalid.Add(blockNumber);
                block.Data.Skip(withinBlock).Take(count).ToArray().CopyTo(bytes, destination);
            }
            destination += count;
        }
        return new(bytes, missing, invalid);
    }
}
