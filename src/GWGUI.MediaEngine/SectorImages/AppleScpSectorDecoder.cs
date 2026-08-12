using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Reconstruction.Apple;
using GWGUI.MediaEngine.Geometries.Apple;

namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Décode et sélectionne les candidats sectoriels communs aux reconstructeurs Apple SCP.</summary>
internal sealed class AppleScpSectorDecoder(FluxDecoderRegistry decoders)
{
    private readonly AppleMacGcrDecoder _macDecoder = new();

    /// <summary>Décode toutes les révolutions avec le codec demandé et regroupe les candidats par adresse.</summary>
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
                    : decoders.Decode(decoderId, track.Revolutions[revolution].Flux);
                foreach (var sector in decoded.Sectors)
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

    /// <summary>Sélectionne le meilleur candidat d'une adresse selon son intégrité.</summary>
    public static SectorBlock Select(int logical, SectorAddress address,
        List<(DecodedSector Sector, int Revolution)> values)
    {
        var best = values.OrderByDescending(value => value.Sector.IntegrityValid == true)
            .ThenByDescending(value => value.Sector.IntegrityValid is null).First();
        return new(logical, address, best.Sector.Data!.ToArray(), best.Sector.IntegrityValid, best.Revolution, best.Sector.Tag?.ToArray(), best.Sector.FormatCode);
    }

    /// <summary>Reconstruit une charge utile linéaire lorsque tous les blocs sont présents.</summary>
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

    /// <summary>Décode une révolution Macintosh avec plusieurs durées de cellule voisines.</summary>
    private FluxDecodeResult DecodeMacTrack(ScpTrack track, ScpRevolution revolution)
    {
        var expected = MacintoshGcrGeometry.Sectors(track.Cylinder);
        var initial = FluxTimingEstimator.EstimateNonFmBitCell(revolution.FluxIntervals) * 2;
        var factors = AppleScpReconstructionDefinitions.MacintoshBitCellFactors;
        FluxDecodeResult? best = null;
        var bestScore = int.MinValue;
        foreach (var factor in factors)
        {
            var candidate = _macDecoder.DecodeAtBitCell(revolution.Flux, initial * factor);
            var plausible = candidate.Sectors.Where(sector => sector.Data?.Count == AppleIwmGcrFormat.SectorByteCount &&
                sector.Cylinder == track.Cylinder && sector.Head == track.Head &&
                sector.Number >= 0 && sector.Number < expected).ToArray() ?? [];
            var score = plausible.Select(sector => sector.Number).Distinct().Count() * AppleScpReconstructionDefinitions.DistinctSectorScoreWeight + plausible.Count(sector => sector.IntegrityValid == true) * AppleScpReconstructionDefinitions.ValidSectorScoreWeight + plausible.Length;
            if (score > bestScore)
            {
                best = candidate;
                bestScore = score;
            }
            if (plausible.Where(sector => sector.IntegrityValid == true).Select(sector => sector.Number)
                    .Distinct().Count() == expected) break;
        }
        return best ?? _macDecoder.Decode(revolution.Flux);
    }
}
