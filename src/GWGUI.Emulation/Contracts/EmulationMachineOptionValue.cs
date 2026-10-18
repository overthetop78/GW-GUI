namespace GWGUI.Emulation.Contracts;

public sealed record EmulationMachineOptionValue(string Value, string Label, IReadOnlySet<string>? MachineIds = null);
