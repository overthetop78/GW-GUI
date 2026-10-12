using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Coleco.Emulators.GearColeco.Constants;
using Settings = GWGUI.Emulation.Coleco.Emulators.GearColeco.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Coleco.Emulators.GearColeco.Factories;

internal sealed class GearColecoMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettings, IEmulatorConfigurationValidator
{
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions() => Settings.GetOptions();
    public void ValidateConfiguration(MachineConfiguration configuration) => Settings.Validate(configuration);
}
