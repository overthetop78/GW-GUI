namespace GWGUI.Emulation.Contracts;

public sealed record EmulationEmulatorDefinition(
    string Id,
    string DisplayName,
    string DescriptionResourceKey,
    IReadOnlySet<string> MachineIds);
