namespace GWGUI.Domain.Formats;

public static class GwFormatArgument
{
    public static string? FromCatalogId(string? formatId) =>
        string.IsNullOrWhiteSpace(formatId) || formatId.StartsWith("raw.", StringComparison.OrdinalIgnoreCase) ? null : formatId;
}
