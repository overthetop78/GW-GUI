using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Coleco.Emulators.JollyCV.Constants;
using Settings = GWGUI.Emulation.Coleco.Emulators.JollyCV.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Coleco.Emulators.JollyCV.Factories;

internal sealed class JollyCVMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorControllerAdapter, IEmulatorSettingsAdapter
{
    public override IReadOnlyList<PeripheralCategory> GetPeripherals(MachineConfiguration configuration) => [PeripheralCategory.None, PeripheralCategory.Standard, PeripheralCategory.SuperAction, PeripheralCategory.Paddle];
    public override IReadOnlyList<InputActionDefinition> DescribeInput(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => InputSettingsFunctions.Definitions(peripheral).Select(action => new InputActionDefinition(action)).ToArray();
    public override EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => NativeInputSnapshotFunctions.Apply(snapshot, configuration, Emulator.JollyCV, DescribeInput);

    public override IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => Settings.GetOptions();
}
