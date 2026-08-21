using GWGUI.App.Enums;

namespace GWGUI.App.Contracts;

public sealed record ExplorerDetailsPresentation(
    string Title,
    ExplorerIconCategory IconCategory,
    IReadOnlyList<ExplorerDetailRow> Rows,
    bool IsSyntheticTitle = false);
