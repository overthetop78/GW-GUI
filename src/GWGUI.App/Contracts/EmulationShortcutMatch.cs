namespace GWGUI.App.Input;

internal sealed record EmulationShortcutMatch(
    EmulationShortcutMatchCategory Category,
    string? Action = null,
    bool ShouldExecute = false);
