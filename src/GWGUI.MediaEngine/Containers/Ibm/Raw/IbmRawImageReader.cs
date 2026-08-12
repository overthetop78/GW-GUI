using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Ibm;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Builders;
using GWGUI.MediaEngine.SectorImages.Reading;

namespace GWGUI.MediaEngine.Containers.Ibm.Raw;

/// <summary>Lit une image sectorielle brute IBM IMG ou IMA.</summary>
public sealed class IbmRawImageReader : ISectorImageReader
{
    /// <summary>Indique si le chemin porte une extension IMG ou IMA ; cette valeur reste seulement indicative tant que le contrat commun l'impose.</summary>
    public bool CanRead(string path) => Path.GetExtension(path) is var extension && (extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase) || extension.Equals(DiskImageFileExtensions.Ima, StringComparison.OrdinalIgnoreCase));

    /// <summary>Lit le fichier, détecte sa géométrie puis appelle le constructeur sectoriel IBM commun.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return IbmRawSectorImageBuilder.Create(data, IbmRawImageGeometryDetector.Detect(data), cancellationToken);
    }
}
