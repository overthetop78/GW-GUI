using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Recognition.Apple;

namespace GWGUI.MediaEngine.SectorImages.Builders.Apple;

/// <summary>Construit une image sectorielle Apple II depuis des pistes GCR déjà décodées.</summary>
internal static class AppleIISectorImageBuilder
{
    /// <summary>Sélectionne les secteurs valides, construit les blocs DOS et ProDOS, puis identifie leur organisation.</summary>
    public static SectorImage Create(IEnumerable<(int Track, IReadOnlyList<DecodedSector> Sectors)> decodedTracks)
    {
        var selected = DecodedAppleSectorSelection.Best(decodedTracks, sector => sector.Data is { Count: AppleIIGeometry.SectorSize } && sector.Number is >= 0 and < AppleIIGeometry.SectorsPerTrack);
        var trackCount = Math.Max(AppleIIGeometry.TrackCount, selected.Count == 0 ? AppleIIGeometry.TrackCount : selected.Keys.Max(key => key.Track) + 1);
        var sectorsPerTrack = selected.Count > 0 && selected.Keys.Max(key => key.Sector) < 13 ? 13 : AppleIIGeometry.SectorsPerTrack;
        var dosBlocks = CreateDosBlocks(selected, sectorsPerTrack);
        if (dosBlocks.Length == 0) return new(DiskImageFormatIds.AppleIIGcr, AppleIIGeometry.SectorSize, trackCount, DiskGeometryConstants.SingleSidedHeadCount, AppleIIGeometry.SectorsPerTrack, []);
        if (sectorsPerTrack == 13) return new(DiskImageFormatIds.AppleIIDos32, AppleIIGeometry.SectorSize, trackCount, DiskGeometryConstants.SingleSidedHeadCount, sectorsPerTrack, dosBlocks);
        var proDosBlocks = CreateProDosBlocks(selected, trackCount);
        var proDosProbe = ToDense(proDosBlocks, trackCount * 8, MacintoshGcrGeometry.BlockSize);
        if (AppleRawImageProbe.LooksLikeProDos(proDosProbe)) return new(DiskImageFormatIds.AppleIIProDos, MacintoshGcrGeometry.BlockSize, trackCount, DiskGeometryConstants.SingleSidedHeadCount, 8, proDosBlocks);
        return new(AppleRawImageProbe.LooksLikeDos33(ToDense(dosBlocks, trackCount * AppleIIGeometry.SectorsPerTrack, AppleIIGeometry.SectorSize)) ? DiskImageFormatIds.AppleIIDos33 : DiskImageFormatIds.AppleIIGcr, AppleIIGeometry.SectorSize, trackCount, DiskGeometryConstants.SingleSidedHeadCount, sectorsPerTrack, dosBlocks);
    }

    /// <summary>Construit les blocs dans l'ordre d'un fichier DOS Apple II.</summary>
    private static SectorBlock[] CreateDosBlocks(IReadOnlyDictionary<(int Track, int Sector), DecodedSector> selected, int sectorsPerTrack) => selected.Where(pair => pair.Key.Sector < sectorsPerTrack).Select(pair => new SectorBlock(pair.Key.Track * sectorsPerTrack + (sectorsPerTrack == AppleIIGeometry.SectorsPerTrack ? AppleIIGeometry.PhysicalToDos[pair.Key.Sector] : pair.Key.Sector), new(pair.Key.Track, 0, pair.Key.Sector), pair.Value.Data!.ToArray(), pair.Value.IntegrityValid)).ToArray();

    /// <summary>Assemble deux secteurs physiques de 256 octets dans chaque bloc logique ProDOS de 512 octets.</summary>
    private static SectorBlock[] CreateProDosBlocks(IReadOnlyDictionary<(int Track, int Sector), DecodedSector> selected, int trackCount)
    {
        var blocks = new List<SectorBlock>();
        for (var track = 0; track < trackCount; track++)
        {
            for (var block = 0; block < 8; block++)
            {
                var first = AppleIIGeometry.ProDosToPhysical[block * 2];
                var second = AppleIIGeometry.ProDosToPhysical[(block * 2) + 1];
                if (!selected.TryGetValue((track, first), out var low) || !selected.TryGetValue((track, second), out var high)) continue;
                blocks.Add(new(track * 8 + block, new(track, 0, block), low.Data!.Concat(high.Data!).ToArray(), low.IntegrityValid == true && high.IntegrityValid == true));
            }
        }
        return blocks.ToArray();
    }

    /// <summary>Place les blocs disponibles à leur position logique dans un tampon dense utilisé uniquement par les sondes.</summary>
    private static byte[] ToDense(IEnumerable<SectorBlock> blocks, int count, int blockSize)
    {
        var data = new byte[count * blockSize];
        foreach (var block in blocks) block.Data.ToArray().CopyTo(data, block.LogicalBlock * blockSize);
        return data;
    }
}
