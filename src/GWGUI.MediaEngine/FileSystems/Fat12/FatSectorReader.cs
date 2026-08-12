using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Fat12;

/// <summary>Lit positionnellement des secteurs FAT sans confondre absence et contenu nul.</summary>
internal static class FatSectorReader
{
    /// <summary>Réserve la taille logique de chaque secteur et signale les secteurs absents ou mal dimensionnés.</summary>
    public static FatSectorRange Read(SectorImage image, int firstSector, int count, List<string> warnings)
    {
        var output = new byte[checked(count * FatBootSectorLayout.SectorSize)];
        var present = new bool[count];
        for (var index = 0; index < count; index++)
        {
            var logicalSector = firstSector + index;
            if (!image.TryGetBlock(logicalSector, out var block))
            {
                warnings.Add(Fat12FileSystemExceptions.MissingSector(logicalSector, 0, FatBootSectorLayout.SectorSize));
                continue;
            }
            if (block.Data.Count != FatBootSectorLayout.SectorSize)
            {
                warnings.Add(Fat12FileSystemExceptions.MissingSector(logicalSector, block.Data.Count, FatBootSectorLayout.SectorSize));
                continue;
            }
            for (var byteIndex = 0; byteIndex < block.Data.Count; byteIndex++) output[index * FatBootSectorLayout.SectorSize + byteIndex] = block.Data[byteIndex];
            present[index] = true;
        }
        return new(output, present);
    }
}
