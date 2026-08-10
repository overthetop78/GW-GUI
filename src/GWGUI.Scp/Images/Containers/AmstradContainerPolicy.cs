using GWGUI.Scp.Containers.Amstrad.CpcDsk;
using GWGUI.Scp.Images.Interpretations;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Containers;

internal sealed class AmstradContainerPolicy(CpcDskReader reader) : IDiskImageContainerPolicy
{
    public ValueTask<bool> CanReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        ValueTask.FromResult(context.Extension.Equals(".dsk", StringComparison.OrdinalIgnoreCase) ||
                             context.Extension.Equals(".edsk", StringComparison.OrdinalIgnoreCase));

    public async Task<SectorImage> ReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken)
    {
        var image = await reader.ReadAsync(context.Path, cancellationToken).ConfigureAwait(false);
        var formatId = image.Cylinders >= 80 && image.Heads == 2 ? "amstrad.pcw" : "amstrad.cpc";
        return SectorImageInterpretation.Retag(image, formatId);
    }
}
