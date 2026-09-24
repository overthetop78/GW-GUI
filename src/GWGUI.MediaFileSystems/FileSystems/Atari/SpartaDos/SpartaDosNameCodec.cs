using System.Text;

namespace GWGUI.MediaFileSystems.FileSystems.Atari.SpartaDos;

internal static class SpartaDosNameCodec
{
    public static string Decode(ReadOnlySpan<byte> bytes) => Encoding.ASCII.GetString(bytes).TrimEnd((char)SpartaDosFileSystemLayout.NamePadding, '\0');

    public static string DecodeFileName(ReadOnlySpan<byte> name, ReadOnlySpan<byte> extension)
    {
        var decodedName = Decode(name);
        var decodedExtension = Decode(extension);
        return decodedExtension.Length == 0 ? decodedName : $"{decodedName}.{decodedExtension}";
    }
}
