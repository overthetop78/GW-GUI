using GWGUI.Emulation;

namespace GWGUI.App.Controls;

internal sealed record EmulationSettingsChoiceView(EmulationSettingsChoice Choice, string DisplayName)
{
    public override string ToString() => DisplayName;
}
