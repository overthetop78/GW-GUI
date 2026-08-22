namespace GWGUI.Domain.Formats;

public sealed record GwFormatCapabilities(
    IReadOnlySet<string> FormatIds,
    IReadOnlySet<string> ImageExtensions)
{
    public static GwFormatCapabilities Unknown { get; } = new(
        new HashSet<string>(StringComparer.OrdinalIgnoreCase),
        new HashSet<string>(StringComparer.OrdinalIgnoreCase));

    public bool IsKnown => FormatIds.Count > 0 || ImageExtensions.Count > 0;
}
