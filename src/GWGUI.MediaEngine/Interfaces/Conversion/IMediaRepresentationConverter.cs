using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Conversion;
using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Interfaces.Conversion;

/// <summary>Transforms an in-memory media representation without writing an output file.</summary>
public interface IMediaRepresentationConverter
{
    string Id { get; }

    IReadOnlySet<MediaRepresentationKind> SourceRepresentationKinds { get; }

    IReadOnlySet<MediaRepresentationKind> TargetRepresentationKinds { get; }

    bool CanConvert(
        MediaImageDocument source,
        string targetFormatId,
        MediaRepresentationKind targetRepresentationKind);

    Task<MediaRepresentationConversionResult> ConvertAsync(
        MediaImageDocument source,
        string targetFormatId,
        MediaRepresentationKind targetRepresentationKind,
        IReadOnlyDictionary<string, string> options,
        CancellationToken cancellationToken = default);
}
