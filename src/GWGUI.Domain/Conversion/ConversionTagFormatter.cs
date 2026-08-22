using GWGUI.Domain.Formats;
namespace GWGUI.Domain.Conversion;

public static class ConversionTagFormatter
{
    private static readonly string[] SupportedTokens =
        ["{TAG}", "{NAME}", "{FAMILY}", "{FORMAT}", "{EXTENSION}", "{DATE:", "{TIME:"];

    public static string Format(string pattern, DiskFormat format, string extension, string sourceName, DateTime timestamp)
    {
        if (string.IsNullOrWhiteSpace(pattern) ||
            !SupportedTokens.Any(token => pattern.Contains(token, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("The tag pattern must contain a supported variable.", nameof(pattern));

        var legacyTag = format.Tag ?? format.Id.ToUpperInvariant().Replace('.', '-');
        var separator = legacyTag.IndexOf('-');
        var family = separator < 0 ? legacyTag : legacyTag[..separator];
        var diskFormat = separator < 0 ? format.Id.Split('.').Last().ToUpperInvariant() : legacyTag[(separator + 1)..];
        return pattern
            .Replace("{TAG}", legacyTag, StringComparison.OrdinalIgnoreCase)
            .Replace("{FAMILY}", family, StringComparison.OrdinalIgnoreCase)
            .Replace("{FORMAT}", diskFormat, StringComparison.OrdinalIgnoreCase)
            .Replace("{EXTENSION}", extension.TrimStart('.').ToUpperInvariant(), StringComparison.OrdinalIgnoreCase)
            .Replace("{NAME}", sourceName, StringComparison.OrdinalIgnoreCase)
            .Replace("{DATE:YYYY-MM-DD}", timestamp.ToString("yyyy-MM-dd"), StringComparison.OrdinalIgnoreCase)
            .Replace("{DATE:YYYYMMDD}", timestamp.ToString("yyyyMMdd"), StringComparison.OrdinalIgnoreCase)
            .Replace("{DATE:DD-MM-YYYY}", timestamp.ToString("dd-MM-yyyy"), StringComparison.OrdinalIgnoreCase)
            .Replace("{TIME:HH-MM-SS}", timestamp.ToString("HH-mm-ss"), StringComparison.OrdinalIgnoreCase)
            .Replace("{TIME:HHMMSS}", timestamp.ToString("HHmmss"), StringComparison.OrdinalIgnoreCase)
            .Replace("{TIME:HH-MM}", timestamp.ToString("HH-mm"), StringComparison.OrdinalIgnoreCase);
    }
}
