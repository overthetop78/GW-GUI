using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Interfaces.Writing;

namespace GWGUI.MediaEngine.Images.Writing;

/// <summary>Shares capability validation and delegation for typed media image writer adapters.</summary>
internal abstract class MediaImageWriterAdapter<TImage> : IMediaImageWriter
    where TImage : class
{
    private readonly Func<TImage, MediaImageDocument, string, string, bool> canWrite;
    private readonly Func<TImage, MediaImageDocument, string, string, CancellationToken, Task<IReadOnlyList<string>>> write;

    protected MediaImageWriterAdapter(
        string id,
        MediaRepresentationKind representationKind,
        IEnumerable<string> formatIds,
        IEnumerable<string> producedFileExtensions,
        Func<TImage, MediaImageDocument, string, string, bool> canWrite,
        Func<TImage, MediaImageDocument, string, string, CancellationToken, Task<IReadOnlyList<string>>> write,
        bool producesMultipleFiles)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(formatIds);
        ArgumentNullException.ThrowIfNull(producedFileExtensions);
        ArgumentNullException.ThrowIfNull(canWrite);
        ArgumentNullException.ThrowIfNull(write);
        var formats = formatIds.ToArray();
        var extensions = producedFileExtensions.ToArray();
        if (formats.Length == 0 || formats.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Target format identifiers cannot be empty.", nameof(formatIds));
        if (extensions.Length == 0 || extensions.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Output extensions cannot be empty.", nameof(producedFileExtensions));

        Id = id;
        RepresentationKinds = new[] { representationKind }.ToFrozenSet();
        FormatIds = formats.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        ProducedFileExtensions = extensions
            .Select(MediaFileExtensionFunctions.Normalize)
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        this.canWrite = canWrite;
        this.write = write;
        ProducesMultipleFiles = producesMultipleFiles;
    }

    public string Id { get; }

    public IReadOnlySet<string> FormatIds { get; }

    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds { get; }

    public IReadOnlySet<string> ProducedFileExtensions { get; }

    public bool ProducesMultipleFiles { get; }

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        var extension = MediaFileExtensionFunctions.Normalize(targetExtension);
        return TryGetImage(document, out var image) &&
               FormatIds.Contains(targetFormatId) &&
               ProducedFileExtensions.Contains(extension) &&
               canWrite(image, document, targetFormatId, extension);
    }

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        if (!TryGetImage(document, out var image))
            throw new NotSupportedException($"Writer '{Id}' cannot extract its required media representation.");
        var extension = Path.GetExtension(outputPath);
        if (!CanWrite(document, targetFormatId, extension))
            throw new NotSupportedException(
                $"Writer '{Id}' does not accept target '{targetFormatId}' with extension '{extension}'.");
        var produced = await write(
            image,
            document,
            outputPath,
            targetFormatId,
            cancellationToken).ConfigureAwait(false);
        if (produced.Count == 0 || produced.Any(string.IsNullOrWhiteSpace))
            throw new InvalidDataException($"Writer '{Id}' did not report valid produced files.");
        return produced.ToArray();
    }

    protected abstract bool TryGetImage(MediaImageDocument document, out TImage image);
}
