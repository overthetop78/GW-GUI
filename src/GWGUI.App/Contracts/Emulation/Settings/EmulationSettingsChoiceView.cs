using GWGUI.Emulation;

namespace GWGUI.App.Contracts.Emulation.Settings;

internal sealed record EmulationSettingsChoiceView(EmulationSettingsChoice Choice, string DisplayName)
{
    public override string ToString() => DisplayName;
}
