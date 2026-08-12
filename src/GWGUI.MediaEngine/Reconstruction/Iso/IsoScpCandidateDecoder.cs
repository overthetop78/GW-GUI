using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Reconstruction.Scp;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Décode et partage les candidats sectoriels ISO communs aux politiques d'une même capture SCP.</summary>
internal sealed class IsoScpCandidateDecoder(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    private readonly object cacheGate = new();
    private readonly Dictionary<string, CacheEntry> cache = new(StringComparer.Ordinal);

    /// <summary>Retourne les candidats décodés pour la capture et l'ensemble de codecs demandés.</summary>
    public async Task<IsoSectorCandidateSet> DecodeAsync(string path, IReadOnlyList<string> decoderIds, CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(path);
        var file = new FileInfo(fullPath);
        var fingerprint = file.Exists
            ? new CaptureFingerprint(fullPath, file.Length, file.LastWriteTimeUtc)
            : new CaptureFingerprint(fullPath, -1, DateTime.MinValue);
        var decoderKey = string.Join('\0', decoderIds);
        CacheEntry entry;
        lock (cacheGate)
        {
            if (!cache.TryGetValue(decoderKey, out entry!) || entry.Fingerprint != fingerprint)
            {
                entry = new(fingerprint, new(() => DecodeCoreAsync(fullPath, decoderIds, cancellationToken), LazyThreadSafetyMode.ExecutionAndPublication));
                cache[decoderKey] = entry;
            }
        }

        try
        {
            return await entry.Operation.Value.ConfigureAwait(false);
        }
        catch
        {
            lock (cacheGate)
                if (cache.TryGetValue(decoderKey, out var current) && ReferenceEquals(current, entry)) cache.Remove(decoderKey);
            throw;
        }
    }

    private async Task<IsoSectorCandidateSet> DecodeCoreAsync(string path, IReadOnlyList<string> decoderIds, CancellationToken cancellationToken)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var candidates = new Dictionary<SectorAddress, List<IsoSectorCandidate>>();
        var physicalCandidates = new Dictionary<SectorAddress, List<IsoSectorCandidate>>();
        foreach (var track in scp.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var window in ScpTrackDecodeWindowFactory.Create(track))
            {
                var result = decoderIds.Select(decoder => decoders.Decode(decoder, window.Flux))
                    .OrderByDescending(Score).First();
                foreach (var sector in result.Sectors)
                {
                    if (sector.Data is null || sector.Number < 0) continue;
                    AddCandidate(physicalCandidates, new(track.Cylinder, track.Head, sector.Number), sector, window.Revolution);
                    if (sector.Cylinder != track.Cylinder || sector.Head != track.Head) continue;
                    AddCandidate(candidates, new(sector.Cylinder, sector.Head, sector.Number), sector, window.Revolution);
                }
            }
        }

        return new(candidates, physicalCandidates);
    }

    private static double Score(FluxDecodeResult result) =>
        result.Sectors.Count(sector => sector.Data is not null) * IsoScpReconstructionDefinitions.DataSectorScoreWeight + result.Confidence;

    private static void AddCandidate(Dictionary<SectorAddress, List<IsoSectorCandidate>> candidates, SectorAddress address, DecodedSector sector, int revolution)
    {
        if (!candidates.TryGetValue(address, out var list)) candidates[address] = list = [];
        list.Add(new(sector, revolution));
    }

    private sealed record CacheEntry(CaptureFingerprint Fingerprint, Lazy<Task<IsoSectorCandidateSet>> Operation);
    private readonly record struct CaptureFingerprint(string Path, long Length, DateTime LastWriteTimeUtc);
}
