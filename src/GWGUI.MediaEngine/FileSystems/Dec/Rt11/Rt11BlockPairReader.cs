using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Dec.Rt11;

/// <summary>Lit les deux blocs d'un segment RT-11 sans décaler le second.</summary>
public static class Rt11BlockPairReader
{
    /// <summary>Lit une paire et réserve exactement un secteur par bloc invalide.</summary>
    public static Rt11BlockPairResult Read(SectorImage image, int firstBlock)
    {
        var bytes = new byte[Rt11FileSystemLayout.BlockSize * Rt11FileSystemLayout.SegmentBlockCount];
        var firstPresent = image.TryGetBlock(firstBlock, out var first);
        var secondPresent = image.TryGetBlock(firstBlock + 1, out var second);
        var firstValid = firstPresent && first!.Data.Count == Rt11FileSystemLayout.BlockSize;
        var secondValid = secondPresent && second!.Data.Count == Rt11FileSystemLayout.BlockSize;
        if (firstValid) first!.Data.ToArray().AsSpan().CopyTo(bytes);
        if (secondValid) second!.Data.ToArray().AsSpan().CopyTo(bytes.AsSpan(Rt11FileSystemLayout.BlockSize));
        return new(Array.AsReadOnly(bytes), firstPresent, secondPresent, firstValid, secondValid);
    }
}
