using System.Buffers.Binary;

namespace GWGUI.MediaEngine.FileSystems.Apple.Dos;

/// <summary>Encode et décode l'en-tête adresse/longueur des fichiers binaires Apple DOS.</summary>
internal static class AppleDosBinaryFileCodec
{
    /// <summary>Ajoute l'en-tête binaire en conservant l'adresse de chargement fournie.</summary>
    public static byte[] Encode(IReadOnlyList<byte> content, ushort loadAddress)
    {
        if (content.Count > ushort.MaxValue) throw new InvalidDataException("An Apple DOS binary file cannot exceed 65535 bytes.");
        var output = new byte[AppleDosFileSystemLayout.BinaryHeaderSize + content.Count];
        BinaryPrimitives.WriteUInt16LittleEndian(output.AsSpan(AppleDosFileSystemLayout.BinaryLoadAddressOffset), loadAddress);
        BinaryPrimitives.WriteUInt16LittleEndian(output.AsSpan(AppleDosFileSystemLayout.BinaryLengthOffset), checked((ushort)content.Count));
        content.ToArray().CopyTo(output, AppleDosFileSystemLayout.BinaryHeaderSize);
        return output;
    }

    /// <summary>Extrait le contenu logique et l'adresse lorsque l'en-tête est cohérent.</summary>
    public static bool TryDecode(IReadOnlyList<byte> stored, out IReadOnlyList<byte> content, out ushort loadAddress)
    {
        content = stored;
        loadAddress = 0;
        if (stored.Count < AppleDosFileSystemLayout.BinaryHeaderSize) return false;
        var bytes = stored.ToArray();
        loadAddress = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(AppleDosFileSystemLayout.BinaryLoadAddressOffset));
        var length = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(AppleDosFileSystemLayout.BinaryLengthOffset));
        if (length > bytes.Length - AppleDosFileSystemLayout.BinaryHeaderSize) return false;
        content = Array.AsReadOnly(bytes.AsSpan(AppleDosFileSystemLayout.BinaryHeaderSize, length).ToArray());
        return true;
    }
}
