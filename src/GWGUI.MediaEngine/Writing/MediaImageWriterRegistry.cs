using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Interfaces.Writing;

namespace GWGUI.MediaEngine.Writing;

/// <summary>Selects media image writers by declared target and representation capabilities.</summary>
public sealed class MediaImageWriterRegistry
{
    private readonly IReadOnlyList<IMediaImageWriter> writers;

    public MediaImageWriterRegistry(IReadOnlyList<IMediaImageWriter> writers)
    {
        ArgumentNullException.ThrowIfNull(writers);
        if (writers.Any(writer => writer is null)) throw new ArgumentException("A media image writer cannot be null.", nameof(writers));
        var duplicate = writers.GroupBy(writer => writer.Id, StringComparer.OrdinalIgnoreCase).FirstOrDefault(group => group.Skip(1).Any());
        if (duplicate is not null) throw new ArgumentException($"Media image writer identifier '{duplicate.Key}' is registered more than once.", nameof(writers));
        this.writers = writers.ToArray();
    }

    public IReadOnlyList<IMediaImageWriter> Writers => writers;

    public IReadOnlyList<IMediaImageWriter> FindTargetCandidates(string targetFormatId, string targetExtension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFormatId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetExtension);
        var extension = MediaFileExtensionFunctions.Normalize(targetExtension);
        return writers.Where(writer =>
                writer.FormatIds.Contains(targetFormatId, StringComparer.OrdinalIgnoreCase) &&
                writer.ProducedFileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            .ToArray();
    }

    public IMediaImageWriter? Resolve(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFormatId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetExtension);
        return FindTargetCandidates(targetFormatId, targetExtension).FirstOrDefault(writer =>
            writer.RepresentationKinds.Contains(document.Representation.RepresentationKind) &&
            writer.CanWrite(document, targetFormatId, MediaFileExtensionFunctions.Normalize(targetExtension)));
    }

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension) =>
        Resolve(document, targetFormatId, targetExtension) is not null;
}
