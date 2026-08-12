namespace GWGUI.MediaEngine.FileSystems.Fat12;

/// <summary>Décode les noms 8.3, labels et champs Latin-1 des entrées FAT.</summary>
public static class FatDirectoryEntryReader
{
    /// <summary>Décode un nom FAT au format 8.3.</summary>
    public static string DecodeName(ReadOnlySpan<byte> value)
    {
        var name = DecodeFixed(value, 0, FatDirectoryLayout.NameLength).Trim();
        var extension = DecodeFixed(value, FatDirectoryLayout.ExtensionOffset, FatDirectoryLayout.ExtensionLength).Trim();
        return extension.Length == 0 ? name : name + FatDirectoryLayout.ExtensionSeparator + extension;
    }

    /// <summary>Lit le premier label actif du répertoire racine.</summary>
    public static string? ReadVolumeLabel(ReadOnlySpan<byte> root)
    {
        for (var offset = 0; offset + FatDirectoryLayout.EntrySize <= root.Length; offset += FatDirectoryLayout.EntrySize)
        {
            if (root[offset] is FatDirectoryLayout.EndMarker or FatDirectoryLayout.DeletedMarker) continue;
            var attributes = (FatDirectoryAttributes)root[offset + FatDirectoryLayout.AttributesOffset];
            if ((attributes & FatDirectoryLayout.LongFileName) == FatDirectoryLayout.LongFileName) continue;
            if (attributes.HasFlag(FatDirectoryAttributes.VolumeLabel)) return DecodeFixed(root, offset, FatDirectoryLayout.NameLength + FatDirectoryLayout.ExtensionLength).Trim();
        }
        return null;
    }

    /// <summary>Lit le label étendu du secteur d'amorçage et ignore « NO NAME ».</summary>
    public static string ReadBootVolumeLabel(IReadOnlyList<byte> boot)
    {
        if (boot.Count < FatBootSectorLayout.ExtendedBootMinimumLength) return string.Empty;
        var bytes = new byte[FatBootSectorLayout.VolumeLabelLength];
        for (var index = 0; index < bytes.Length; index++) bytes[index] = boot[FatBootSectorLayout.VolumeLabelOffset + index];
        if (bytes.Any(value => value is < FatBootSectorLayout.PrintableAsciiStart or > FatBootSectorLayout.PrintableAsciiEnd)) return string.Empty;
        var label = System.Text.Encoding.ASCII.GetString(bytes).Trim();
        return label.Equals(FatBootSectorLayout.EmptyVolumeLabel, StringComparison.OrdinalIgnoreCase) ? string.Empty : label;
    }

    /// <summary>Décode une chaîne Latin-1 bornée et retire les remplissages FAT.</summary>
    public static string DecodeFixed(ReadOnlySpan<byte> value, int offset, int length) => System.Text.Encoding.Latin1.GetString(value.Slice(offset, length)).TrimEnd(FatBootSectorLayout.NullPadding, FatBootSectorLayout.SpacePadding);
}
