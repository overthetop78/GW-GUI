using GWGUI.Emulation;

namespace GWGUI.App.Contracts.Emulation.Configurations;

internal sealed record EmulationConfigurationListItem(
    IEmulationModule Module,
    IEmulationConfiguration Configuration,
    string DisplayName);
