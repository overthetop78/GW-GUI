namespace GWGUI.App.Input;

internal enum EmulationShortcutMatchKind
{
    None,
    Global,
    ReservedForGlobal
}

internal sealed record EmulationShortcutMatch(EmulationShortcutMatchKind Kind, string? Action = null,
    bool ShouldExecute = false);
