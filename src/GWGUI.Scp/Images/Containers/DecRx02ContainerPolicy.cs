using GWGUI.Scp.Recognition.Definitions;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Containers;

internal sealed class DecRx02ContainerPolicy(DecRx02ImageReader reader) : IDiskImageContainerPolicy
{
    public async ValueTask<bool> CanReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken)
    {
        if (!context.Extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase)) return false;
        return context.FormatId?.Equals("dec.rx02", StringComparison.OrdinalIgnoreCase) == true ||
               DecRx02ImageReader.LooksLikeRt11(await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false));
    }

    public Task<SectorImage> ReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        reader.ReadAsync(context.Path, cancellationToken);
}
