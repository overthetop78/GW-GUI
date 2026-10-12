using GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Constants;

namespace GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Factories;

internal sealed class BeetleWonderSwanMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettings
{
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration) => GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Functions.SettingsDescriptionFunctions.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions() => GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Dictionaries.BeetleWonderSwanOptionCatalog.All;
}
