using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Containers;

internal sealed class AmstradContainerPolicy(AmstradDskImageReader reader) : IDiskImageContainerPolicy
{
    public ValueTask<bool> CanReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        ValueTask.FromResult(context.Extension.Equals(".dsk", StringComparison.OrdinalIgnoreCase) ||
                             context.Extension.Equals(".edsk", StringComparison.OrdinalIgnoreCase));

    public Task<SectorImage> ReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        reader.ReadAsync(context.Path, cancellationToken);
}
