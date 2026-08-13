namespace GWGUI.Domain.Parity;

public sealed class MediaParityMatrix(IEnumerable<MediaParityRow> rows)
{
    private readonly IReadOnlyList<MediaParityRow> _rows = rows.ToArray();

    public IReadOnlyList<MediaParityRow> Rows => _rows;

    public MediaParityRow? Find(string sourceContainer, string formatId, string targetContainer)
    {
        var source = Normalize(sourceContainer);
        var target = Normalize(targetContainer);
        return _rows.FirstOrDefault(row =>
            row.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase) &&
            row.SourceContainer.Equals(source, StringComparison.OrdinalIgnoreCase) &&
            row.TargetContainer.Equals(target, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsValidated(
        string sourceContainer,
        string formatId,
        string targetContainer,
        MediaParityOperation operation) =>
        Find(sourceContainer, formatId, targetContainer)?.IsValidatedFor(operation) == true;

    private static string Normalize(string extension) => extension.StartsWith('.')
        ? extension.ToLowerInvariant()
        : "." + extension.ToLowerInvariant();
}
