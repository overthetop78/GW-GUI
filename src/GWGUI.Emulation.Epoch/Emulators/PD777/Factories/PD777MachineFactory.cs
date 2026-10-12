using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Epoch.Emulators.PD777.Constants;
using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Factories;
using Settings = GWGUI.Emulation.Epoch.Emulators.PD777.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Epoch.Emulators.PD777.Factories;

internal sealed class PD777MachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettings, IEmulatorInputMapper, IEmulatorStorageSettings
{
    public IReadOnlyList<InputActionDefinition> DescribeInput(MachineModel model, int port) => ControllerProtocolDictionary.Actions(model, Emulator.PD777, port).Select(action => new InputActionDefinition(action.Definition, action.Key)).ToArray();
    public EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model) => NativeInputSnapshotFunctions.Apply(snapshot, input, model, Emulator.PD777);

    public IReadOnlyList<EmulationMediaDevice> DescribeStorage(MachineConfiguration configuration) => Settings.Storage(configuration);
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions() => Settings.GetOptions();
}
