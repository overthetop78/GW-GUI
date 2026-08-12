using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Builders;

namespace GWGUI.MediaEngine.Reconstruction;

/// <summary>Construit une image sectorielle brute à partir d'une géométrie régulière.</summary>
internal static class RegularSectorImageBuilder
{
    /// <summary>Découpe les données en blocs ordonnés par cylindre, face et secteur.</summary>
    public static SectorImage Create(ReadOnlySpan<byte> data, RegularSectorGeometry geometry, CancellationToken cancellationToken, int allowedTrailingByteCount = 0)
    {
        if (data.Length != geometry.Capacity + allowedTrailingByteCount) throw new InvalidDataException($"Raw image contains {data.Length} bytes; expected {geometry.Capacity + allowedTrailingByteCount} bytes.");
        var numbering = geometry.FirstSectorNumber == 0 ? SectorNumbering.ZeroBased : SectorNumbering.OneBased;
        var linearGeometry = new LinearSectorImageGeometry(geometry.BlockSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, numbering);
        return LinearSectorImageBuilder.Create(data[..geometry.Capacity].ToArray(), geometry.FormatId, linearGeometry, cancellationToken);
    }
}
