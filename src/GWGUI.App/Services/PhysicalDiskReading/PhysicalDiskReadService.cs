using GWGUI.App.Constants.Services.PhysicalDiskReading;
using GWGUI.App.Contracts.Services.PhysicalDiskReading;
using GWGUI.App.Enums.Services.PhysicalDiskReading;
using GWGUI.Infrastructure.Hardware.Greaseweazle;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Exploration.Contracts;

namespace GWGUI.App.Services.PhysicalDiskReading;

public sealed class PhysicalDiskReadService(
    PhysicalDiskFluxAcquisitionService acquisitionService,
    IScpWriter writer,
    FluxDecoderRegistry decoders,
    DiskImageExplorer explorer)
{
    public static PhysicalDiskReadService CreateDefault(IGreaseweazleReadDevice device) => new(
        new PhysicalDiskFluxAcquisitionService(device),
        new ScpWriter(),
        new FluxDecoderRegistry(),
        DiskImageExplorer.CreateDefault());

    public async Task<PhysicalDiskReadResult> ReadAsync(
        PhysicalDiskReadOptions options,
        string outputPath,
        IProgress<PhysicalDiskReadOperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        var acquisitionProgress = new AcquisitionProgressAdapter(progress, options.Tracks);
        var acquisition = await acquisitionService.AcquireAsync(options, acquisitionProgress, cancellationToken).ConfigureAwait(false);

        progress?.Report(new(
            PhysicalDiskReadStage.Saving,
            options.Tracks.Count,
            options.Tracks.Count,
            tracks: options.Tracks));
        await writer.WriteAsync(outputPath, acquisition.Image, cancellationToken).ConfigureAwait(false);

        var diagnostics = DecodeTracks(acquisition.Image, progress, cancellationToken);
        progress?.Report(new(
            PhysicalDiskReadStage.Exploring,
            options.Tracks.Count,
            options.Tracks.Count,
            tracks: options.Tracks));
        var document = await explorer.ExploreScpAsync(outputPath, acquisition.Image, cancellationToken).ConfigureAwait(false);
        return new(outputPath, acquisition, diagnostics, document);
    }

    private IReadOnlyList<PhysicalDiskTrackDiagnostic> DecodeTracks(
        ScpImage image,
        IProgress<PhysicalDiskReadOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        var diagnostics = new List<PhysicalDiskTrackDiagnostic>(image.Tracks.Count);
        for (var index = 0; index < image.Tracks.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var track = image.Tracks[index];
            var revolutions = track.Revolutions.Select(revolution => revolution.Flux).ToArray();
            var results = decoders.Decoders.Select(decoder => decoders.DecodeBest(revolutions, decoder.Id)!).ToArray();
            var best = decoders.DecodeBest(revolutions)!;
            diagnostics.Add(new(track.Cylinder, track.Head, best, results));
            progress?.Report(new(
                PhysicalDiskReadStage.Decoding,
                index + 1,
                image.Tracks.Count,
                track.Cylinder,
                track.Head,
                tracks: image.Tracks.Select(item => new PhysicalDiskTrackAddress(
                    item.Cylinder,
                    item.Head,
                    item.Cylinder,
                    item.Head)).ToArray(),
                acquiredTrack: DiskTrackContractMapper.FromScpTrack(
                    track,
                    image.Header.ResolutionNanoseconds)));
        }
        return diagnostics;
    }

    private sealed class AcquisitionProgressAdapter(
        IProgress<PhysicalDiskReadOperationProgress>? progress,
        IReadOnlyList<PhysicalDiskTrackAddress> tracks) : IProgress<PhysicalDiskReadProgress>
    {
        public void Report(PhysicalDiskReadProgress value)
        {
            IPiste? acquiredTrack = null;
            if (value.CapturedTrack is not null)
            {
                acquiredTrack = DiskTrackContractMapper.FromScpTrack(
                    value.CapturedTrack,
                    ScpFormatConstants.ResolutionStepNanoseconds *
                    (PhysicalDiskReadDefaults.ScpResolution + ScpFormatConstants.ResolutionIndexOffset));
            }

            progress?.Report(new(
                PhysicalDiskReadStage.Acquiring,
                value.CompletedTracks,
                value.TotalTracks,
                value.Cylinder,
                value.Head,
                value.Attempt,
                tracks,
                acquiredTrack));
        }
    }
}
