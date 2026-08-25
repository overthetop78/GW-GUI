namespace GWGUI.Emulation.Contracts;

public sealed record EmulationControllerChoice(
    string Id,
    string DisplayResourceKey,
    string? InvariantDisplayValue = null,
    IReadOnlyList<InputBindingDefinition>? BindingDefinitions = null);
