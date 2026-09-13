using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.Formats.Floppy.Scp;
using GWGUI.MediaEngine.Reconstruction.Scp;

namespace GWGUI.MediaEngine.Recognition.Scp;

/// <summary>Sonde rapidement des pistes réparties avant de lancer les reconstructions complètes.</summary>
internal sealed class ScpFamilyProbe(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    public Task<IReadOnlySet<ScpFormatFamily>> DetectAsync(string path, CancellationToken cancellationToken) =>
        DetectAsync(path, null, cancellationToken);

    public async Task<IReadOnlySet<ScpFormatFamily>> DetectAsync(
        string path,
        IProgress<ScpExplorationProgress>? progress,
        CancellationToken cancellationToken)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var samples = ScpTrackSampler.Sample(scp.Tracks);
        if (samples.Count == 0) return new HashSet<ScpFormatFamily>();

        var found = new HashSet<ScpFormatFamily>();
        for (var index = 0; index < ScpFamilyProbeCatalog.Definitions.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var definition = ScpFamilyProbeCatalog.Definitions[index];
            progress?.Report(new(
                ScpExplorationProgressKind.FormatProbeStarted,
                definition.DisplayName,
                index,
                ScpFamilyProbeCatalog.Definitions.Count));

            var recognized = await ProbeFamilyAsync(definition, samples, cancellationToken).ConfigureAwait(false);
            if (recognized) found.Add(definition.Family);

            progress?.Report(new(
                ScpExplorationProgressKind.FormatProbeCompleted,
                definition.DisplayName,
                index + 1,
                ScpFamilyProbeCatalog.Definitions.Count,
                recognized));
        }

        return found;
    }

    private async Task<bool> ProbeFamilyAsync(
        ScpFamilyProbeDefinition definition,
        IReadOnlyList<ScpTrack> samples,
        CancellationToken cancellationToken)
    {
        var recognized = 0;
        await Parallel.ForEachAsync(
            samples,
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount)
            },
            (track, token) =>
            {
                if (Volatile.Read(ref recognized) != 0) return ValueTask.CompletedTask;
                token.ThrowIfCancellationRequested();
                var flux = ScpTrackDecodeWindowFactory.Primary(track).Flux;
                foreach (var decoderId in definition.DecoderIds)
                {
                    var result = decoders.Decode(decoderId, flux);
                    if (!HasValidSector(result)) continue;
                    Interlocked.Exchange(ref recognized, 1);
                    break;
                }
                return ValueTask.CompletedTask;
            }).ConfigureAwait(false);
        return recognized != 0;
    }

    private static bool HasValidSector(FluxDecodeResult result) =>
        result.Sectors.Any(sector => sector.Data is not null && sector.IntegrityValid == true);
}
