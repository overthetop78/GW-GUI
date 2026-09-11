using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.FileSystems.Definitions;

namespace GWGUI.MediaEngine.FileSystems.Iso9660;

/// <summary>Reads ISO 9660 supplementary volume descriptors carrying Joliet UCS-2 names.</summary>
public sealed class JolietExtensionReader : Iso9660FileSystemReader
{
    public override string Id => FileSystemIds.Joliet;

    protected override bool AcceptVolumeDescriptor(ReadOnlySpan<byte> descriptor)
    {
        if (descriptor[0] != Iso9660Constants.SupplementaryVolumeDescriptorType || descriptor.Length < 91)
            return false;
        return descriptor.Slice(88, 3).SequenceEqual("%/@"u8)
            || descriptor.Slice(88, 3).SequenceEqual("%/C"u8)
            || descriptor.Slice(88, 3).SequenceEqual("%/E"u8);
    }

    protected override string DecodeVolumeIdentifier(ReadOnlySpan<byte> value) =>
        System.Text.Encoding.BigEndianUnicode.GetString(value).TrimEnd(' ', '\0');

    protected override string DecodeFileIdentifier(ReadOnlySpan<byte> value)
    {
        if (value.Length % 2 != 0) throw new InvalidDataException("A Joliet file identifier has an odd byte length.");
        var name = System.Text.Encoding.BigEndianUnicode.GetString(value);
        var version = name.IndexOf(';');
        if (version >= 0) name = name[..version];
        return name.EndsWith('.') ? name[..^1] : name;
    }
}
