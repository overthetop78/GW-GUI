using GWGUI.App.Enums.Emulation.Shortcuts;
namespace GWGUI.App.Contracts.Emulation.Shortcuts;

internal sealed record EmulationShortcutMatch(
    EmulationShortcutMatchCategory Category,
    string? Action = null,
    bool ShouldExecute = false);
