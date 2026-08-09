using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Containers;

internal interface IDiskImageContainerPolicy
{
    ValueTask<bool> CanReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken);
    Task<SectorImage> ReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken);
}
