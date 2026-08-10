using GWGUI.Scp.Recognition.Definitions;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Containers;

internal sealed class MsxContainerPolicy(MsxImageReader reader) : IDiskImageContainerPolicy
{
    public async ValueTask<bool> CanReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken)
    {
        if (!context.Extension.Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase)) return false;
        return context.FormatId?.StartsWith("msx.", StringComparison.OrdinalIgnoreCase) == true ||
               MsxImageReader.LooksLikeMsx(await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false));
    }

    public Task<SectorImage> ReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        reader.ReadAsync(context.Path, cancellationToken);
}
