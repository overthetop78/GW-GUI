using GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Functions;
using Settings = GWGUI.Emulation.Fairchild.Emulators.FreeChaF.Functions.SettingsDescriptionFunctions;
using GWGUI.Emulation.Fairchild.Emulators.FreeChaF.Constants;
using GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Fairchild.Emulators.FreeChaF.Factories;

internal sealed class FreeChaFMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettingsAdapter, IEmulatorControllerAdapter, IEmulatorMediaCapabilities
{
    public IReadOnlyList<InputActionDefinition> DescribeInput(MachineModel model, int port, PeripheralCategory peripheral) => ControllerProtocolDictionary.Actions(model, Emulator.FreeChaF, port).Select(action => new InputActionDefinition(action.Definition)).ToArray();
    public EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => NativeInputSnapshotFunctions.Apply(snapshot, configuration.Input, configuration.Model, Emulator.FreeChaF);
    public IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => Settings.Storage(configuration);

    public override IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => Settings.GetOptions();
}
