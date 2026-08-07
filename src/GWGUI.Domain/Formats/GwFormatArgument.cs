namespace GWGUI.Domain.Formats;

public static class GwFormatArgument
{
    public static string? FromCatalogId(string? formatId) =>
        string.IsNullOrWhiteSpace(formatId) || formatId.Equals("raw.scp", StringComparison.OrdinalIgnoreCase) || formatId.Equals("raw.hfe", StringComparison.OrdinalIgnoreCase)
            ? null
            : formatId.Equals("atarist.1440", StringComparison.OrdinalIgnoreCase)
                ? "ibm.1440"
                : formatId;
}
