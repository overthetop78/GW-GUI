using GWGUI.MediaEngine.Containers.Atari.St;
using GWGUI.MediaEngine.Geometries.Atari;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Conversion.Atari;

/// <summary>Valide et transforme sans perte une image sectorielle vers une géométrie Atari ST cataloguée.</summary>
internal static class AtariStGeometryTransformer
{
    /// <summary>Reconstruit les adresses de la géométrie cible en conservant exactement tous les secteurs logiques.</summary>
    public static SectorImage Transform(SectorImage source, string targetFormatId, bool completeMissingSectors = false)
    {
        if (!AtariStGeometry.TryFromFormatId(targetFormatId, out var target)) throw AtariStConversionExceptions.UnsupportedTargetFormat(targetFormatId);
        if (source.BlockSize != AtariStGeometry.SectorSize || source.Capacity != target.Capacity || source.BlockCount != target.Capacity / AtariStGeometry.SectorSize) throw AtariStConversionExceptions.LossyGeometryChange(source, target);
        var blocks = new List<SectorBlock>(source.BlockCount);
        for (var logical = 0; logical < source.BlockCount; logical++)
        {
            if (!source.TryGetBlock(logical, out var block))
            {
                if (!completeMissingSectors) throw AtariStConversionExceptions.MissingSourceSector(logical);
                block = new(logical, new(0, 0, 0), new byte[AtariStGeometry.SectorSize]);
            }
            if (block.Data.Count != AtariStGeometry.SectorSize) throw AtariStExceptions.InvalidLogicalSectorSize(logical, block.Data.Count, AtariStGeometry.SectorSize);
            var cylinder = logical / (target.Heads * target.SectorsPerTrack);
            var withinCylinder = logical % (target.Heads * target.SectorsPerTrack);
            var head = withinCylinder / target.SectorsPerTrack;
            var sector = withinCylinder % target.SectorsPerTrack + 1;
            blocks.Add(block with { Address = new(cylinder, head, sector) });
        }
        return new(target.FormatId, AtariStGeometry.SectorSize, target.Cylinders, target.Heads, target.SectorsPerTrack, blocks);
    }
}
