namespace GWGUI.Emulation;

public sealed record EmulationControllerChoice(
    string Id,
    string DisplayResourceKey,
    string? InvariantDisplayValue = null,
    IReadOnlyList<InputBindingDefinition>? BindingDefinitions = null);
