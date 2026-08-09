using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Containers;

public sealed class DiskImageContainerRegistry
{
    private readonly IReadOnlyList<IDiskImageContainerPolicy> policies;

    internal DiskImageContainerRegistry(IReadOnlyList<IDiskImageContainerPolicy> policies) =>
        this.policies = policies;

    public async Task<SectorImage> ReadAsync(
        string path,
        string? formatId,
        CancellationToken cancellationToken)
    {
        var context = new DiskImageContainerContext(path, formatId);
        foreach (var policy in policies)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!await policy.CanReadAsync(context, cancellationToken).ConfigureAwait(false)) continue;
            return await policy.ReadAsync(context, cancellationToken).ConfigureAwait(false);
        }
        throw new NotSupportedException($"The image extension '{context.Extension}' is not supported by the explorer yet.");
    }
}
