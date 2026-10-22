using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Epoch.Emulators.PD777.Constants;
using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Factories;
using Settings = GWGUI.Emulation.Epoch.Emulators.PD777.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Epoch.Emulators.PD777.Factories;

internal sealed class PD777MachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettingsAdapter, IEmulatorInputMapper, IEmulatorStorageSettings
{
    public IReadOnlyList<InputActionDefinition> DescribeInput(MachineModel model, int port)
    {
        var actions = ControllerProtocolDictionary.Actions(model, Emulator.PD777, port);
        return actions.Select(action => new InputActionDefinition(action.Definition with
        {
            OppositeDirectionId = action.Axis >= 0 ? actions.Single(other => other.Axis == action.Axis && other.Direction == -action.Direction).Id : action.Definition.OppositeDirectionId
        }, action.Key)).ToArray();
    }
    public EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => NativeInputSnapshotFunctions.Apply(snapshot, configuration.Input, configuration.Model, Emulator.PD777);

    public IReadOnlyList<EmulationMediaDevice> DescribeStorage(MachineConfiguration configuration) => Settings.Storage(configuration);
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions() => Settings.GetOptions();
}
