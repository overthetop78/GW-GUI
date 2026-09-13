using System.Globalization;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Formats.Floppy.DiskCopy;

/// <summary>Converts DiskCopy header data to and from common media document metadata.</summary>
internal static class DiskCopyMetadataFunctions
{
    public static IReadOnlyDictionary<string, string> Create(DiskCopyImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [DiskCopyMetadataKeys.NameBytes] = Convert.ToBase64String(image.NameBytes.ToArray()),
            [DiskCopyMetadataKeys.DiskFormat] = image.DiskFormat.ToString(CultureInfo.InvariantCulture),
            [DiskCopyMetadataKeys.FormatByte] = image.FormatByte.ToString(CultureInfo.InvariantCulture)
        };
        metadata[DiskCopyMetadataKeys.DataChecksumStatus] = ChecksumStatus(image.DataChecksumValid);
        metadata[DiskCopyMetadataKeys.TagChecksumStatus] = ChecksumStatus(image.TagChecksumValid);
        return metadata;
    }

    public static DiskCopyImage? Restore(
        SectorImage image,
        IReadOnlyDictionary<string, string> metadata)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(metadata);
        if (!metadata.ContainsKey(DiskCopyMetadataKeys.NameBytes) &&
            !metadata.ContainsKey(DiskCopyMetadataKeys.DiskFormat) &&
            !metadata.ContainsKey(DiskCopyMetadataKeys.FormatByte))
            return null;

        if (!metadata.TryGetValue(DiskCopyMetadataKeys.NameBytes, out var encodedName) ||
            !metadata.TryGetValue(DiskCopyMetadataKeys.DiskFormat, out var diskFormatText) ||
            !metadata.TryGetValue(DiskCopyMetadataKeys.FormatByte, out var formatByteText))
            throw new InvalidDataException("The DiskCopy metadata is incomplete.");

        byte[] nameBytes;
        try
        {
            nameBytes = Convert.FromBase64String(encodedName);
        }
        catch (FormatException exception)
        {
            throw new InvalidDataException("The DiskCopy name metadata is invalid.", exception);
        }

        if (nameBytes.Length > DiskCopyLayout.MaximumNameLength ||
            !byte.TryParse(diskFormatText, NumberStyles.None, CultureInfo.InvariantCulture, out var diskFormat) ||
            !byte.TryParse(formatByteText, NumberStyles.None, CultureInfo.InvariantCulture, out var formatByte))
            throw new InvalidDataException("The DiskCopy format metadata is invalid.");

        return new DiskCopyImage(image, nameBytes, diskFormat, formatByte,
            ParseChecksumStatus(metadata, DiskCopyMetadataKeys.DataChecksumStatus),
            ParseChecksumStatus(metadata, DiskCopyMetadataKeys.TagChecksumStatus));
    }

    private static string ChecksumStatus(bool? valid) => valid switch { true => "valid", false => "invalid", null => "missing" };

    private static bool? ParseChecksumStatus(IReadOnlyDictionary<string, string> metadata, string key)
    {
        if (!metadata.TryGetValue(key, out var value) || value == "missing") return null;
        if (value == "valid") return true;
        if (value == "invalid") return false;
        throw new InvalidDataException($"The DiskCopy checksum metadata '{key}' is invalid.");
    }
}
