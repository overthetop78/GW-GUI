using GWGUI.Emulation;

namespace GWGUI.App.Controls;

internal sealed record EmulationConfigurationListItem(
    IEmulationModule Module,
    IEmulationConfiguration Configuration,
    string DisplayName);
