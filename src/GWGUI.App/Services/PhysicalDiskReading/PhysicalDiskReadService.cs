using GWGUI.App.Constants.Services.PhysicalDiskReading;
using GWGUI.App.Contracts.Services.PhysicalDiskReading;
using GWGUI.App.Enums.Services.PhysicalDiskReading;
using MediaAcquisitionResult = global::GWGUI.MediaEngine.Contracts.MediaAcquisitionResult;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Reading;
using GWGUI.MediaEngine.Images.Reading.Decoding;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Exploration.Contracts;

using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;

namespace GWGUI.App.Services.PhysicalDiskReading;

public sealed class PhysicalDiskReadService(
    FloppyFluxAcquisitionService acquisitionService,
    IScpWriter writer,
    FluxDecoderRegistry decoders,
    DiskImageExplorer explorer)
{
    public static PhysicalDiskReadService CreateDefault() => new(
        new FloppyFluxAcquisitionService(),
        new ScpWriter(),
        new FluxDecoderRegistry(),
        DiskImageExplorer.CreateDefault());

    public async Task<PhysicalDiskReadResult> ReadAsync(
        MediaAcquisitionResult acquisition,
        string outputPath,
        IProgress<PhysicalDiskReadOperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(acquisition);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        var image = acquisitionService.CreateScpImage(acquisition);
        var tracks = image.Tracks.Select(track => new PhysicalDiskTrackAddress(
            track.Cylinder,
            track.Head,
            track.Cylinder,
            track.Head)).ToArray();

        progress?.Report(new(
            PhysicalDiskReadStage.Saving,
            tracks.Length,
            tracks.Length,
            tracks: tracks));
        await writer.WriteAsync(outputPath, image, cancellationToken).ConfigureAwait(false);

        var diagnostics = DecodeTracks(image, progress, cancellationToken);
        progress?.Report(new(
            PhysicalDiskReadStage.Exploring,
            tracks.Length,
            tracks.Length,
            tracks: tracks));
        var document = await explorer.ExploreScpAsync(outputPath, image, cancellationToken).ConfigureAwait(false);
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
                acquiredTrack: ScpTrackContractMapper.FromScpTrack(
                    track,
                    image.Header.ResolutionNanoseconds)));
        }
        return diagnostics;
    }

}
