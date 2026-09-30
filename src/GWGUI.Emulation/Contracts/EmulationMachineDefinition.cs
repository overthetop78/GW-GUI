namespace GWGUI.Emulation.Contracts;

public sealed record EmulationMachineDefinition(
    string Id,
    string DisplayResourceKey,
    string? ImageResourceName = null);
