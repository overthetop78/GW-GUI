using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Writing;

/// <summary>Validates a media write request and delegates it to the selected format writer.</summary>
public sealed class MediaImageWritingService
{
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
        var produced = await writer.WriteAsync(document, outputPath, targetFormatId, cancellationToken).ConfigureAwait(false);
        if (produced.Count == 0) throw new InvalidDataException($"Media image writer '{writer.Id}' did not report any produced file.");
        if (produced.Any(string.IsNullOrWhiteSpace)) throw new InvalidDataException($"Media image writer '{writer.Id}' reported an empty output path.");
        return produced.ToArray();
    }
}
