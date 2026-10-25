namespace GWGUI.MediaAudit;

internal static class MediaAuditTree
{
    internal static IEnumerable<T> Flatten<T>(IEnumerable<T> entries, Func<T, IEnumerable<T>> children)
    {
        foreach (var entry in entries)
        {
            yield return entry;
            foreach (var child in Flatten(children(entry), children)) yield return child;
        }
    }
}
