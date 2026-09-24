using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Contracts;

/// <summary>One explicit row in the media-content recognition catalog.</summary>
public sealed record MediaContentRecognitionRule(
    MediaFileSystemFamily? Family,
    MediaContentRecognitionPriority Priority,
    string Extension,
    IReadOnlyList<MediaContentSignature> Signatures,
    MediaContentCategory Category,
    MediaTextEncoding TextEncoding,
    MediaExecutionKind ExecutionKind,
    MediaPreviewKind PreviewKind,
    IReadOnlyList<int>? ContentLengths = null);
