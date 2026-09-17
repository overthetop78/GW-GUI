using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Interfaces;

public interface IMediaContentClassifier
{
    MediaContentTypeDefinition? Classify(
        string name,
        MediaEntryKind kind,
        string? nativeTypeId,
        IReadOnlyList<byte>? content,
        IReadOnlyDictionary<string, string> metadata,
        MediaFileSystemFamily family);
}
