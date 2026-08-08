namespace GWGUI.Domain.Formats;

public static class GwFormatArgument
{
    public static string? FromCatalogId(string? formatId) =>
        string.IsNullOrWhiteSpace(formatId) || formatId.Equals("raw.scp", StringComparison.OrdinalIgnoreCase) || formatId.Equals("raw.hfe", StringComparison.OrdinalIgnoreCase)
            ? null
            : formatId.Equals("atarist.1440", StringComparison.OrdinalIgnoreCase)
                ? "ibm.1440"
                : formatId.Equals("mac.1440", StringComparison.OrdinalIgnoreCase)
                    ? "ibm.1440"
                : formatId.Equals("apple3.sos", StringComparison.OrdinalIgnoreCase)
                    ? "apple2.prodos.140"
                : formatId.Equals("apple2.prodos.800", StringComparison.OrdinalIgnoreCase)
                    ? "mac.800"
                : formatId;
}
