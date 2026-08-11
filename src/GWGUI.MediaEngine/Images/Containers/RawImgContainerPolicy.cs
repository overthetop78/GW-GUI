using GWGUI.MediaEngine.FileSystems.Readers;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Images.Interpretations;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Containers;

internal sealed class RawImgContainerPolicy : IDiskImageRecognitionPolicy
{
    public ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken) =>
        ValueTask.FromResult(context.Extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase));

    public async Task<SectorImage> ReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken)
    {
        var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        var hasFatBpb = IbmPcImageReader.HasValidBpbGeometry(bytes.Span);
        var image = IbmPcImageReader.Create(bytes.Span, cancellationToken);
        if (!hasFatBpb && AmstradCpmFileSystemReader.LooksLikeCpcRawImage(bytes.ToArray()))
            return SectorImageInterpretation.Retag(image, DiskImageFormatIds.AmstradCpc);
        if (!hasFatBpb && AmstradCpmFileSystemReader.LooksLikePcwDiskSpecification(bytes.Span))
            return SectorImageInterpretation.Retag(image, DiskImageFormatIds.AmstradPcw);
        return image;
    }
}
