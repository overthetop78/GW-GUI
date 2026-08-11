namespace GWGUI.MediaEngine.Recognition.Amstrad;

/// <summary>Vérifie les champs de spécification disque placés au début des images Amstrad PCW.</summary>
internal static class PcwDiskSpecificationProbe
{
    /// <summary>Indique si les données contiennent une spécification disque PCW plausible.</summary>
    /// <param name="bytes">Données commençant par la spécification disque.</param>
    /// <returns><see langword="true"/> lorsque les dimensions et allocations déclarées sont acceptables.</returns>
    public static bool LooksLikePcwDiskSpecification(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 512) return false;
        var sectorsPerTrack = bytes[3];
        var sectorSize = 128 << (bytes[4] & 7);
        var reservedTracks = bytes[5];
        var allocationSize = 128 << (bytes[6] & 7);
        var directoryBlocks = bytes[7];
        return bytes[2] is > 0 and <= 96 && sectorsPerTrack is > 0 and <= 64 && sectorSize is >= 128 and <= 4096 && reservedTracks <= 8 && allocationSize is >= 512 and <= 16384 && directoryBlocks is > 0 and <= 16;
    }
}
