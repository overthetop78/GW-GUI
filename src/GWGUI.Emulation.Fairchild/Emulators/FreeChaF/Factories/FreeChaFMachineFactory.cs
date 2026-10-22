using GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Functions;
using Settings = GWGUI.Emulation.Fairchild.Emulators.FreeChaF.Functions.SettingsDescriptionFunctions;
using GWGUI.Emulation.Fairchild.Emulators.FreeChaF.Constants;
using GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Fairchild.Emulators.FreeChaF.Factories;

internal sealed class FreeChaFMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettingsAdapter, IEmulatorInputMapper, IEmulatorStorageSettings
{
    public IReadOnlyList<InputActionDefinition> DescribeInput(MachineModel model, int port) => ControllerProtocolDictionary.Actions(model, Emulator.FreeChaF, port).Select(action => new InputActionDefinition(action.Definition)).ToArray();
    public EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => NativeInputSnapshotFunctions.Apply(snapshot, configuration.Input, configuration.Model, Emulator.FreeChaF);
    public IReadOnlyList<EmulationMediaDevice> DescribeStorage(MachineConfiguration configuration) => Settings.Storage(configuration);

    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions() => Settings.GetOptions();
}
