namespace GWGUI.MediaFileSystems.FileSystems.Acorn.Adfs;

/// <summary>Initialise une carte ADFS BBC ancienne et son répertoire racine, sans conteneur ni accès fichier.</summary>
public static class AcornAdfsFormatter
{
    public const int SectorSize = 256, FirstFreeSector = 7, HeaderSize = FirstFreeSector * SectorSize;

    /// <summary>Produit les sept secteurs système pour une capacité exprimée en secteurs de 256 octets.</summary>
    /// <remarks>Les secteurs 0/1 contiennent une plage libre unique ; le répertoire Hugo occupe 2 à 6.
    /// Le même en-tête convient aux disquettes S/M/L et aux volumes Winchester anciens.</remarks>
    public static byte[] CreateHeader(int sectorCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sectorCount, FirstFreeSector);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sectorCount, 0xffffff);
        var bytes = new byte[HeaderSize];
        Write24(bytes, 0, FirstFreeSector); Write24(bytes, 0x100, sectorCount - FirstFreeSector); Write24(bytes, 0xfc, sectorCount);
        bytes[0x1fe] = 3;
        bytes.AsSpan(0xf7, 5).Fill((byte)' '); bytes.AsSpan(0x1f6, 5).Fill((byte)' ');
        bytes[0xff] = Checksum(bytes.AsSpan(0, SectorSize)); bytes[0x1ff] = Checksum(bytes.AsSpan(SectorSize, SectorSize));
        var root = SectorSize * 2;
        "Hugo"u8.CopyTo(bytes.AsSpan(root + 1)); "Hugo"u8.CopyTo(bytes.AsSpan(root + 1275));
        bytes[root + 1228] = (byte)'$'; bytes[root + 1241] = (byte)'$';
        Write24(bytes, root + 1238, 2);
        return bytes;
    }

    private static void Write24(byte[] bytes, int offset, int value) { bytes[offset] = (byte)value; bytes[offset + 1] = (byte)(value >> 8); bytes[offset + 2] = (byte)(value >> 16); }
    private static byte Checksum(ReadOnlySpan<byte> bytes)
    {
        var sum = 0; var carry = 0;
        for (var index = SectorSize - 2; index >= 0; index--) { var next = sum + carry + bytes[index]; sum = next & 255; carry = next >> 8; }
        return (byte)sum;
    }
}
