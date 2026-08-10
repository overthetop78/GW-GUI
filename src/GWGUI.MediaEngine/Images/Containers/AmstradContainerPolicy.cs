using GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;
using GWGUI.MediaEngine.Images.Interpretations;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Containers;

internal sealed class AmstradContainerPolicy(CpcDskReader reader) : IDiskImageContainerPolicy
{
    public ValueTask<bool> CanReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        ValueTask.FromResult(context.Extension.Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase) ||
                             context.Extension.Equals(DiskImageFileExtensions.Edsk, StringComparison.OrdinalIgnoreCase));

    public async Task<SectorImage> ReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken)
    {
        var image = await reader.ReadAsync(context.Path, cancellationToken).ConfigureAwait(false);
        var formatId = image.Cylinders >= 80 && image.Heads == 2
            ? DiskImageFormatIds.AmstradPcw
            : DiskImageFormatIds.AmstradCpc;
        return SectorImageInterpretation.Retag(image, formatId);
    }
}
