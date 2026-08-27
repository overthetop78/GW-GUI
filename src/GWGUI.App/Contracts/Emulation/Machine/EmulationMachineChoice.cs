using GWGUI.Emulation;

namespace GWGUI.App.Contracts.Emulation.Machine;

internal sealed record EmulationMachineChoice(EmulationMachineDefinition Definition, string DisplayName,
    bool HasSavedConfiguration = false)
{
    public override string ToString() => DisplayName;
}
