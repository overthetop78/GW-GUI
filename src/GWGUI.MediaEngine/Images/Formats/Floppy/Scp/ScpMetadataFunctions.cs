using System.Globalization;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Scp;

/// <summary>Creates the SCP metadata shared by media documents.</summary>
internal static class ScpMetadataFunctions
{
    public static IReadOnlyDictionary<string, string> Create(ScpImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ScpMetadataKeys.Version] = image.Header.Version.ToString(CultureInfo.InvariantCulture),
            [ScpMetadataKeys.VersionText] = image.Header.VersionText,
            [ScpMetadataKeys.DiskType] = image.Header.DiskType.ToString(CultureInfo.InvariantCulture),
            [ScpMetadataKeys.Flags] = ((byte)image.Header.Flags).ToString(CultureInfo.InvariantCulture),
            [ScpMetadataKeys.BitCellEncoding] = ((byte)image.Header.BitCellEncoding).ToString(CultureInfo.InvariantCulture),
            [ScpMetadataKeys.Heads] = ((byte)image.Header.Heads).ToString(CultureInfo.InvariantCulture),
            [ScpMetadataKeys.Resolution] = image.Header.Resolution.ToString(CultureInfo.InvariantCulture),
            [ScpMetadataKeys.ResolutionNanoseconds] = image.Header.ResolutionNanoseconds.ToString(CultureInfo.InvariantCulture),
            [ScpMetadataKeys.ChecksumValid] = image.ChecksumValid.ToString(CultureInfo.InvariantCulture)
        };
    }
}
