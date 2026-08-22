using GWGUI.Domain.Formats;
namespace GWGUI.Domain.Conversion;

internal static class ConversionOutputFactory
{
    public static ConversionOutput Create(
        string destinationFolder,
        string outputBaseName,
        DiskFormat format,
        ImageExtension extension,
        string sourceExtension,
        bool addTags,
        string tagPattern,
        bool usesImplicitExtension)
    {
        var tag = addTags
            ? ConversionTagFormatter.Format(tagPattern, format, extension.Extension, outputBaseName, DateTime.Now)
            : "";
        var fileName = !addTags
            ? outputBaseName
            : tagPattern.Contains("{NAME}", StringComparison.OrdinalIgnoreCase) ? tag : tag + outputBaseName;
        return new ConversionOutput(
            format.Id,
            extension.Extension,
            Path.Combine(destinationFolder, fileName + extension.Extension),
            usesImplicitExtension,
            ConversionFidelity.ForConversion(sourceExtension, extension.Extension));
    }
}
