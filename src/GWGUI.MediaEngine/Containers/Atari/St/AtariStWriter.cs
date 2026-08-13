using GWGUI.MediaEngine.Containers.Raw;
using GWGUI.MediaEngine.Geometries.Atari;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Atari.St;

/// <summary>Écrit une image sectorielle Atari ST brute dans l'ordre logique des secteurs.</summary>
public sealed class AtariStWriter(LinearSectorImageWriter writer)
{
    /// <summary>Écrit tous les blocs annoncés sans matérialiser silencieusement les secteurs absents.</summary>
    public async Task WriteAsync(SectorImage image, string path, CancellationToken cancellationToken = default)
    {
        if (!AtariStGeometry.TryFromFormatId(image.FormatId, out var geometry)) throw AtariStExceptions.UnsupportedSectorImage(image.FormatId, AtariStGeometry.SectorSize);
        var target = new RegularSectorGeometry(geometry.FormatId, AtariStGeometry.SectorSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, 1);
        await writer.WriteAsync(image, path, target, cancellationToken).ConfigureAwait(false);
    }
}
