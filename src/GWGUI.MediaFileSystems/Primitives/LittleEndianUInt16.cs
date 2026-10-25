namespace GWGUI.MediaFileSystems.Primitives;

/// <summary>Lit les entiers little-endian sur seize bits par les indexeurs de leur source.</summary>
public static class LittleEndianUInt16
{
    /// <summary>Lit deux octets successifs sans consulter la taille de la source.</summary>
    public static int ReadIndexed(IReadOnlyList<byte> data, int offset)
        => data[offset] | data[offset + 1] << Constants.BitConstants.BitsPerByte;
}
