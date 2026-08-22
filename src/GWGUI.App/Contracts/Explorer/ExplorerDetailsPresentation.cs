using GWGUI.App.Enums.Explorer;
namespace GWGUI.App.Contracts.Explorer;

public sealed record ExplorerDetailsPresentation(
    string Title,
    ExplorerIconCategory IconCategory,
    IReadOnlyList<ExplorerDetailRow> Rows,
    bool IsSyntheticTitle = false);
