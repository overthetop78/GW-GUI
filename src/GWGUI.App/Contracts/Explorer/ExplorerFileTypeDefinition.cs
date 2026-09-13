using GWGUI.App.Enums.Explorer;

namespace GWGUI.App.Contracts.Explorer;

public sealed record ExplorerFileTypeDefinition(
    ExplorerFileSystemFamily? Family,
    string Extension,
    ExplorerFileCategory Category,
    string TypeResourceKey,
    ExplorerExecutionKind ExecutionKind,
    ExplorerIconCategory IconCategory,
    ExplorerContentFormat ContentFormat,
    ExplorerTextEncoding TextEncoding,
    ExplorerPreviewKind PreviewKind);
