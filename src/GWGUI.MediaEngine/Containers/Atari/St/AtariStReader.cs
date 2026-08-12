using GWGUI.MediaEngine.Geometries.Atari;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Builders;

namespace GWGUI.MediaEngine.Containers.Atari.St;

/// <summary>Lit une image Atari ST brute et la construit avec des secteurs numérotés à partir de un.</summary>
public sealed class AtariStReader
{
    /// <summary>Charge, détecte et valide exactement la géométrie de l'image.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (data.Length == 0 || data.Length % AtariStGeometry.SectorSize != 0) throw AtariStExceptions.InvalidLength(data.Length, AtariStGeometry.SectorSize);
        var detection = AtariStGeometryDetector.Detect(data);
        var geometry = detection.Geometry;
        if (data.Length != geometry.Capacity) throw AtariStExceptions.IncompatibleGeometry(data.Length, geometry.Capacity, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack);
        var linear = new LinearSectorImageGeometry(AtariStGeometry.SectorSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, SectorNumbering.OneBased);
        return LinearSectorImageBuilder.Create(data, geometry.FormatId, linear, cancellationToken);
    }
}
