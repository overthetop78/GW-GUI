using GWGUI.Emulation;

namespace GWGUI.App.Controls;

internal sealed record EmulationMachineChoice(EmulationMachineDefinition Definition, string DisplayName)
{
    public override string ToString() => DisplayName;
}
