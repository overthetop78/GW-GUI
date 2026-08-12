using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Ibm;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Builders;

namespace GWGUI.MediaEngine.Containers.Ibm.Raw;

/// <summary>Lit une image sectorielle brute IBM IMG ou IMA.</summary>
public sealed class IbmRawImageReader
{
    /// <summary>Lit le fichier, dÃ©tecte sa gÃ©omÃ©trie puis appelle le constructeur sectoriel IBM commun.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return IbmRawSectorImageBuilder.Create(data, IbmRawImageGeometryDetector.Detect(data), cancellationToken);
    }
}
