using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Recognition;

namespace GWGUI.MediaEngine.Images.Containers;

internal interface IDiskImageContainerPolicy
{
    ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken);
    Task<SectorImage> ReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken);
}
