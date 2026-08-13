namespace GWGUI.MediaEngine.Containers.Hfe;

/// <summary>Convertit les cellules HFE dont l'ordre des bits est inversé dans chaque octet.</summary>
internal static class HfeBitPacking
{
    public static byte[] Pack(IReadOnlyList<bool> bits)
    {
        var bytes = new byte[(bits.Count + HfeFormat.BitsPerByte - 1) / HfeFormat.BitsPerByte];
        for (var index = 0; index < bits.Count; index++) if (bits[index]) bytes[index / HfeFormat.BitsPerByte] |= (byte)(1 << (index % HfeFormat.BitsPerByte));
        return bytes;
    }

    public static IReadOnlyList<bool> Unpack(ReadOnlySpan<byte> bytes)
    {
        var bits = new bool[bytes.Length * HfeFormat.BitsPerByte];
        for (var index = 0; index < bits.Length; index++) bits[index] = (bytes[index / HfeFormat.BitsPerByte] & 1 << (index % HfeFormat.BitsPerByte)) != 0;
        return Array.AsReadOnly(bits);
    }
}
