using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction;

/// <summary>Construit une image sectorielle brute à partir d'une géométrie régulière.</summary>
internal static class RegularSectorImageBuilder
{
    /// <summary>Découpe les données en blocs ordonnés par cylindre, face et secteur.</summary>
    public static SectorImage Create(ReadOnlySpan<byte> data, RegularSectorGeometry geometry, CancellationToken cancellationToken, int allowedTrailingByteCount = 0)
    {
        if (data.Length != geometry.Capacity + allowedTrailingByteCount) throw new InvalidDataException($"Raw image contains {data.Length} bytes; expected {geometry.Capacity + allowedTrailingByteCount} bytes.");
        var blocks = new SectorBlock[geometry.BlockCount];
        for (var logical = 0; logical < blocks.Length; logical++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var track = logical / geometry.SectorsPerTrack;
            blocks[logical] = new(logical, new(track / geometry.Heads, track % geometry.Heads, logical % geometry.SectorsPerTrack + geometry.FirstSectorNumber), data.Slice(logical * geometry.BlockSize, geometry.BlockSize).ToArray());
        }
        return new(geometry.FormatId, geometry.BlockSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, blocks, capacity: geometry.Capacity, logicalBlockCount: geometry.BlockCount);
    }
}
