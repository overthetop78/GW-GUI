namespace GWGUI.MediaEngine.FileSystems.Dec.Rt11;

/// <summary>Valide les champs structurants d'un home block RT-11 remis en ordre logique.</summary>
internal static class Rt11HomeBlockProbe
{
    /// <summary>Indique si le bloc contient un numéro de répertoire et un identifiant système RT-11 valides.</summary>
    public static bool LooksLikeRt11(ReadOnlySpan<byte> homeBlock)
    {
        if (homeBlock.Length != Rt11FileSystemLayout.BlockSize) return false;
        var directoryBlock = Rt11Primitives.ReadUInt16(homeBlock, Rt11FileSystemLayout.DirectoryBlockOffset);
        var systemId = Rt11Primitives.DecodeAscii(homeBlock.Slice(Rt11FileSystemLayout.SystemIdOffset, Rt11FileSystemLayout.SystemIdLength));
        return directoryBlock is >= Rt11FileSystemLayout.MinimumDirectoryBlock and < Rt11FileSystemLayout.MaximumDirectoryBlockExclusive && systemId.StartsWith(Rt11FileSystemLayout.SystemSignature, StringComparison.Ordinal);
    }
}
