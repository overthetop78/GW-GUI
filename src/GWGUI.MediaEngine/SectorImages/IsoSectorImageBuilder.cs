namespace GWGUI.MediaEngine.SectorImages;

internal static class IsoSectorImageBuilder
{
    public static (int SectorSize, int Cylinders, int Heads, int SectorsPerTrack, int[] SectorOrder, bool ZeroBased)
        Measure(IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> candidates)
    {
        if (candidates.Count == 0)
            throw new InvalidDataException("No consistently addressed ISO FM/MFM sectors could be decoded from the SCP image.");

        var sectorSize = candidates.Values.SelectMany(value => value)
            .GroupBy(value => value.Sector.Data!.Count).OrderByDescending(group => group.Count()).First().Key;
        var cylinders = candidates.Keys.Max(address => address.Cylinder) + 1;
        var heads = candidates.Keys.Max(address => address.Head) + 1;
        var sectorsPerTrack = candidates.Keys.GroupBy(address => (address.Cylinder, address.Head))
            .Select(group => group.Select(item => item.Number).Distinct().Count())
            .GroupBy(count => count).OrderByDescending(group => group.Count()).ThenByDescending(group => group.Key).First().Key;
        var sectorOrder = candidates.Keys.Select(address => address.Number).Distinct().OrderBy(number => number).ToArray();
        return (sectorSize, cylinders, heads, sectorsPerTrack, sectorOrder, sectorOrder.Length > 0 && sectorOrder[0] == 0);
    }

    public static SectorImage CreateUniform(
        string formatId,
        IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> candidates,
        int sectorSize,
        int cylinders,
        int heads,
        int sectorsPerTrack,
        Func<SectorAddress, int> sectorIndex,
        bool allowSectorNumbersBeyondGeometry = false,
        bool allowVariableBlockSize = false,
        long? capacity = null)
    {
        var blocks = new List<SectorBlock>();
        foreach (var (address, values) in candidates)
        {
            if (!allowSectorNumbersBeyondGeometry && address.Number > sectorsPerTrack) continue;
            if (address.Cylinder >= cylinders || address.Head >= heads) continue;
            var index = sectorIndex(address);
            if (index < 0 || index >= sectorsPerTrack) continue;
            var best = Best(values);
            var logical = (address.Cylinder * heads + address.Head) * sectorsPerTrack + index;
            blocks.Add(new(logical, address, best.Sector.Data!.ToArray(), best.Sector.IntegrityValid, best.Revolution));
        }
        return new(formatId, sectorSize, cylinders, heads, sectorsPerTrack, blocks,
            allowVariableBlockSize: allowVariableBlockSize, capacity: capacity);
    }

    public static byte[] BestData(
        IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> candidates,
        SectorAddress address)
    {
        if (!candidates.TryGetValue(address, out var values)) return [];
        return values.OrderByDescending(value => value.Sector.IntegrityValid == true)
            .ThenByDescending(value => value.Sector.IntegrityValid is null)
            .Select(value => value.Sector.Data?.ToArray() ?? [])
            .FirstOrDefault(data => data.Length > 0) ?? [];
    }

    private static IsoSectorCandidate Best(IEnumerable<IsoSectorCandidate> candidates) =>
        candidates.OrderByDescending(value => value.Sector.IntegrityValid == true)
            .ThenByDescending(value => value.Sector.IntegrityValid is null).First();
}
