using GWGUI.Emulation.Apple.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Apple.Emulators.AppleWin.Constants;

namespace GWGUI.Emulation.Apple.Emulators.AppleWin.Factories;

internal sealed class AppleWinMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettings, IEmulatorSettingsApplication, IEmulatorStorageConfiguration, IEmulatorPointerCapabilities, IEmulatorConfiguration, IEmulatorRuntimePolicy
{
    public bool SupportsPointer(MachineConfiguration configuration) => configuration.Options.GetValueOrDefault(OptionConstants.Slot4) == ExpansionConstants.Mouse;
    public MachineConfiguration ApplySettings(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values) => GWGUI.Emulation.Apple.Emulators.AppleWin.Functions.ExpansionFunctions.ApplySettings(configuration, values);
    public MachineConfiguration ApplyStorageConfiguration(MachineConfiguration configuration, IReadOnlyList<EmulationMediaSlot> slots) => GWGUI.Emulation.Apple.Emulators.AppleWin.Functions.ExpansionFunctions.ApplyStorage(configuration, slots);
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => GWGUI.Emulation.Apple.Emulators.AppleWin.Functions.SettingsDescriptionFunctions.Create(configuration);
    public void InitializeOptions(MachineConfiguration configuration, IDictionary<string, string> options) => GWGUI.Emulation.Apple.Emulators.AppleWin.Functions.ConfigurationFunctions.InitializeOptions(configuration, options);
    public void ValidateConfiguration(MachineConfiguration configuration) => GWGUI.Emulation.Apple.Emulators.AppleWin.Functions.ConfigurationFunctions.ValidateConfiguration(configuration);
    public bool RequiresMediaRestart(EmulationMediaSlot slot, string? path) => GWGUI.Emulation.Apple.Emulators.AppleWin.Functions.RuntimeFunctions.RequiresMediaRestart(slot, path);
    public bool RequiresOptionRestart(string key) => GWGUI.Emulation.Apple.Emulators.AppleWin.Functions.RuntimeFunctions.RequiresOptionRestart(key);
}
