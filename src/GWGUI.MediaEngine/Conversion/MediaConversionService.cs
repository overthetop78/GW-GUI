using GWGUI.MediaEngine.Writing;
using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Conversion;

/// <summary>Coordinates an in-memory representation conversion and the target image writer.</summary>
public sealed class MediaConversionService
{
    private readonly MediaRepresentationConverterRegistry converters;
    private readonly MediaImageWriterRegistry writers;
    private readonly MediaImageWritingService writingService;

    public MediaConversionService(
        MediaRepresentationConverterRegistry converters,
        MediaImageWriterRegistry writers,
        MediaImageWritingService writingService)
    {
        ArgumentNullException.ThrowIfNull(converters);
        ArgumentNullException.ThrowIfNull(writers);
        ArgumentNullException.ThrowIfNull(writingService);
        this.converters = converters;
        this.writers = writers;
        this.writingService = writingService;
    }

    public IReadOnlyList<MediaConversionDestination> GetAvailableDestinations(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return writers.Writers
            .SelectMany(writer => writer.FormatIds.SelectMany(formatId =>
                writer.ProducedFileExtensions.Select(extension => (Writer: writer, FormatId: formatId, Extension: extension))))
            .Where(candidate => candidate.Writer.CanWrite(document, candidate.FormatId, candidate.Extension))
            .Select(candidate => new MediaConversionDestination(
                candidate.FormatId,
                candidate.Extension,
                candidate.Writer.Id,
                candidate.Writer.ProducesMultipleFiles))
            .Distinct()
            .OrderBy(destination => destination.FormatId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(destination => destination.Extension, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<MediaConversionResult> ConvertAsync(
        MediaConversionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var extension = Path.GetExtension(request.OutputPath);
        if (string.IsNullOrWhiteSpace(extension))
            throw new ArgumentException("The media output path requires a file extension.", nameof(request));

        var document = request.Source;
        var diagnostics = new List<string>(document.Diagnostics);
        var losses = new List<string>();

        if (!writers.CanWrite(document, request.TargetFormatId, extension))
        {
            var targetRepresentationKinds = writers
                .FindTargetCandidates(request.TargetFormatId, extension)
                .SelectMany(writer => writer.RepresentationKinds)
                .Distinct()
                .ToArray();
            if (targetRepresentationKinds.Length == 0)
                throw new NotSupportedException(
                    $"No media image writer is registered for target '{request.TargetFormatId}' and extension '{extension}'.");

            MediaRepresentationConversionResult? conversion = null;
            foreach (var targetRepresentationKind in targetRepresentationKinds)
            {
                var converter = converters.Resolve(document, request.TargetFormatId, targetRepresentationKind);
                if (converter is null) continue;

                conversion = await converter.ConvertAsync(
                    document,
                    request.TargetFormatId,
                    targetRepresentationKind,
                    request.Options,
                    cancellationToken).ConfigureAwait(false);
                if (writers.CanWrite(conversion.Document, request.TargetFormatId, extension)) break;
                conversion = null;
            }

            if (conversion is null)
                throw new NotSupportedException(
                    $"No representation converter can prepare '{document.Representation.RepresentationKind}' for target '{request.TargetFormatId}' and extension '{extension}'.");

            document = conversion.Document;
            diagnostics.AddRange(conversion.Diagnostics);
            diagnostics.AddRange(document.Diagnostics);
            losses.AddRange(conversion.Losses);
        }

        var producedFiles = await writingService.WriteAsync(
            document,
            request.OutputPath,
            request.TargetFormatId,
            cancellationToken).ConfigureAwait(false);

        return new MediaConversionResult(
            producedFiles,
            diagnostics.Distinct(StringComparer.Ordinal).ToArray(),
            losses.Distinct(StringComparer.Ordinal).ToArray());
    }
}
