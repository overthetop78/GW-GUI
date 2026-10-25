using GWGUI.Emulation.Bandai.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Bandai.Emulators.TamaLibretro.Constants;

namespace GWGUI.Emulation.Bandai.Emulators.TamaLibretro.Factories;

internal sealed class TamaLibretroMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettings
{
    public override IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => GWGUI.Emulation.Bandai.Emulators.TamaLibretro.Functions.SettingsDescriptionFunctions.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => GWGUI.Emulation.Bandai.Emulators.TamaLibretro.Dictionaries.TamaLibretroOptionCatalog.All;
}
