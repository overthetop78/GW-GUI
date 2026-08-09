using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Containers;

internal sealed class ScpContainerPolicy(
    ScpImageExplorationService exploration,
    IReadOnlySet<string> supportedFormatIds) : IDiskImageContainerPolicy
{
    public ValueTask<bool> CanReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        ValueTask.FromResult(context.Extension.Equals(".scp", StringComparison.OrdinalIgnoreCase));

    public Task<SectorImage> ReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken)
    {
        if (context.FormatId is not null && !supportedFormatIds.Contains(context.FormatId))
            throw new NotSupportedException($"The selected format '{context.FormatId}' is not supported by the explorer yet.");
        return exploration.ReadAsync(context.Path, context.FormatId, cancellationToken);
    }
}
