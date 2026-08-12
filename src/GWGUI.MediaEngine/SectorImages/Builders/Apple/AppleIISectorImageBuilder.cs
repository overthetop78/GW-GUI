using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Conversion.Apple;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Recognition.Apple;

namespace GWGUI.MediaEngine.SectorImages.Builders.Apple;

/// <summary>Construit une image sectorielle Apple II depuis des pistes GCR dÃ©jÃ  dÃ©codÃ©es.</summary>
internal static class AppleIISectorImageBuilder
{
    /// <summary>SÃ©lectionne les secteurs valides, construit les blocs DOS et ProDOS, puis identifie leur organisation.</summary>
    public static SectorImage Create(IEnumerable<(int Track, IReadOnlyList<DecodedSector> Sectors)> decodedTracks)
    {
        var selected = DecodedAppleSectorSelection.Best(decodedTracks, sector => sector.Data is { Count: AppleIIGeometry.SectorSize } && sector.Number is >= 0 and < AppleIIGeometry.SectorsPerTrack);
        var trackCount = Math.Max(AppleIIGeometry.TrackCount, selected.Count == 0 ? AppleIIGeometry.TrackCount : selected.Keys.Max(key => key.Track) + 1);
        var sectorsPerTrack = selected.Count > 0 && selected.Keys.Max(key => key.Sector) < 13 ? 13 : AppleIIGeometry.SectorsPerTrack;
        var dosBlocks = CreateDosBlocks(selected, sectorsPerTrack);
        if (dosBlocks.Length == 0) return new(DiskImageFormatIds.AppleIIGcr, AppleIIGeometry.SectorSize, trackCount, DiskGeometryConstants.SingleSidedHeadCount, AppleIIGeometry.SectorsPerTrack, []);
        if (sectorsPerTrack == 13) return new(DiskImageFormatIds.AppleIIDos32, AppleIIGeometry.SectorSize, trackCount, DiskGeometryConstants.SingleSidedHeadCount, sectorsPerTrack, dosBlocks);
        var proDosBlocks = CreateProDosBlocks(selected, trackCount);
        var proDosProbe = ToDense(proDosBlocks, trackCount * AppleIIGeometry.ProDosBlocksPerTrack, MacintoshGcrGeometry.BlockSize);
        if (!proDosProbe.MissingBlocks.Any() && AppleRawImageProbe.LooksLikeProDos(proDosProbe.Data)) return new(DiskImageFormatIds.AppleIIProDos, MacintoshGcrGeometry.BlockSize, trackCount, DiskGeometryConstants.SingleSidedHeadCount, AppleIIGeometry.ProDosBlocksPerTrack, proDosBlocks);
        var dosProbe = ToDense(dosBlocks, trackCount * AppleIIGeometry.SectorsPerTrack, AppleIIGeometry.SectorSize);
        return new(!dosProbe.MissingBlocks.Any() && AppleRawImageProbe.LooksLikeDos33(dosProbe.Data) ? DiskImageFormatIds.AppleIIDos33 : DiskImageFormatIds.AppleIIGcr, AppleIIGeometry.SectorSize, trackCount, DiskGeometryConstants.SingleSidedHeadCount, sectorsPerTrack, dosBlocks);
    }

    /// <summary>Construit les blocs dans l'ordre d'un fichier DOS Apple II.</summary>
    private static SectorBlock[] CreateDosBlocks(IReadOnlyDictionary<(int Track, int Sector), DecodedSector> selected, int sectorsPerTrack) => selected.Where(pair => pair.Key.Sector < sectorsPerTrack).Select(pair => new SectorBlock(pair.Key.Track * sectorsPerTrack + (sectorsPerTrack == AppleIIGeometry.SectorsPerTrack ? AppleIISectorOrderConverter.PhysicalToDosFileSector(pair.Key.Sector) : pair.Key.Sector), new(pair.Key.Track, 0, pair.Key.Sector), pair.Value.Data!.ToArray(), pair.Value.IntegrityValid)).ToArray();

    /// <summary>Assemble deux secteurs physiques de 256 octets dans chaque bloc logique ProDOS de 512 octets.</summary>
    private static SectorBlock[] CreateProDosBlocks(IReadOnlyDictionary<(int Track, int Sector), DecodedSector> selected, int trackCount)
    {
        var blocks = new List<SectorBlock>();
        for (var track = 0; track < trackCount; track++)
        {
            for (var block = 0; block < 8; block++)
            {
                var first = AppleIISectorOrderConverter.ProDosToPhysicalSector(block * 2);
                var second = AppleIISectorOrderConverter.ProDosToPhysicalSector((block * 2) + 1);
                if (!selected.TryGetValue((track, first), out var low) || !selected.TryGetValue((track, second), out var high)) continue;
                blocks.Add(new(track * 8 + block, new(track, 0, block), low.Data!.Concat(high.Data!).ToArray(), low.IntegrityValid == true && high.IntegrityValid == true));
            }
        }
        return blocks.ToArray();
    }

    /// <summary>Place les blocs disponibles Ã  leur position logique dans un tampon dense utilisÃ© uniquement par les sondes.</summary>
    internal static DenseAppleSectorImage ToDense(IEnumerable<SectorBlock> blocks, int count, int blockSize)
    {
        var data = new byte[count * blockSize];
        var present = new bool[count];
        foreach (var block in blocks)
        {
            if (block.LogicalBlock < 0 || block.LogicalBlock >= count) throw SectorImageBuilderExceptions.InvalidLogicalBlock(nameof(AppleIISectorImageBuilder), block.LogicalBlock, count);
            block.Data.ToArray().CopyTo(data, block.LogicalBlock * blockSize);
            present[block.LogicalBlock] = true;
        }
        return new(data, Enumerable.Range(0, count).Where(index => !present[index]).ToArray());
    }
}

/// <summary>Contient le tampon dense utilisÃ© par les sondes et les positions restÃ©es absentes.</summary>
internal sealed record DenseAppleSectorImage(byte[] Data, IReadOnlyList<int> MissingBlocks);
