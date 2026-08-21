namespace GWGUI.Emulation;

public sealed record InputBindingDefinition(
    string Id,
    string DisplayResourceKey,
    string DefaultBinding,
    string? InvariantDisplayValue = null);
