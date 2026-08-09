using GWGUI.Scp.FileSystems.Readers;
using GWGUI.Scp.Images.Interpretations;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Containers;

internal sealed class RawImgContainerPolicy : IDiskImageContainerPolicy
{
    public ValueTask<bool> CanReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        ValueTask.FromResult(context.Extension.Equals(".img", StringComparison.OrdinalIgnoreCase));

    public async Task<SectorImage> ReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken)
    {
        var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        var hasFatBpb = IbmPcImageReader.HasValidBpbGeometry(bytes);
        var image = IbmPcImageReader.Create(bytes, cancellationToken);
        if (!hasFatBpb && AmstradCpmFileSystemReader.LooksLikeCpcRawImage(bytes))
            return SectorImageInterpretation.Retag(image, "amstrad.cpc");
        if (!hasFatBpb && AmstradCpmFileSystemReader.LooksLikePcwDiskSpecification(bytes))
            return SectorImageInterpretation.Retag(image, "amstrad.pcw");
        return image;
    }
}
