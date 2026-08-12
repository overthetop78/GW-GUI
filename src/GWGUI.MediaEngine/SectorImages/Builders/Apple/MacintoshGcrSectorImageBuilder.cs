using GWGUI.MediaEngine.Geometries.Apple;

namespace GWGUI.MediaEngine.SectorImages.Builders.Apple;

/// <summary>Construit une image Macintosh GCR en appliquant l'adressage zoné de sa géométrie validée.</summary>
internal static class MacintoshGcrSectorImageBuilder
{
    /// <summary>Valide la capacité puis construit les blocs zonés sans recalculer la géométrie dans le Reader.</summary>
    public static SectorImage Create(ReadOnlyMemory<byte> data, string formatId, MacintoshGcrImageGeometry geometry)
    {
        if (data.Length != geometry.Capacity) throw new InvalidDataException($"Macintosh GCR image length {data.Length} does not match geometry capacity {geometry.Capacity} bytes.");
        var blocks = new SectorBlock[geometry.BlockCount];
        for (var logical = 0; logical < blocks.Length; logical++) blocks[logical] = new(logical, MacintoshGcrGeometry.Address(logical, geometry.Heads), data.Slice(logical * MacintoshGcrGeometry.BlockSize, MacintoshGcrGeometry.BlockSize).ToArray());
        return new(formatId, MacintoshGcrGeometry.BlockSize, MacintoshGcrGeometry.CylinderCount, geometry.Heads, MacintoshGcrGeometry.MaximumSectorsPerTrack, blocks, capacity: geometry.Capacity, logicalBlockCount: geometry.BlockCount);
    }
}
