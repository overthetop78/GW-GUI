namespace GWGUI.Scp.Images;

internal static class DiskProtectionCatalog
{
    public static string? NameFor(IEnumerable<string> formatIds) =>
        formatIds.Any(id => id.Equals("apple2.rwts18", StringComparison.OrdinalIgnoreCase))
            ? "Brøderbund RWTS18"
            : null;
}
