using GWGUI.Domain.Formats;
using System.Text.RegularExpressions;

namespace GWGUI.Domain.Formats.Parsing;

public static partial class GwFormatCapabilitiesParser
{
    [GeneratedRegex(@"(?<![\w.])[a-z][a-z0-9_-]*(?:\.[a-z0-9_-]+)+(?![\w.])", RegexOptions.IgnoreCase)]
    private static partial Regex FormatIdRegex();

    [GeneratedRegex(@"(?<![\w.])\.[a-z0-9][a-z0-9_-]*(?![\w.])", RegexOptions.IgnoreCase)]
    private static partial Regex ExtensionRegex();

    public static GwFormatCapabilities ParseReadHelp(string? output)
    {
        if (string.IsNullOrWhiteSpace(output)) return GwFormatCapabilities.Unknown;
        var formatSection = Section(output, "FORMAT options:", "Supported file suffixes:");
        var suffixSection = Section(output, "Supported file suffixes:", null);
        return new GwFormatCapabilities(
            FormatIdRegex().Matches(formatSection).Select(x => x.Value.ToLowerInvariant()).ToHashSet(StringComparer.OrdinalIgnoreCase),
            ExtensionRegex().Matches(suffixSection).Select(x => x.Value.ToLowerInvariant()).ToHashSet(StringComparer.OrdinalIgnoreCase));
    }

    private static string Section(string text, string startMarker, string? endMarker)
    {
        var start = text.IndexOf(startMarker, StringComparison.OrdinalIgnoreCase);
        if (start < 0) return string.Empty;
        start += startMarker.Length;
        var end = endMarker is null ? text.Length : text.IndexOf(endMarker, start, StringComparison.OrdinalIgnoreCase);
        return text[start..(end < 0 ? text.Length : end)];
    }
}
