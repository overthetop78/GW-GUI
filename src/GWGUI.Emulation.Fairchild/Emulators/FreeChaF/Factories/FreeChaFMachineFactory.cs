using GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Functions;
using Settings = GWGUI.Emulation.Fairchild.Emulators.FreeChaF.Functions.SettingsDescriptionFunctions;
using GWGUI.Emulation.Fairchild.Emulators.FreeChaF.Constants;
using GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Fairchild.Emulators.FreeChaF.Factories;

internal sealed class FreeChaFMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettings, IEmulatorInputMapper, IEmulatorStorageSettings
{
    public IReadOnlyList<InputActionDefinition> DescribeInput(MachineModel model, int port) => ControllerProtocolDictionary.Actions(model, Emulator.FreeChaF, port).Select(action => new InputActionDefinition(action.Definition)).ToArray();
    public EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model) => NativeInputSnapshotFunctions.Apply(snapshot, input, model, Emulator.FreeChaF);
    public IReadOnlyList<EmulationMediaDevice> DescribeStorage(MachineConfiguration configuration) => Settings.Storage(configuration);

    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions() => Settings.GetOptions();
}
