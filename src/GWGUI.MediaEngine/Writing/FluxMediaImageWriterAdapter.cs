using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.MediaEngine.Writing;

/// <summary>Extracts a protected-track image for an existing writer using the common adapter behavior.</summary>
internal sealed class FluxMediaImageWriterAdapter : MediaImageWriterAdapter<ProtectedTrackImage>
{
    public FluxMediaImageWriterAdapter(
        string id,
        IEnumerable<string> formatIds,
        IEnumerable<string> producedFileExtensions,
        Func<ProtectedTrackImage, string, string, bool> canWrite,
        Func<ProtectedTrackImage, IReadOnlyDictionary<string, string>, string, string, CancellationToken, Task<IReadOnlyList<string>>> write,
        bool producesMultipleFiles = false)
        : base(
            id,
            MediaRepresentationKind.Flux,
            formatIds,
            producedFileExtensions,
            WrapCanWrite(canWrite),
            WrapWrite(write),
            producesMultipleFiles)
    {
    }

    protected override bool TryGetImage(MediaImageDocument document, out ProtectedTrackImage image)
    {
        if (document.Representation is FluxMediaImageRepresentation flux)
        {
            image = flux.Image;
            return true;
        }

        image = null!;
        return false;
    }

    private static Func<ProtectedTrackImage, MediaImageDocument, string, string, bool> WrapCanWrite(
        Func<ProtectedTrackImage, string, string, bool> canWrite)
    {
        ArgumentNullException.ThrowIfNull(canWrite);
        return (image, _, formatId, extension) => canWrite(image, formatId, extension);
    }

    private static Func<ProtectedTrackImage, MediaImageDocument, string, string, CancellationToken, Task<IReadOnlyList<string>>> WrapWrite(
        Func<ProtectedTrackImage, IReadOnlyDictionary<string, string>, string, string, CancellationToken, Task<IReadOnlyList<string>>> write)
    {
        ArgumentNullException.ThrowIfNull(write);
        return (image, document, path, formatId, token) =>
            write(image, document.Metadata, path, formatId, token);
    }
}
