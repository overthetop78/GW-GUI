using GWGUI.MediaEngine.Containers.Acorn.BbcDfs;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Acorn;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Conversion.Acorn;

/// <summary>Convertit une capture SCP ou une image BBC DFS en SSD ou DSD.</summary>
public sealed class BbcDfsConversionService(IsoScpSectorImageReader scpReader, BbcDfsReader reader, BbcDfsImageWriter writer)
{
    /// <summary>Indique si le format et l'extension décrivent le même conteneur BBC DFS.</summary>
    public static bool CanCreate(string formatId, string extension)
    {
        var geometry = BbcDfsGeometry.FindByFormatId(formatId);
        return geometry is not null && extension.Equals(geometry.Heads == 1 ? DiskImageFileExtensions.Ssd : DiskImageFileExtensions.Dsd, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Reconstruit ou relit la source puis écrit le conteneur BBC DFS demandé.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, string formatId, CancellationToken cancellationToken = default)
    {
        var image = Path.GetExtension(sourcePath).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase) ? await scpReader.ReadAsync(sourcePath, formatId, cancellationToken).ConfigureAwait(false) : await reader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        await writer.WriteAsync(image, outputPath, formatId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Écrit une image sectorielle BBC DFS déjà reconstruite.</summary>
    public Task ConvertAsync(SectorImage image, string outputPath, string formatId, CancellationToken cancellationToken = default) => writer.WriteAsync(image, outputPath, formatId, cancellationToken);
}
