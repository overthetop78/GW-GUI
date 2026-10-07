using GWGUI.Emulation;
using System.Windows.Media;

namespace GWGUI.App.Contracts.Emulation.Machine;

internal sealed record EmulationMachineChoice(EmulationMachineDefinition Definition, string DisplayName,
    bool HasSavedConfiguration = false, ImageSource? Image = null)
{
    public override string ToString() => DisplayName;
}
