using GWGUI.Scp.Decoding;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

internal static class AppleSectorImageFactory
{
    public static SectorImage CreateLinear(byte[] data, string formatId, int blockSize, int cylinders, int heads, int sectorsPerTrack)
    {
        var count = data.Length / blockSize;
        var blocks = new SectorBlock[count];
        for (var logical = 0; logical < count; logical++)
        {
            var perCylinder = heads * sectorsPerTrack;
            blocks[logical] = new(logical,
                new(logical / perCylinder, logical / sectorsPerTrack % heads, logical % sectorsPerTrack),
                data.AsSpan(logical * blockSize, blockSize).ToArray());
        }
        return new(formatId, blockSize, cylinders, heads, sectorsPerTrack, blocks,
            capacity: data.Length, logicalBlockCount: count);
    }

    public static SectorImage CreateAppleMacZoned(byte[] data, string formatId, int heads)
    {
        var count = data.Length / 512;
        var blocks = new SectorBlock[count];
        for (var logical = 0; logical < count; logical++)
            blocks[logical] = new(logical, AppleDiskGeometry.AppleMacZonedAddress(logical, heads),
                data.AsSpan(logical * 512, 512).ToArray());
        return new(formatId, 512, 80, heads, 12, blocks,
            capacity: data.Length, logicalBlockCount: count);
    }

    public static SectorImage CreateAppleIIFromDecodedTracks(
        IEnumerable<(int Track, IReadOnlyList<DecodedSector> Sectors)> decodedTracks)
    {
        var selected = decodedTracks.SelectMany(item => item.Sectors
                .Where(sector => sector.Data is { Count: 256 } && sector.Number is >= 0 and < 16)
                .Select(sector => (item.Track, Sector: sector)))
            .GroupBy(item => (item.Track, item.Sector.Number))
            .ToDictionary(group => group.Key,
                group => group.OrderByDescending(item => item.Sector.IntegrityValid == true).First().Sector);
        var trackCount = Math.Max(35, selected.Count == 0 ? 35 : selected.Keys.Max(key => key.Track) + 1);
        var sectorsPerTrack = selected.Count > 0 && selected.Keys.Max(key => key.Number) < 13 ? 13 : 16;
        var dosBlocks = selected.Where(pair => pair.Key.Number < sectorsPerTrack)
            .Select(pair => new SectorBlock(
                pair.Key.Track * sectorsPerTrack + (sectorsPerTrack == 16
                    ? AppleDiskGeometry.PhysicalToDos[pair.Key.Number]
                    : pair.Key.Number),
                new(pair.Key.Track, 0, pair.Key.Number), pair.Value.Data!.ToArray(), pair.Value.IntegrityValid))
            .ToArray();
        if (dosBlocks.Length == 0) return new("apple2.gcr", 256, trackCount, 1, 16, []);

        if (sectorsPerTrack == 13)
            return new("apple2.dos32", 256, trackCount, 1, 13, dosBlocks);

        var prodosBlocks = new List<SectorBlock>();
        for (var track = 0; track < trackCount; track++)
        for (var block = 0; block < 8; block++)
        {
            var first = AppleDiskGeometry.ProDosToPhysical[block * 2];
            var second = AppleDiskGeometry.ProDosToPhysical[block * 2 + 1];
            if (!selected.TryGetValue((track, first), out var low) ||
                !selected.TryGetValue((track, second), out var high)) continue;
            var data = low.Data!.Concat(high.Data!).ToArray();
            prodosBlocks.Add(new(track * 8 + block, new(track, 0, block), data,
                low.IntegrityValid == true && high.IntegrityValid == true));
        }
        var prodosProbe = new byte[trackCount * 8 * 512];
        foreach (var block in prodosBlocks)
            block.Data.ToArray().CopyTo(prodosProbe, block.LogicalBlock * 512);
        if (AppleDiskImageSignatures.LooksLikeProDos(prodosProbe))
            return new("apple2.prodos", 512, trackCount, 1, 8, prodosBlocks);
        return new(AppleDiskImageSignatures.LooksLikeDos33(ToDense(dosBlocks, trackCount * 16, 256))
                ? "apple2.dos33"
                : "apple2.gcr",
            256, trackCount, 1, sectorsPerTrack, dosBlocks);
    }

    public static SectorImage CreateRwts18FromDecodedTracks(
        IEnumerable<(int Track, IReadOnlyList<DecodedSector> Sectors)> decodedTracks)
    {
        var blocks = decodedTracks.SelectMany(item => item.Sectors
                .Where(sector => sector.Data is { Count: 768 } && sector.Number is >= 0 and < 6)
                .Select(sector => (item.Track, Sector: sector)))
            .GroupBy(item => (item.Track, item.Sector.Number))
            .Select(group => group.OrderByDescending(item => item.Sector.IntegrityValid == true).First())
            .Select(item => new SectorBlock(item.Track * 6 + item.Sector.Number,
                new(item.Track, 0, item.Sector.Number), item.Sector.Data!.ToArray(), item.Sector.IntegrityValid))
            .ToArray();
        if (blocks.Length == 0)
            throw new InvalidDataException("No Apple II RWTS18 sectors could be decoded.");
        var trackCount = Math.Max(35, blocks.Max(block => block.Address.Cylinder) + 1);
        return new("apple2.rwts18", 768, trackCount, 1, 6, blocks);
    }

    private static byte[] ToDense(IEnumerable<SectorBlock> blocks, int count, int blockSize)
    {
        var data = new byte[count * blockSize];
        foreach (var block in blocks)
            block.Data.ToArray().CopyTo(data, block.LogicalBlock * blockSize);
        return data;
    }
}
