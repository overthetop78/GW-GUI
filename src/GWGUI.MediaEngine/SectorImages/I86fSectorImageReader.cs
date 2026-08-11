using GWGUI.MediaEngine.Containers.I86f;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Flux.Conversion;
using GWGUI.MediaEngine.Images;

namespace GWGUI.MediaEngine.SectorImages;

public sealed class I86fSectorImageReader(I86fReader reader, FluxDecoderRegistry decoders)
{
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var container = await reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var candidates = new Dictionary<SectorAddress, List<DecodedSector>>();
        foreach (var track in container.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var revolution = I86fBitCellFluxConverter.Convert(track.Bits);
            if (revolution is null) continue;
            var decoderId = (track.Flags & I86fTrackFlags.EncodingMask) == I86fTrackFlags.MfmEncoding ? FluxDecoderIds.IsoMfm : FluxDecoderIds.IsoFm;
            var decoded = decoders.Decode(decoderId, revolution);
            foreach (var sector in decoded.Sectors ?? [])
            {
                if (sector.Data is null || sector.Number < 0) continue;
                var address = new SectorAddress(sector.Cylinder, sector.Head, sector.Number);
                if (!candidates.TryGetValue(address, out var values)) candidates[address] = values = [];
                values.Add(sector);
            }
        }
        if (candidates.Count == 0) throw new InvalidDataException("No FM or MFM sector could be decoded from the 86F image.");
        return BuildSectorImage(candidates);
    }

    private static SectorImage BuildSectorImage(Dictionary<SectorAddress, List<DecodedSector>> candidates)
    {
        var sectorSize = candidates.Values.SelectMany(value => value).GroupBy(value => value.Data!.Count).OrderByDescending(group => group.Count()).First().Key;
        var cylinders = candidates.Keys.Max(address => address.Cylinder) + 1;
        var heads = candidates.Keys.Max(address => address.Head) + 1;
        var sectorNumbers = candidates.Keys.Select(address => address.Number).Distinct().OrderBy(value => value).ToArray();
        var sectorsPerTrack = candidates.Keys.GroupBy(address => (address.Cylinder, address.Head)).Select(group => group.Select(address => address.Number).Distinct().Count()).GroupBy(value => value).OrderByDescending(group => group.Count()).ThenByDescending(group => group.Key).First().Key;
        var zeroBased = sectorNumbers.Length > 0 && sectorNumbers[0] == 0;
        var blocks = new List<SectorBlock>();
        foreach (var (address, values) in candidates)
        {
            if (address.Cylinder >= cylinders || address.Head >= heads) continue;
            var sectorIndex = zeroBased ? Array.IndexOf(sectorNumbers, address.Number) : address.Number - 1;
            if (sectorIndex < 0 || sectorIndex >= sectorsPerTrack) continue;
            var matchingSize = values.Where(value => value.Data?.Count == sectorSize).ToArray();
            if (matchingSize.Length == 0) continue;
            var best = matchingSize.OrderByDescending(value => value.IntegrityValid == true).ThenByDescending(value => value.IntegrityValid is null).First();
            var logical = (address.Cylinder * heads + address.Head) * sectorsPerTrack + sectorIndex;
            blocks.Add(new(logical, address, best.Data!.ToArray(), best.IntegrityValid));
        }
        var format = sectorSize == 512 ? IbmPcImageReader.FormatIdForGeometry(cylinders, heads, sectorsPerTrack, sectorSize) : $"86f.{sectorSize}.{cylinders}.{heads}.{sectorsPerTrack}";
        return new(format, sectorSize, cylinders, heads, sectorsPerTrack, blocks, capacity: (long)cylinders * heads * sectorsPerTrack * sectorSize);
    }
}
