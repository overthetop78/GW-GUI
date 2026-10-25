namespace GWGUI.MediaFileSystems.Primitives;

/// <summary>Copie une plage d'octets par les indexeurs de sa source.</summary>
public static class ByteListCopy
{
    /// <summary>Alloue une copie indépendante sans énumérer la source ni consulter sa taille.</summary>
    public static byte[] Copy(IReadOnlyList<byte> source, int offset, int length)
    {
        var result = new byte[length];
        for (var index = 0; index < length; index++) result[index] = source[offset + index];
        return result;
    }
}
