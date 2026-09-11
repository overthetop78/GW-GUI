using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Writing;

namespace GWGUI.MediaEngine.Conversion.Optical;

/// <summary>Offers only optical conversions whose registered writer preserves the source document.</summary>
public sealed class OpticalImageConversionService
{
    private readonly MediaImageReadingService reading;
    private readonly MediaImageWriterRegistry writers;
    private readonly MediaImageWritingService writing;

    public OpticalImageConversionService(
        MediaImageReadingService reading,
        MediaImageWriterRegistry writers,
        MediaImageWritingService writing)
    {
        ArgumentNullException.ThrowIfNull(reading);
        ArgumentNullException.ThrowIfNull(writers);
        ArgumentNullException.ThrowIfNull(writing);
        this.reading = reading;
        this.writers = writers;
        this.writing = writing;
    }

    public IReadOnlyList<MediaConversionDestination> GetAvailableDestinations(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.MediaKind != MediaKind.Optical) return [];
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

    public async Task<IReadOnlyList<string>> ConvertAsync(
        string sourcePath,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFormatId);
        var document = await reading.ReadAsync(
            new MediaSourceDescriptor(sourcePath, []),
            cancellationToken).ConfigureAwait(false);
        if (document.MediaKind != MediaKind.Optical)
            throw new NotSupportedException("The source is not an optical media image.");
        var extension = Path.GetExtension(outputPath);
        if (writers.Resolve(document, targetFormatId, extension) is null)
            throw new NotSupportedException(
                "The requested optical destination would lose unsupported track, sector, session, layer, pregap, postgap, or subchannel information.");
        return await writing.WriteAsync(document, outputPath, targetFormatId, cancellationToken).ConfigureAwait(false);
    }
}
