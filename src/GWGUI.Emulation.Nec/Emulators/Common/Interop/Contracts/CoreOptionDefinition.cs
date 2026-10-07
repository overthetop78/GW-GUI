namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;

internal sealed record CoreOptionChoice(string Value, string Label, bool IsInvariant = false);

internal sealed record CoreOptionDefinition(string Key, string LabelResourceKey,
    string DefaultValue, EmulationMachineTab Tab, IReadOnlyList<CoreOptionChoice> Choices);
