using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Coleco.Emulators.JollyCV.Constants;
using Settings = GWGUI.Emulation.Coleco.Emulators.JollyCV.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Coleco.Emulators.JollyCV.Factories;

internal sealed class JollyCVMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorControllerAdapter, IEmulatorSettingsAdapter
{
    public IReadOnlyList<PeripheralCategory> GetPeripherals(MachineModel model) => [PeripheralCategory.None, PeripheralCategory.Standard, PeripheralCategory.SuperAction, PeripheralCategory.Paddle];
    public IReadOnlyList<InputActionDefinition> DescribeInput(MachineModel model, int port, PeripheralCategory peripheral) => InputSettingsFunctions.Definitions(peripheral).Select(action => new InputActionDefinition(action)).ToArray();
    public EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => NativeInputSnapshotFunctions.Apply(snapshot, configuration.Input, configuration.Model, Emulator.JollyCV, DescribeInput);

    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => Settings.GetOptions();
}
