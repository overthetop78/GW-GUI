using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Calcule le checksum du format Data General 2F.</summary>
internal static class DataGeneralChecksum
{
    /// <summary>Calcule le checksum, y compris l'itération terminale avec un octet nul.</summary>
    /// <param name="data">Octets de la charge utile.</param>
    /// <returns>Checksum sur seize bits.</returns>
    public static ushort Calculate(IEnumerable<byte> data)
    {
        ushort value = 0;
        foreach (var input in data.Append((byte)0)) value = (ushort)(((value & byte.MaxValue) ^ (value >> BitPrimitives.BitsPerByte)) | (((value & byte.MaxValue) ^ input) << BitPrimitives.BitsPerByte));
        return value;
    }
}
