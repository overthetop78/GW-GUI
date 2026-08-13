using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Conversion.Fat12;

public sealed class Fat12ReinterpretationService(
    DiskImageExplorer explorer,
    Fat12TargetImageWriter writer)
{
    public async Task ConvertAsync(string sourcePath, string outputPath, string targetFormatId, CancellationToken cancellationToken = default)
    {
        var explored = await explorer.ExploreAsync(sourcePath, cancellationToken: cancellationToken).ConfigureAwait(false);
        var sourceIsHybrid = explored.DetectedImageFormatIds.Distinct(StringComparer.OrdinalIgnoreCase).Skip(1).Any() || explored.DetectedFileSystems.Select(item => item.ReaderId).Distinct(StringComparer.OrdinalIgnoreCase).Skip(1).Any();
        await ConvertAsync(explored.Image, outputPath, targetFormatId, sourceIsHybrid, cancellationToken).ConfigureAwait(false);
    }

    public async Task ConvertAsync(SectorImage source, string outputPath, string targetFormatId, bool sourceIsHybrid = false, CancellationToken cancellationToken = default)
    {
        _ = Fat12ReinterpretationPolicy.Validate(source, targetFormatId, sourceIsHybrid);
        await writer.WriteAsync(source, outputPath, targetFormatId, cancellationToken).ConfigureAwait(false);
    }
}
