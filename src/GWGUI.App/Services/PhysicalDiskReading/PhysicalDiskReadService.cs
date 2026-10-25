using GWGUI.App.Constants.Services.PhysicalDiskReading;
using GWGUI.App.Contracts.Services.PhysicalDiskReading;
using GWGUI.App.Enums.Services.PhysicalDiskReading;
using MediaAcquisitionResult = GWGUI.MediaEngine.Contracts.MediaAcquisitionResult;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Reading;
using GWGUI.MediaEngine.Images.Reading.Decoding;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Images.Conversion;
using GWGUI.MediaEngine.Constants;
using System.IO;

using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;

namespace GWGUI.App.Services.PhysicalDiskReading;

public sealed class PhysicalDiskReadService(FloppyFluxAcquisitionService acquisitionService, IScpWriter writer, FluxDecoderRegistry decoders, DiskImageExplorer explorer, MediaConversionService? conversionService = null)
{
    public static PhysicalDiskReadService CreateDefault() => new(new FloppyFluxAcquisitionService(), new ScpWriter(), new FluxDecoderRegistry(), DiskImageExplorer.CreateDefault());

    public async Task<PhysicalDiskReadResult> ReadAsync(MediaAcquisitionResult acquisition, string outputPath, IProgress<PhysicalDiskReadOperationProgress>? progress = null, CancellationToken cancellationToken = default, string? targetFormatId = null)
    {
        ArgumentNullException.ThrowIfNull(acquisition);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        var image = acquisitionService.CreateScpImage(acquisition);
        var tracks = image.Tracks.Select(track => new PhysicalDiskTrackAddress(track.Cylinder, track.Head, track.Cylinder, track.Head)).ToArray();

        progress?.Report(new(PhysicalDiskReadStage.Saving, tracks.Length, tracks.Length, tracks: tracks));
        if (targetFormatId is null)
            await writer.WriteAsync(outputPath, image, cancellationToken).ConfigureAwait(false);
        else
        {
            var source = acquisitionService.CreateDocument(outputPath, image);
            var conversion = conversionService ?? MediaEngineComposition.CreateDefault().ConversionService;
            await conversion.ConvertAsync(new MediaConversionRequest(source, outputPath, targetFormatId), cancellationToken).ConfigureAwait(false);
        }

        var diagnostics = DecodeTracks(image, progress, cancellationToken);
        progress?.Report(new(PhysicalDiskReadStage.Exploring, tracks.Length, tracks.Length, tracks: tracks));
        var isScp = Path.GetExtension(outputPath).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase);
        var explorationFormat = Path.GetExtension(outputPath).Equals(DiskImageFileExtensions.Hfe, StringComparison.OrdinalIgnoreCase)
            ? DiskImageFormatIds.RawHfe : targetFormatId;
        var document = targetFormatId is null || isScp
            ? await explorer.ExploreScpAsync(outputPath, image, cancellationToken).ConfigureAwait(false)
            : await explorer.ExploreAsync(outputPath, explorationFormat, cancellationToken).ConfigureAwait(false);
        return new(outputPath, acquisition, diagnostics, document);
    }

    private IReadOnlyList<PhysicalDiskTrackDiagnostic> DecodeTracks(ScpImage image, IProgress<PhysicalDiskReadOperationProgress>? progress, CancellationToken cancellationToken)
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
                tracks: image.Tracks.Select(item => new PhysicalDiskTrackAddress(item.Cylinder, item.Head, item.Cylinder, item.Head)).ToArray(),
                acquiredTrack: ScpTrackContractMapper.FromScpTrack(track, image.Header.ResolutionNanoseconds)));
        }
        return diagnostics;
    }

}
