using GWGUI.Emulation.Apple.Emulators.MiniVmac.Constants;

namespace GWGUI.Emulation.Apple.Emulators.MiniVmac.Factories;

internal sealed class MiniVmacMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettings
{
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.SettingsDescriptionFunctions.Create(configuration);
}
