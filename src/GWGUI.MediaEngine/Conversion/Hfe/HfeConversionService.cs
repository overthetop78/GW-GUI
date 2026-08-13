using GWGUI.MediaEngine.Containers.Hfe;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Exploration;

namespace GWGUI.MediaEngine.Conversion.Hfe;

/// <summary>Convertit une image sectorielle reconnue en pistes HFE uniformes.</summary>
public sealed class HfeConversionService(DiskImageExplorer explorer, SectorImageTrackEncoder encoder, HfeWriter writer)
{
    public static bool CanCreate(string formatId, string extension) => formatId.Equals(DiskImageFormatIds.RawHfe, StringComparison.OrdinalIgnoreCase) && extension.Equals(DiskImageFileExtensions.Hfe, StringComparison.OrdinalIgnoreCase);

    public async Task ConvertAsync(string sourcePath, string outputPath, CancellationToken cancellationToken = default)
    {
        if (Path.GetExtension(sourcePath).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase)) throw new NotSupportedException("La conservation directe SCP vers HFE sera raccordée avec les conversions flux vers flux.");
        var explored = await explorer.ExploreAsync(sourcePath, null, cancellationToken).ConfigureAwait(false);
        var tracks = encoder.Encode(explored.Image, cancellationToken);
        await writer.WriteAsync(tracks, outputPath, cancellationToken).ConfigureAwait(false);
    }
}
