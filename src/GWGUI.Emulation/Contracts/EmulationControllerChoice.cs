using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationControllerChoice(
    string Id,
    string DisplayResourceKey,
    string? InvariantDisplayValue = null,
    IReadOnlyList<InputBindingDefinition>? BindingDefinitions = null,
    IReadOnlyList<string>? CompatibleVisualIds = null,
    string? DefaultVisualId = null,
    IReadOnlyDictionary<EmulationControllerVisualControl, string>? VisualCommandIds = null);
