using GWGUI.Emulation.Apple.Emulators.AppleWin.Constants;

namespace GWGUI.Emulation.Apple.Emulators.AppleWin.Factories;

internal sealed class AppleWinMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettings, IEmulatorConfiguration
{
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration) => GWGUI.Emulation.Apple.Emulators.AppleWin.Functions.SettingsDescriptionFunctions.Create(configuration);
    public void InitializeOptions(MachineConfiguration configuration, IDictionary<string, string> options) => GWGUI.Emulation.Apple.Emulators.AppleWin.Functions.ConfigurationFunctions.InitializeOptions(configuration, options);
    public void ValidateConfiguration(MachineConfiguration configuration) => GWGUI.Emulation.Apple.Emulators.AppleWin.Functions.ConfigurationFunctions.ValidateConfiguration(configuration);
}
