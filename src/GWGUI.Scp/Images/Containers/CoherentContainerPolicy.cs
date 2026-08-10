using GWGUI.Scp.Recognition.Definitions;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Containers;

internal sealed class CoherentContainerPolicy(CoherentImageReader reader) : IDiskImageContainerPolicy
{
    public async ValueTask<bool> CanReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        context.Extension.Equals(DiskImageFileExtensions.Bin, StringComparison.OrdinalIgnoreCase) &&
        CoherentImageReader.LooksLikeCoherent(await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false));

    public Task<SectorImage> ReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        reader.ReadAsync(context.Path, cancellationToken);
}
