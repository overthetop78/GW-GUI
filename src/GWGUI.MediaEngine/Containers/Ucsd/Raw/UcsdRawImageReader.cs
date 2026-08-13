using GWGUI.MediaEngine.Geometries.Ucsd;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Builders;

namespace GWGUI.MediaEngine.Containers.Ucsd.Raw;

/// <summary>Lit une image UCSD p-System IBM MFM brute de 160 Kio.</summary>
public sealed class UcsdRawImageReader
{
    /// <summary>Lit et valide l'image brute avant de construire ses secteurs logiques.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        var geometry = UcsdIbmMfmGeometry.SectorGeometry;
        var linear = new LinearSectorImageGeometry(geometry.BlockSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, SectorNumbering.OneBased);
        return LinearSectorImageBuilder.Create(data, geometry.FormatId, linear, cancellationToken);
    }
}
