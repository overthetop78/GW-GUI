using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Recognition.Msx;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Reading;
using GWGUI.MediaEngine.SectorImages.Builders;

namespace GWGUI.MediaEngine.Images;

/// <summary>Lit et valide les images sectorielles brutes MSX-DOS.</summary>
public sealed class MsxImageReader : ISectorImageReader
{
    /// <summary>Indique si le chemin porte l'extension DSK utilisée comme indice des images brutes MSX.</summary>
    /// <param name="path">Chemin à examiner.</param>
    /// <returns><see langword="true"/> lorsque le chemin porte l'extension DSK.</returns>
    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase);

    /// <summary>Lit le contenu, valide de nouveau son BPB MSX puis construit son image sectorielle.</summary>
    /// <param name="path">Chemin de l'image brute MSX-DOS.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Image sectorielle MSX-DOS validée.</returns>
    /// <exception cref="InvalidDataException">Le BPB n'est pas MSX-DOS ou la géométrie n'est pas prise en charge.</exception>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (!MsxBootSectorProbe.LooksLikeMsx(data)) throw new InvalidDataException("The image does not contain an MSX-DOS boot sector.");
        var (format, cylinders, heads, sectors) = data.Length switch
        {
            184_320 => (DiskImageFormatIds.Msx1D, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.SingleSidedHeadCount, 9),
            368_640 when data[21] == 0xf8 => (DiskImageFormatIds.Msx1Dd, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.SingleSidedHeadCount, 9),
            368_640 => (DiskImageFormatIds.Msx2D, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 9),
            737_280 => (DiskImageFormatIds.Msx2Dd, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 9),
            _ => throw new InvalidDataException("The MSX disk geometry is not supported.")
        };
        var geometry = new LinearSectorImageGeometry(512, cylinders, heads, sectors, SectorNumbering.OneBased);
        return LinearSectorImageBuilder.Create(data, format, geometry);
    }
}
