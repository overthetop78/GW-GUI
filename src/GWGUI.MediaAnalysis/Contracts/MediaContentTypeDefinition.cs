using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Contracts;

public sealed record MediaContentTypeDefinition(
    MediaFileSystemFamily? Family,
    string Extension,
    MediaContentCategory Category,
    string TypeResourceKey,
    MediaExecutionKind ExecutionKind,
    string IconId,
    MediaContentFormat ContentFormat,
    MediaTextEncoding TextEncoding,
    MediaPreviewKind PreviewKind);
