namespace GWGUI.Emulation;

public sealed record EmulationConfigurationSummary(
    string MachineDisplayResourceKey,
    IReadOnlyList<string> Details);
