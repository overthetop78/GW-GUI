using GWGUI.MediaEngine.Recognition.Definitions;

namespace GWGUI.MediaEngine.Images;

internal static class DiskProtectionCatalog
{
    public static string? NameFor(IEnumerable<string> formatIds) =>
        formatIds.Any(id => id.Equals(DiskImageFormatIds.AppleIIRwts18, StringComparison.OrdinalIgnoreCase))
            ? "Brøderbund RWTS18"
            : null;
}
