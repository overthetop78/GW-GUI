using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.SectorImages;

public sealed class IsoScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    public async Task<SectorImage> ReadAsync(string path, string? formatId = null, CancellationToken cancellationToken = default)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var policy = IsoScpSectorImagePolicyRegistry.Resolve(formatId);
        var candidates = new Dictionary<SectorAddress, List<IsoSectorCandidate>>();
        var physicalCandidates = new Dictionary<SectorAddress, List<IsoSectorCandidate>>();
        foreach (var track in scp.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var revolution = 0; revolution < track.Revolutions.Count; revolution++)
            {
                var result = policy.DecoderIds.Select(decoder => decoders.Decode(decoder, track.Revolutions[revolution].Flux))
                    .OrderByDescending(Score).First();
                foreach (var sector in result.Sectors)
                {
                    if (sector.Data is null || sector.Number < 0) continue;
                    AddCandidate(physicalCandidates, new(track.Cylinder, track.Head, sector.Number), sector, revolution + 1);
                    if (sector.Cylinder != track.Cylinder || sector.Head != track.Head) continue;
                    var address = new SectorAddress(sector.Cylinder, sector.Head, sector.Number);
                    AddCandidate(candidates, address, sector, revolution + 1);
                }
            }
        }
        if (candidates.Count == 0 && physicalCandidates.Count == 0)
            throw new InvalidDataException("No ISO FM/MFM sectors could be decoded from the SCP image.");
        return policy.Build(formatId, new(candidates, physicalCandidates));
    }

    private static double Score(FluxDecodeResult result) => result.Sectors.Count(sector => sector.Data is not null) * 10 + result.Confidence;

    private static void AddCandidate(Dictionary<SectorAddress, List<IsoSectorCandidate>> candidates, SectorAddress address, DecodedSector sector, int revolution)
    {
        if (!candidates.TryGetValue(address, out var list)) candidates[address] = list = [];
        list.Add(new(sector, revolution));
    }

}
