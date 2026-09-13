using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Reconstruction.Scp;

using GWGUI.MediaEngine.Formats.Floppy.Scp;

using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Décode et partage les candidats sectoriels ISO communs aux politiques d'une même capture SCP.</summary>
internal sealed class IsoScpCandidateDecoder(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    private readonly object cacheGate = new();
    private readonly Dictionary<string, CacheEntry> cache = new(StringComparer.Ordinal);

    /// <summary>Retourne les candidats décodés pour la capture et l'ensemble de codecs demandés.</summary>
    public async Task<IsoSectorCandidateSet> DecodeAsync(string path, IReadOnlyList<string> decoderIds, CancellationToken cancellationToken)
        => await DecodeAsync(path, decoderIds, null, cancellationToken).ConfigureAwait(false);

    /// <summary>Décode toutes les pistes et publie chaque piste réellement terminée.</summary>
    public async Task<IsoSectorCandidateSet> DecodeAsync(
        string path,
        IReadOnlyList<string> decoderIds,
        IProgress<GWGUI.MediaEngine.Exploration.Scp.ScpExplorationProgress>? progress,
        CancellationToken cancellationToken)
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
                entry = new(fingerprint, new(() => DecodeCoreAsync(fullPath, decoderIds, progress, cancellationToken), LazyThreadSafetyMode.ExecutionAndPublication));
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

    private async Task<IsoSectorCandidateSet> DecodeCoreAsync(
        string path,
        IReadOnlyList<string> decoderIds,
        IProgress<GWGUI.MediaEngine.Exploration.Scp.ScpExplorationProgress>? progress,
        CancellationToken cancellationToken)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var trackCandidates = new TrackCandidateSet[scp.Tracks.Count];
        var completed = 0;
        var completedRevolutions = 0;
        var totalRevolutions = scp.Tracks.Sum(track => track.Revolutions.Count);
        await Parallel.ForEachAsync(
            Enumerable.Range(0, scp.Tracks.Count),
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount)
            },
            (trackIndex, token) =>
            {
                var track = scp.Tracks[trackIndex];
                trackCandidates[trackIndex] = DecodeTrack(track, decoderIds, token, window =>
                {
                    var revolutionCount = Interlocked.Increment(ref completedRevolutions);
                    progress?.Report(new(
                        GWGUI.MediaEngine.Exploration.Scp.ScpExplorationProgressKind.RevolutionDecoded,
                        $"{track.Cylinder}:{track.Head} · {window.Revolution}/{track.Revolutions.Count}",
                        revolutionCount,
                        totalRevolutions));
                });
                var count = Interlocked.Increment(ref completed);
                progress?.Report(new(
                    GWGUI.MediaEngine.Exploration.Scp.ScpExplorationProgressKind.TrackDecoded,
                    $"{scp.Tracks[trackIndex].Cylinder}:{scp.Tracks[trackIndex].Head}",
                    count,
                    scp.Tracks.Count));
                return ValueTask.CompletedTask;
            }).ConfigureAwait(false);

        var candidates = new Dictionary<SectorAddress, List<IsoSectorCandidate>>();
        var physicalCandidates = new Dictionary<SectorAddress, List<IsoSectorCandidate>>();
        foreach (var track in trackCandidates)
        {
            MergeCandidates(candidates, track.Addressed);
            MergeCandidates(physicalCandidates, track.Physical);
        }

        return new(candidates, physicalCandidates);
    }

    private TrackCandidateSet DecodeTrack(
        ScpTrack track,
        IReadOnlyList<string> decoderIds,
        CancellationToken cancellationToken,
        Action<ScpTrackDecodeWindow> revolutionDecoded)
    {
        var candidates = new Dictionary<SectorAddress, List<IsoSectorCandidate>>();
        var physicalCandidates = new Dictionary<SectorAddress, List<IsoSectorCandidate>>();
        foreach (var window in ScpTrackDecodeWindowFactory.Create(track))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = decoderIds.Select(decoder => decoders.Decode(decoder, window.Flux))
                .OrderByDescending(Score).First();
            foreach (var sector in result.Sectors)
            {
                if (sector.Data is null || sector.Number < 0) continue;
                AddCandidate(physicalCandidates, new(track.Cylinder, track.Head, sector.Number), sector, window.Revolution);
                if (sector.Cylinder != track.Cylinder || sector.Head != track.Head) continue;
                AddCandidate(candidates, new(sector.Cylinder, sector.Head, sector.Number), sector, window.Revolution);
            }
            revolutionDecoded(window);
        }

        return new(candidates, physicalCandidates);
    }

    internal static double Score(FluxDecodeResult result) =>
        result.Sectors.Count(sector => sector.IntegrityValid == true) * IsoScpReconstructionDefinitions.ValidSectorScoreWeight
        + result.Sectors.Count(sector => sector.Data is not null) * IsoScpReconstructionDefinitions.DataSectorScoreWeight
        - result.Sectors.Count(sector => sector.IntegrityValid == false) * IsoScpReconstructionDefinitions.InvalidSectorScorePenalty
        + result.Confidence;

    private static void AddCandidate(Dictionary<SectorAddress, List<IsoSectorCandidate>> candidates, SectorAddress address, DecodedSector sector, int revolution)
    {
        if (!candidates.TryGetValue(address, out var list)) candidates[address] = list = [];
        list.Add(new(sector, revolution));
    }

    private static void MergeCandidates(
        Dictionary<SectorAddress, List<IsoSectorCandidate>> destination,
        IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> source)
    {
        foreach (var (address, sourceCandidates) in source)
        {
            if (!destination.TryGetValue(address, out var destinationCandidates))
                destination[address] = destinationCandidates = [];
            destinationCandidates.AddRange(sourceCandidates);
        }
    }

    private sealed record CacheEntry(CaptureFingerprint Fingerprint, Lazy<Task<IsoSectorCandidateSet>> Operation);
    private readonly record struct CaptureFingerprint(string Path, long Length, DateTime LastWriteTimeUtc);
    private sealed record TrackCandidateSet(
        IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> Addressed,
        IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> Physical);
}
