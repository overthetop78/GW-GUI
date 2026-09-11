using System.Globalization;

namespace GWGUI.MediaEngine.Formats.Floppy.Hfe;

/// <summary>Creates the HFE metadata shared by media documents.</summary>
internal static class HfeMetadataFunctions
{
    public static IReadOnlyDictionary<string, string> Create(HfeImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [HfeMetadataKeys.Revision] = image.Revision.ToString(CultureInfo.InvariantCulture),
            [HfeMetadataKeys.Encoding] = image.Encoding.ToString(CultureInfo.InvariantCulture),
            [HfeMetadataKeys.BitRate] = image.BitRate.ToString(CultureInfo.InvariantCulture)
        };
    }
}
