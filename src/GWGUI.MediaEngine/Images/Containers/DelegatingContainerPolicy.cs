using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Containers;

internal sealed class DelegatingContainerPolicy(
    Func<string, CancellationToken, Task<SectorImage>> read,
    params string[] extensions) : IDiskImageContainerPolicy
{
    private readonly HashSet<string> supportedExtensions = new(extensions, StringComparer.OrdinalIgnoreCase);

    public ValueTask<bool> CanReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        ValueTask.FromResult(supportedExtensions.Contains(context.Extension));

    public Task<SectorImage> ReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        read(context.Path, cancellationToken);
}
