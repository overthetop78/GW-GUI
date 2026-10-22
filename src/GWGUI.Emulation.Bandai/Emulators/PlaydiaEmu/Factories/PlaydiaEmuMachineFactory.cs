using GWGUI.Emulation.Bandai.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Bandai.Emulators.PlaydiaEmu.Constants;

namespace GWGUI.Emulation.Bandai.Emulators.PlaydiaEmu.Factories;

internal sealed class PlaydiaEmuMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettingsAdapter
{
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => GWGUI.Emulation.Bandai.Emulators.PlaydiaEmu.Functions.SettingsDescriptionFunctions.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => GWGUI.Emulation.Bandai.Emulators.PlaydiaEmu.Dictionaries.PlaydiaEmuOptionCatalog.All;
}
