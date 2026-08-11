using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Images;

namespace GWGUI.MediaEngine.SectorImages;

internal sealed class AppleScpSectorDecoder(FluxDecoderRegistry decoders)
{
    private readonly AppleMacGcrDecoder _macDecoder = new();

    public Dictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>> DecodeCandidates(ScpImage scp, string decoderId, int size, CancellationToken cancellationToken)
    {
        var result = new Dictionary<SectorAddress, List<(DecodedSector, int)>>();
        foreach (var track in scp.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var revolution = 0; revolution < track.Revolutions.Count; revolution++)
            {
                var decoded = decoderId == FluxCodecIds.AppleMacGcr
                    ? DecodeMacTrack(track, track.Revolutions[revolution])
                    : decoders.Decode(decoderId, track.Revolutions[revolution]);
                foreach (var sector in decoded.Sectors ?? [])
                {
                    if (sector.Data is not { Count: var length } || length != size) continue;
                    var address = new SectorAddress(sector.Cylinder, sector.Head, sector.Number);
                    if (!result.TryGetValue(address, out var list)) result[address] = list = [];
                    list.Add((sector, revolution + 1));
                }
            }
        }
        return result;
    }

    public static SectorBlock Select(int logical, SectorAddress address,
        List<(DecodedSector Sector, int Revolution)> values)
    {
        var best = values.OrderByDescending(value => value.Sector.IntegrityValid == true)
            .ThenByDescending(value => value.Sector.IntegrityValid is null).First();
        return new(logical, address, best.Sector.Data!.ToArray(), best.Sector.IntegrityValid, best.Revolution,
            best.Sector.Tag?.ToArray());
    }

    public static bool TryFlattenPayload(SectorImage image, out byte[] payload)
    {
        payload = new byte[image.BlockCount * image.BlockSize];
        if (image.AvailableBlocks.Count != image.BlockCount) return false;
        foreach (var block in image.AvailableBlocks)
        {
            if (block.LogicalBlock < 0 || block.LogicalBlock >= image.BlockCount || block.Data.Count != image.BlockSize) return false;
            block.Data.ToArray().CopyTo(payload, block.LogicalBlock * image.BlockSize);
        }
        return true;
    }

    private FluxDecodeResult DecodeMacTrack(ScpTrack track, ScpRevolution revolution)
    {
        var expected = AppleDiskGeometry.AppleMacSectors(track.Cylinder);
        var initial = FluxTimingEstimator.EstimateNonFmBitCell(revolution.FluxIntervals) * 2;
        var factors = new[] { 1.0, .95, 1.05, .9, 1.1, .85, 1.15 }.Distinct();
        FluxDecodeResult? best = null;
        var bestScore = int.MinValue;
        foreach (var factor in factors)
        {
            var candidate = _macDecoder.DecodeAtBitCell(revolution, initial * factor);
            var plausible = candidate.Sectors?.Where(sector => sector.Data?.Count == 512 &&
                sector.Cylinder == track.Cylinder && sector.Head == track.Head &&
                sector.Number >= 0 && sector.Number < expected).ToArray() ?? [];
            var score = plausible.Select(sector => sector.Number).Distinct().Count() * 100 +
                        plausible.Count(sector => sector.IntegrityValid == true) * 10 + plausible.Length;
            if (score > bestScore)
            {
                best = candidate;
                bestScore = score;
            }
            if (plausible.Where(sector => sector.IntegrityValid == true).Select(sector => sector.Number)
                    .Distinct().Count() == expected) break;
        }
        return best ?? _macDecoder.Decode(revolution);
    }
}
