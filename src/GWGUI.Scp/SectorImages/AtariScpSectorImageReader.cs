using GWGUI.Scp.Decoding;
using GWGUI.Scp.Images;

namespace GWGUI.Scp.SectorImages;

public sealed class AtariScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    public async Task<SectorImage> ReadAsync(string path, string? formatId = null, CancellationToken cancellationToken = default)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var atari8 = formatId?.StartsWith("atari.", StringComparison.OrdinalIgnoreCase) == true;
        var atariSt = formatId?.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase) == true;
        var amstrad = formatId?.StartsWith("amstrad.", StringComparison.OrdinalIgnoreCase) == true;
        var ibm = formatId?.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase) == true;
        var bbc = formatId?.StartsWith("acorn.dfs.", StringComparison.OrdinalIgnoreCase) == true;
        var epson = formatId?.StartsWith("epson.qx10.", StringComparison.OrdinalIgnoreCase) == true;
        var ucsd = formatId?.Equals("ucsd.ibm.mfm", StringComparison.OrdinalIgnoreCase) == true;
        var candidates = new Dictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>>();
        var physicalCandidates = new Dictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>>();
        foreach (var track in scp.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var revolution = 0; revolution < track.Revolutions.Count; revolution++)
            {
                FluxDecodeResult result;
                if (bbc) result = decoders.Decode("iso.fm", track.Revolutions[revolution]);
                else if (atari8) result = decoders.Decode(formatId == "atari.90" ? "iso.fm" : "iso.mfm", track.Revolutions[revolution]);
                else if (atariSt) result = decoders.Decode("iso.mfm", track.Revolutions[revolution]);
                else
                {
                    var fm = decoders.Decode("iso.fm", track.Revolutions[revolution]); var mfm = decoders.Decode("iso.mfm", track.Revolutions[revolution]);
                    result = Score(fm) > Score(mfm) ? fm : mfm;
                }
                foreach (var sector in result.Sectors ?? [])
                {
                    if (sector.Data is null || sector.Number < 0) continue;
                    AddCandidate(physicalCandidates, new(track.Cylinder, track.Head, sector.Number), sector, revolution + 1);
                    if (sector.Cylinder != track.Cylinder || sector.Head != track.Head) continue;
                    var address = new SectorAddress(sector.Cylinder, sector.Head, sector.Number);
                    AddCandidate(candidates, address, sector, revolution + 1);
                }
            }
        }
        if (candidates.Count == 0 && physicalCandidates.Count == 0) throw new InvalidDataException("No Atari ISO FM/MFM sectors could be decoded from the SCP image.");
        if (!epson && formatId is null && TryDetectEpsonQx10Format(physicalCandidates, out var detectedEpsonFormat))
        {
            formatId = detectedEpsonFormat;
            epson = true;
        }
        if (epson || ucsd) candidates = physicalCandidates;
        if (candidates.Count == 0)
            throw new InvalidDataException("No consistently addressed Atari ISO FM/MFM sectors could be decoded from the SCP image.");
        var sectorSize = candidates.Values.SelectMany(value => value).GroupBy(value => value.Sector.Data!.Count).OrderByDescending(group => group.Count()).First().Key;
        var cylinders = candidates.Keys.Max(address => address.Cylinder) + 1; var heads = candidates.Keys.Max(address => address.Head) + 1;
        var sectorsPerTrack = candidates.Keys.GroupBy(address => (address.Cylinder, address.Head)).Select(group => group.Select(item => item.Number).Distinct().Count())
            .GroupBy(count => count).OrderByDescending(group => group.Count()).ThenByDescending(group => group.Key).First().Key;
        if (bbc)
        {
            cylinders = formatId!.EndsWith("80", StringComparison.OrdinalIgnoreCase) ? 80 : 40;
            heads = formatId.Contains(".ds", StringComparison.OrdinalIgnoreCase) ? 2 : 1;
            sectorsPerTrack = 10;
        }
        if (ucsd)
        {
            cylinders = candidates.Keys.Max(address => address.Cylinder) + 1;
            heads = 1;
            sectorsPerTrack = 8;
        }
        var sectorOrder = candidates.Keys.Select(address => address.Number).Distinct().OrderBy(number => number).ToArray();
        var zeroBased = sectorOrder.Length > 0 && sectorOrder[0] == 0;
        if (sectorSize == 512 && !zeroBased && (ibm || formatId is null))
        {
            var boot = BestData(candidates, new(0, 0, 1));
            var fat = BestData(candidates, new(0, 0, 2));
            var fatMedia = fat.Length > 0 ? fat[0] : (byte)0;
            var detectedAsIbm = ibm
                ? IbmPcImageReader.TryDetectFluxGeometry(boot, fatMedia, out var detected)
                : IbmPcImageReader.TryIdentifyFluxGeometry(boot, fatMedia, out detected);
            if (detectedAsIbm)
            {
                ibm = true;
                cylinders = detected.Cylinders; heads = detected.Heads; sectorsPerTrack = detected.SectorsPerTrack;
            }
        }
        var is8Bit = atari8 || (!atariSt && sectorSize is 128 or 256 && heads == 1 && sectorsPerTrack is 18 or 26);
        var resolvedFormat = formatId ?? (zeroBased && sectorSize == 256 && sectorsPerTrack == 10
            ? heads == 1 ? cylinders == 40 ? "acorn.dfs.ss" : "acorn.dfs.ss80" : cylinders == 40 ? "acorn.dfs.ds" : "acorn.dfs.ds80"
            : is8Bit
            ? (sectorSize, sectorsPerTrack) switch { (128, 18) => "atari.90", (128, 26) => "atari.130", (256, 18) => "atari.180", _ => $"atari.scp.{sectorSize}.{sectorsPerTrack}" }
            : $"atarist.{(cylinders * heads * sectorsPerTrack * sectorSize) / 1024}");
        if (ibm) resolvedFormat = IbmPcImageReader.FormatIdForGeometry(cylinders, heads, sectorsPerTrack, sectorSize);
        if (epson) return CreateEpsonQx10Image(formatId!, candidates);
        var blocks = new List<SectorBlock>();
        foreach (var (address, values) in candidates)
        {
            if (!amstrad && address.Number > sectorsPerTrack) continue;
            if (address.Cylinder >= cylinders || address.Head >= heads) continue;
            var best = values.OrderByDescending(value => value.Sector.IntegrityValid == true).ThenByDescending(value => value.Sector.IntegrityValid is null).First();
            var sectorIndex = ucsd
                ? Array.IndexOf(candidates.Keys
                    .Where(item => item.Cylinder == address.Cylinder && item.Head == address.Head)
                    .Select(item => item.Number).Distinct().OrderBy(number => number).ToArray(), address.Number)
                : amstrad || zeroBased ? Array.IndexOf(sectorOrder, address.Number) : address.Number - 1;
            if (sectorIndex < 0 || sectorIndex >= sectorsPerTrack) continue;
            var logical = (address.Cylinder * heads + address.Head) * sectorsPerTrack + sectorIndex;
            blocks.Add(new(logical, address, best.Sector.Data!.ToArray(), best.Sector.IntegrityValid, best.Revolution));
        }
        var capacity = is8Bit && sectorSize > 128 ? 3L * 128 + (cylinders * heads * sectorsPerTrack - 3L) * sectorSize : (long)cylinders * heads * sectorsPerTrack * sectorSize;
        return new(resolvedFormat, sectorSize, cylinders, heads, sectorsPerTrack, blocks,
            allowVariableBlockSize: is8Bit && sectorSize > 128, capacity: capacity);
    }

    private static double Score(FluxDecodeResult result) => (result.Sectors?.Count(sector => sector.Data is not null) ?? 0) * 10 + result.Confidence;

    private static void AddCandidate(Dictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>> candidates,
        SectorAddress address, DecodedSector sector, int revolution)
    {
        if (!candidates.TryGetValue(address, out var list)) candidates[address] = list = [];
        list.Add((sector, revolution));
    }

    private static byte[] BestData(IReadOnlyDictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>> candidates, SectorAddress address)
    {
        if (!candidates.TryGetValue(address, out var values)) return [];
        return values.OrderByDescending(value => value.Sector.IntegrityValid == true)
            .ThenByDescending(value => value.Sector.IntegrityValid is null)
            .Select(value => value.Sector.Data?.ToArray() ?? [])
            .FirstOrDefault(data => data.Length > 0) ?? [];
    }

    private static SectorImage CreateEpsonQx10Image(string formatId,
        IReadOnlyDictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>> candidates)
    {
        var geometry = EpsonQx10Geometry(formatId);
        var blocks = new List<SectorBlock>();
        var logical = 0;
        long capacity = 0;
        var maximumSectors = 0;
        var sizes = new HashSet<int>();

        for (var cylinder = 0; cylinder < geometry.Cylinders; cylinder++)
        {
            for (var head = 0; head < geometry.Heads; head++)
            {
                var track = geometry.Track(cylinder, head);
                maximumSectors = Math.Max(maximumSectors, track.Count);
                for (var index = 0; index < track.Count; index++, logical++)
                {
                    var sectorNumber = track.FirstSector + index;
                    var address = new SectorAddress(cylinder, head, sectorNumber);
                    capacity += track.SectorSize;
                    sizes.Add(track.SectorSize);
                    if (!candidates.TryGetValue(address, out var values)) continue;

                    var best = values.Where(value => value.Sector.Data?.Count == track.SectorSize)
                        .OrderByDescending(value => value.Sector.IntegrityValid == true)
                        .ThenByDescending(value => value.Sector.IntegrityValid is null)
                        .FirstOrDefault();
                    if (best.Sector?.Data is null) continue;
                    blocks.Add(new(logical, address, best.Sector.Data.ToArray(), best.Sector.IntegrityValid, best.Revolution));
                }
            }
        }

        var blockSize = sizes.GroupBy(size => size).OrderByDescending(group =>
            geometry.AllTracks.Where(track => track.SectorSize == group.Key).Sum(track => track.Count)).First().Key;
        return new(formatId, blockSize, geometry.Cylinders, geometry.Heads, maximumSectors, blocks,
            allowVariableBlockSize: sizes.Count > 1, capacity: capacity, logicalBlockCount: logical);
    }

    private static EpsonGeometry EpsonQx10Geometry(string formatId) => formatId.ToLowerInvariant() switch
    {
        "epson.qx10.320" => EpsonGeometry.Uniform(40, 2, new(1, 16, 256)),
        "epson.qx10.400" => EpsonGeometry.Uniform(40, 2, new(1, 5, 1024)),
        "epson.qx10.booter" => new(15, 1, (cylinder, _) => cylinder == 0 ? new(1, 16, 256) : new(1, 17, 256)),
        "epson.qx10.399" => new(40, 2, (cylinder, head) => cylinder == 0 && head == 0 ? new(1, 16, 256) : new(1, 10, 512)),
        "epson.qx10.logo" => new(40, 2, (cylinder, _) => cylinder switch
        {
            0 or 1 or 4 => new(1, 16, 256),
            5 or 6 => new(2, 10, 512),
            3 or 7 => default,
            _ => new(1, 10, 512)
        }),
        _ => new(40, 2, (cylinder, _) => cylinder <= 1 ? new(1, 16, 256) : new(1, 10, 512))
    };

    private static bool TryDetectEpsonQx10Format(
        IReadOnlyDictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>> candidates, out string formatId)
    {
        formatId = string.Empty;
        var tracks = candidates.GroupBy(pair => (pair.Key.Cylinder, pair.Key.Head))
            .Select(group => new DetectedEpsonTrack(group.Key.Cylinder, group.Key.Head,
                group.Select(pair => new DetectedEpsonSector(pair.Key.Number,
                    pair.Value
                        .Where(value => value.Sector.Data is not null)
                        .GroupBy(value => value.Sector.IntegrityValid == true ? 2 : value.Sector.IntegrityValid is null ? 1 : 0)
                        .OrderByDescending(group => group.Key)
                        .First()
                        .GroupBy(value => value.Sector.Data!.Count)
                        .OrderByDescending(sizes => sizes.Count())
                        .ThenByDescending(sizes => sizes.Key)
                        .First().Key)).ToArray())).ToArray();
        if (tracks.Length == 0) return false;

        static bool Matches(DetectedEpsonTrack track, int first, int count, int size) =>
            track.Sectors.Count == count && track.Sectors.All(sector =>
                sector.Number >= first && sector.Number < first + count && sector.Size == size);

        if (tracks.All(track => Matches(track, 1, 16, 256))) formatId = "epson.qx10.320";
        else if (tracks.All(track => Matches(track, 1, 5, 1024))) formatId = "epson.qx10.400";
        else if (tracks.Length <= 15 && tracks.All(track => track.Head == 0 &&
                     Matches(track, 1, track.Cylinder == 0 ? 16 : 17, 256))) formatId = "epson.qx10.booter";
        else
        {
            var smallTracks = tracks.Where(track => Matches(track, 1, 16, 256)).ToArray();
            var normalTracks = tracks.Where(track => Matches(track, 1, 10, 512)).ToArray();
            if (smallTracks.Length == 1 && smallTracks[0].Cylinder == 0 && smallTracks[0].Head == 0 &&
                smallTracks.Length + normalTracks.Length == tracks.Length) formatId = "epson.qx10.399";
            else if (smallTracks.Length >= 4 && smallTracks.All(track => track.Cylinder <= 1) &&
                     smallTracks.Length + normalTracks.Length == tracks.Length) formatId = "epson.qx10.396";
            else
            {
                var shiftedTracks = tracks.Where(track => Matches(track, 2, 10, 512)).ToArray();
                if (smallTracks.Length >= 6 && smallTracks.All(track => track.Cylinder is 0 or 1 or 4) &&
                    shiftedTracks.All(track => track.Cylinder is 5 or 6) &&
                    smallTracks.Length + normalTracks.Length + shiftedTracks.Length == tracks.Length)
                    formatId = "epson.qx10.logo";
            }
        }
        return formatId.Length > 0;
    }

    private readonly record struct EpsonTrack(int FirstSector, int Count, int SectorSize);
    private readonly record struct DetectedEpsonSector(int Number, int Size);
    private readonly record struct DetectedEpsonTrack(int Cylinder, int Head, IReadOnlyList<DetectedEpsonSector> Sectors);

    private sealed record EpsonGeometry(int Cylinders, int Heads, Func<int, int, EpsonTrack> Track)
    {
        public IEnumerable<EpsonTrack> AllTracks
        {
            get
            {
                for (var cylinder = 0; cylinder < Cylinders; cylinder++)
                    for (var head = 0; head < Heads; head++)
                        yield return Track(cylinder, head);
            }
        }

        public static EpsonGeometry Uniform(int cylinders, int heads, EpsonTrack track) =>
            new(cylinders, heads, (_, _) => track);
    }
}
