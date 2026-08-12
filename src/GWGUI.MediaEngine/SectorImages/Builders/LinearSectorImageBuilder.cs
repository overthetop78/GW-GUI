namespace GWGUI.MediaEngine.SectorImages.Builders;

/// <summary>Construit une image sectorielle dont les blocs suivent un ordre linéaire cylindre, face et secteur.</summary>
internal static class LinearSectorImageBuilder
{
    /// <summary>Valide la longueur puis construit chaque bloc avec l'adresse imposée par la géométrie.</summary>
    public static SectorImage Create(ReadOnlyMemory<byte> data, string formatId, LinearSectorImageGeometry geometry)
    {
        if (data.Length != geometry.Capacity) throw new InvalidDataException($"Linear image length {data.Length} does not match geometry capacity {geometry.Capacity} bytes.");
        var blocks = new SectorBlock[geometry.BlockCount];
        var sectorBase = geometry.Numbering == SectorNumbering.OneBased ? 1 : 0;
        for (var logical = 0; logical < blocks.Length; logical++)
        {
            var perCylinder = geometry.Heads * geometry.SectorsPerTrack;
            var address = new SectorAddress(logical / perCylinder, logical / geometry.SectorsPerTrack % geometry.Heads, logical % geometry.SectorsPerTrack + sectorBase);
            blocks[logical] = new(logical, address, data.Slice(logical * geometry.BlockSize, geometry.BlockSize).ToArray());
        }
        return new(formatId, geometry.BlockSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, blocks, capacity: data.Length, logicalBlockCount: blocks.Length);
    }
}
