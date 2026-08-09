using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Containers;

internal sealed class AppleContainerPolicy(AppleDiskImageReader reader) : IDiskImageContainerPolicy
{
    private static readonly HashSet<string> AppleExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".do", ".po", ".2mg", ".image", ".d13", ".dc42", ".nib", ".woz" };

    public ValueTask<bool> CanReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken)
    {
        if (AppleExtensions.Contains(context.Extension)) return ValueTask.FromResult(true);
        if (context.Extension.Equals(".dsk", StringComparison.OrdinalIgnoreCase))
            return ValueTask.FromResult(context.FormatId?.StartsWith("apple", StringComparison.OrdinalIgnoreCase) == true ||
                                        AppleDiskImageReader.LooksLikeAppleImage(context.Path));
        if (context.Extension.Equals(".img", StringComparison.OrdinalIgnoreCase))
            return ValueTask.FromResult(context.FormatId?.StartsWith("mac.", StringComparison.OrdinalIgnoreCase) == true ||
                                        AppleDiskImageReader.LooksLikeAppleImage(context.Path));
        return ValueTask.FromResult(false);
    }

    public Task<SectorImage> ReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        reader.ReadAsync(context.Path, cancellationToken);
}
