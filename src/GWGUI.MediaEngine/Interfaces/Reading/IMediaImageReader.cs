using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Recognition;

namespace GWGUI.MediaEngine.Interfaces.Reading;

/// <summary>Describes and reads one media image format without application or rendering logic.</summary>
public interface IMediaImageReader
{
    IReadOnlySet<string> FormatIds { get; }

    IReadOnlySet<string> Extensions { get; }

    IReadOnlyList<ReadOnlyMemory<byte>> Signatures { get; }

    IReadOnlySet<string> AssociatedFileExtensions { get; }

    IReadOnlySet<MediaKind> MediaKinds { get; }

    IReadOnlySet<MediaRepresentationKind> RepresentationKinds { get; }

    bool SupportsFormatId(string formatId);

    ValueTask<bool> CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken);

    Task<MediaImageDocument> ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken);
}
