using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Epoch.Emulators.EmuSCV.Constants;
using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Factories;
using Settings = GWGUI.Emulation.Epoch.Emulators.EmuSCV.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Epoch.Emulators.EmuSCV.Factories;

internal sealed class EmuSCVMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettings, IEmulatorControllerAdapter, IEmulatorMediaCapabilities, IEmulatorConfigurationValidation
{
    public override IReadOnlyList<InputActionDefinition> DescribeInput(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => ControllerProtocolDictionary.Actions(configuration.Model, Emulator.EmuSCV, port).Select(action => new InputActionDefinition(action.Definition, action.Key)).ToArray();
    public override EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => NativeInputSnapshotFunctions.Apply(snapshot, configuration.Input, configuration.Model, Emulator.EmuSCV);

    public IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => Settings.Storage(configuration);
    public override IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => Settings.GetOptions();
    public void ValidateConfiguration(MachineConfiguration configuration) => Settings.Validate(configuration);
}
