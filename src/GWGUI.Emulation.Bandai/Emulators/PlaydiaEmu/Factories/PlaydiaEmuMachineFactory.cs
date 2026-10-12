using GWGUI.Emulation.Bandai.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Bandai.Emulators.PlaydiaEmu.Constants;

namespace GWGUI.Emulation.Bandai.Emulators.PlaydiaEmu.Factories;

internal sealed class PlaydiaEmuMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettings
{
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration) => GWGUI.Emulation.Bandai.Emulators.PlaydiaEmu.Functions.SettingsDescriptionFunctions.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions() => GWGUI.Emulation.Bandai.Emulators.PlaydiaEmu.Dictionaries.PlaydiaEmuOptionCatalog.All;
}
