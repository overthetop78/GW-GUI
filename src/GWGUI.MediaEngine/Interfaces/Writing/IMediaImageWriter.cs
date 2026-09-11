using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Interfaces.Writing;

/// <summary>Writes compatible media representations to one declared target format.</summary>
public interface IMediaImageWriter
{
    string Id { get; }

    IReadOnlySet<string> FormatIds { get; }

    IReadOnlySet<MediaRepresentationKind> RepresentationKinds { get; }

    IReadOnlySet<string> ProducedFileExtensions { get; }

    bool ProducesMultipleFiles { get; }

    bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension);

    Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default);
}
