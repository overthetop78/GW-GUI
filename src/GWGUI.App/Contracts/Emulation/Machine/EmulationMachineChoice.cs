using GWGUI.Emulation;

namespace GWGUI.App.Contracts.Emulation.Machine;

internal sealed record EmulationMachineChoice(EmulationMachineDefinition Definition, string DisplayName)
{
    public override string ToString() => DisplayName;
}
