using System.Collections.ObjectModel;
using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Contains a recognized media image, its representation, volumes, diagnostics, and source-provided metadata.</summary>
public sealed class MediaImageDocument
{
    public MediaImageDocument(
        MediaSourceDescriptor source,
        string formatId,
        MediaKind mediaKind,
        IMediaImageRepresentation representation,
        IReadOnlyList<MediaVolumeDescriptor> volumes,
        IReadOnlyList<string> diagnostics,
        IReadOnlyDictionary<string, string> metadata)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (string.IsNullOrWhiteSpace(formatId)) throw new ArgumentException("A recognized format identifier is required.", nameof(formatId));
        ArgumentNullException.ThrowIfNull(representation);
        ArgumentNullException.ThrowIfNull(volumes);
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentNullException.ThrowIfNull(metadata);

        Source = source;
        FormatId = formatId;
        MediaKind = mediaKind;
        Representation = representation;
        Volumes = new ReadOnlyCollection<MediaVolumeDescriptor>(volumes.ToArray());
        Diagnostics = new ReadOnlyCollection<string>(diagnostics.ToArray());
        Metadata = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(metadata, StringComparer.Ordinal));
    }

    public MediaSourceDescriptor Source { get; }

    public string FormatId { get; }

    public MediaKind MediaKind { get; }

    public IMediaImageRepresentation Representation { get; }

    public IReadOnlyList<MediaVolumeDescriptor> Volumes { get; }

    public IReadOnlyList<string> Diagnostics { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }
}
