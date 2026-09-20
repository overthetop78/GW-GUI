using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Interfaces;

public interface IMediaContentClassifier
{
    MediaContentTypeDefinition? Classify(
        string extension,
        MediaEntryKind kind,
        string? nativeTypeId,
        string comment,
        bool? dataValid,
        IReadOnlyList<byte>? content,
        IReadOnlyDictionary<string, string> metadata,
        MediaFileSystemFamily family);
}
