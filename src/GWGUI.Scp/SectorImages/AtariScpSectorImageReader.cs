using GWGUI.Scp.Decoding;

namespace GWGUI.Scp.SectorImages;

public sealed class AtariScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    public async Task<SectorImage> ReadAsync(string path, string? formatId = null, CancellationToken cancellationToken = default)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var atari8 = formatId?.StartsWith("atari.", StringComparison.OrdinalIgnoreCase) == true;
        var atariSt = formatId?.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase) == true;
        var amstrad = formatId?.StartsWith("amstrad.", StringComparison.OrdinalIgnoreCase) == true;
        var candidates = new Dictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>>();
        foreach (var track in scp.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var revolution = 0; revolution < track.Revolutions.Count; revolution++)
            {
                FluxDecodeResult result;
                if (atari8) result = decoders.Decode(formatId == "atari.90" ? "iso.fm" : "iso.mfm", track.Revolutions[revolution]);
                else if (atariSt) result = decoders.Decode("iso.mfm", track.Revolutions[revolution]);
                else
                {
                    var fm = decoders.Decode("iso.fm", track.Revolutions[revolution]); var mfm = decoders.Decode("iso.mfm", track.Revolutions[revolution]);
                    result = Score(fm) > Score(mfm) ? fm : mfm;
                }
                foreach (var sector in result.Sectors ?? [])
                {
                    if (sector.Data is null || sector.Cylinder != track.Cylinder || sector.Head != track.Head || sector.Number <= 0) continue;
                    var address = new SectorAddress(sector.Cylinder, sector.Head, sector.Number);
                    if (!candidates.TryGetValue(address, out var list)) candidates[address] = list = [];
                    list.Add((sector, revolution + 1));
                }
            }
        }
        if (candidates.Count == 0) throw new InvalidDataException("No Atari ISO FM/MFM sectors could be decoded from the SCP image.");
        var sectorSize = candidates.Values.SelectMany(value => value).GroupBy(value => value.Sector.Data!.Count).OrderByDescending(group => group.Count()).First().Key;
        var cylinders = candidates.Keys.Max(address => address.Cylinder) + 1; var heads = candidates.Keys.Max(address => address.Head) + 1;
        var sectorsPerTrack = candidates.Keys.GroupBy(address => (address.Cylinder, address.Head)).Select(group => group.Select(item => item.Number).Distinct().Count())
            .GroupBy(count => count).OrderByDescending(group => group.Count()).ThenByDescending(group => group.Key).First().Key;
        var is8Bit = atari8 || (!atariSt && sectorSize is 128 or 256 && heads == 1 && sectorsPerTrack is 18 or 26);
        var resolvedFormat = formatId ?? (is8Bit
            ? (sectorSize, sectorsPerTrack) switch { (128, 18) => "atari.90", (128, 26) => "atari.130", (256, 18) => "atari.180", _ => $"atari.scp.{sectorSize}.{sectorsPerTrack}" }
            : $"atarist.{(cylinders * heads * sectorsPerTrack * sectorSize) / 1024}");
        var blocks = new List<SectorBlock>();
        var sectorOrder = candidates.Keys.Select(address => address.Number).Distinct().OrderBy(number => number).ToArray();
        foreach (var (address, values) in candidates)
        {
            if (!amstrad && address.Number > sectorsPerTrack) continue;
            var best = values.OrderByDescending(value => value.Sector.IntegrityValid == true).ThenByDescending(value => value.Sector.IntegrityValid is null).First();
            var sectorIndex = amstrad ? Array.IndexOf(sectorOrder, address.Number) : address.Number - 1;
            if (sectorIndex < 0 || sectorIndex >= sectorsPerTrack) continue;
            var logical = (address.Cylinder * heads + address.Head) * sectorsPerTrack + sectorIndex;
            blocks.Add(new(logical, address, best.Sector.Data!.ToArray(), best.Sector.IntegrityValid, best.Revolution));
        }
        var capacity = is8Bit && sectorSize > 128 ? 3L * 128 + (cylinders * heads * sectorsPerTrack - 3L) * sectorSize : (long)cylinders * heads * sectorsPerTrack * sectorSize;
        return new(resolvedFormat, sectorSize, cylinders, heads, sectorsPerTrack, blocks,
            allowVariableBlockSize: is8Bit && sectorSize > 128, capacity: capacity);
    }

    private static double Score(FluxDecodeResult result) => (result.Sectors?.Count(sector => sector.Data is not null) ?? 0) * 10 + result.Confidence;
}
