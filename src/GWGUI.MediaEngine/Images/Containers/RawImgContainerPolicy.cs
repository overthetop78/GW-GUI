using GWGUI.MediaEngine.FileSystems.Readers;
using GWGUI.MediaEngine.Images.Interpretations;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Containers;

internal sealed class RawImgContainerPolicy : IDiskImageContainerPolicy
{
    public ValueTask<bool> CanReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        ValueTask.FromResult(context.Extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase));

    public async Task<SectorImage> ReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken)
    {
        var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        var hasFatBpb = IbmPcImageReader.HasValidBpbGeometry(bytes);
        var image = IbmPcImageReader.Create(bytes, cancellationToken);
        if (!hasFatBpb && AmstradCpmFileSystemReader.LooksLikeCpcRawImage(bytes))
            return SectorImageInterpretation.Retag(image, DiskImageFormatIds.AmstradCpc);
        if (!hasFatBpb && AmstradCpmFileSystemReader.LooksLikePcwDiskSpecification(bytes))
            return SectorImageInterpretation.Retag(image, DiskImageFormatIds.AmstradPcw);
        return image;
    }
}
