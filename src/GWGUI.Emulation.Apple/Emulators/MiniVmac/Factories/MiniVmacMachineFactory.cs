using GWGUI.Emulation.Apple.Emulators.MiniVmac.Constants;

namespace GWGUI.Emulation.Apple.Emulators.MiniVmac.Factories;

internal sealed class MiniVmacMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettings, IEmulatorConfiguration, IEmulatorRuntimePolicy
{
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.SettingsDescriptionFunctions.Create(configuration);
    public void InitializeOptions(MachineConfiguration configuration, IDictionary<string, string> options) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.ConfigurationFunctions.InitializeOptions(configuration, options);
    public void ValidateConfiguration(MachineConfiguration configuration) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.ConfigurationFunctions.ValidateConfiguration(configuration);
    public bool RequiresMediaRestart(EmulationMediaSlot slot, string? path) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.RuntimeFunctions.RequiresMediaRestart(slot, path);
    public bool RequiresOptionRestart(string key) => GWGUI.Emulation.Apple.Emulators.MiniVmac.Functions.RuntimeFunctions.RequiresOptionRestart(key);
}
