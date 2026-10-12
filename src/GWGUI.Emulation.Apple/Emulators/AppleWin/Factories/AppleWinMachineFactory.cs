using GWGUI.Emulation.Apple.Emulators.AppleWin.Constants;

namespace GWGUI.Emulation.Apple.Emulators.AppleWin.Factories;

internal sealed class AppleWinMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettings
{
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration) => GWGUI.Emulation.Apple.Emulators.AppleWin.Functions.SettingsDescriptionFunctions.Create(configuration);
}
