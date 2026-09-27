using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Flux;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Images.Visualization;

namespace GWGUI.MediaEngine.Images.Writing;

/// <summary>Validates a media write request and delegates it to the selected format writer.</summary>
public sealed class MediaImageWritingService
{
    private static readonly MediaVisualizationProviderRegistry VisualizationProviders =
        MediaVisualizationComposition.CreateDefault().Registry;

    private readonly MediaImageWriterRegistry writers;

    public MediaImageWritingService(MediaImageWriterRegistry writers)
    {
        ArgumentNullException.ThrowIfNull(writers);
        this.writers = writers;
    }

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
        => await WriteAsync(document, outputPath, targetFormatId, null, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        Action<MediaExplorationProgress>? progress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFormatId);
        cancellationToken.ThrowIfCancellationRequested();

        var extension = Path.GetExtension(outputPath);
        if (string.IsNullOrWhiteSpace(extension))
            throw new ArgumentException("The media output path requires a file extension.", nameof(outputPath));
        var writer = writers.Resolve(document, targetFormatId, extension) ?? throw new NotSupportedException(
            $"No media image writer accepts representation '{document.Representation.RepresentationKind}' for target '{targetFormatId}' and extension '{extension}'.");
        await ReportInputUnitsAsync(document, targetFormatId, progress, cancellationToken).ConfigureAwait(false);
        var produced = await writer.WriteAsync(document, outputPath, targetFormatId, cancellationToken).ConfigureAwait(false);
        if (produced.Count == 0) throw new InvalidDataException($"Media image writer '{writer.Id}' did not report any produced file.");
        if (produced.Any(string.IsNullOrWhiteSpace)) throw new InvalidDataException($"Media image writer '{writer.Id}' reported an empty output path.");
        return produced.ToArray();
    }

    private static async Task ReportInputUnitsAsync(
        MediaImageDocument document,
        string targetFormatId,
        Action<MediaExplorationProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (progress is null) return;

        var provider = VisualizationProviders.Resolve(document);
        if (provider is null)
        {
            progress(new(MediaExplorationProgressStage.ReadingMedia, targetFormatId, 95, document.MediaKind));
            return;
        }

        var descriptor = provider.CreateDescriptor(document);
        if (descriptor.ProgressUnit == MediaVisualizationProgressUnit.Track && descriptor.Elements.Count > 0)
        {
            progress(new(
                MediaExplorationProgressStage.ReadingMedia,
                $"Converting c={string.Join(',', descriptor.Elements.Select(element => element.Position).Distinct().Order())}:h={string.Join(',', descriptor.Surfaces.Order())}",
                60,
                document.MediaKind));
        }

        for (var index = 0; index < descriptor.Elements.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var element = descriptor.Elements[index];
            ReadElement(document, element);
            var detail = descriptor.ProgressUnit == MediaVisualizationProgressUnit.Track && element.Surface is int surface
                ? $"T{element.Position}.{surface}"
                : targetFormatId;
            progress(new(
                MediaExplorationProgressStage.ReadingMedia,
                detail,
                60d + 35d * (index + 1) / descriptor.Elements.Count,
                document.MediaKind));
            await Task.Yield();
        }

        if (descriptor.Elements.Count == 0)
            progress(new(MediaExplorationProgressStage.ReadingMedia, targetFormatId, 95, document.MediaKind));
    }

    private static void ReadElement(MediaImageDocument document, MediaVisualizationElement element)
    {
        switch (document.Representation)
        {
            case SectorMediaImageRepresentation sectors when element.Surface is int surface:
                foreach (var block in sectors.Image.AvailableBlocks.Where(block =>
                             block.Address.Cylinder == element.Position && block.Address.Head == surface))
                    _ = block.Data.Count;
                break;
            case FluxMediaImageRepresentation flux when element.Surface is int surface:
                var track = flux.Tracks.FirstOrDefault(track =>
                    track.Cylinder == element.Position && track.Head == surface);
                _ = track?.Bits?.Count ?? track?.Revolutions.Count ?? 0;
                break;
        }
    }
}
