using GWGUI.Infrastructure.Hardware.Greaseweazle;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Exploration;

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
        var acquisitionProgress = new AcquisitionProgressAdapter(progress);
        var acquisition = await acquisitionService.AcquireAsync(options, acquisitionProgress, cancellationToken).ConfigureAwait(false);

        progress?.Report(new(PhysicalDiskReadStage.Saving, options.Tracks.Count, options.Tracks.Count));
        await writer.WriteAsync(outputPath, acquisition.Image, cancellationToken).ConfigureAwait(false);

        var diagnostics = DecodeTracks(acquisition.Image, progress, cancellationToken);
        progress?.Report(new(PhysicalDiskReadStage.Exploring, options.Tracks.Count, options.Tracks.Count));
        var document = await explorer.ExploreAsync(outputPath, cancellationToken: cancellationToken).ConfigureAwait(false);
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
            progress?.Report(new(PhysicalDiskReadStage.Decoding, index + 1, image.Tracks.Count, track.Cylinder, track.Head));
        }
        return diagnostics;
    }

    private sealed class AcquisitionProgressAdapter(IProgress<PhysicalDiskReadOperationProgress>? progress) : IProgress<PhysicalDiskReadProgress>
    {
        public void Report(PhysicalDiskReadProgress value) => progress?.Report(new(
            PhysicalDiskReadStage.Acquiring,
            value.CompletedTracks,
            value.TotalTracks,
            value.Cylinder,
            value.Head,
            value.Attempt));
    }
}
