using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Coleco.Emulators.JollyCV.Constants;
using Settings = GWGUI.Emulation.Coleco.Emulators.JollyCV.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Coleco.Emulators.JollyCV.Factories;

internal sealed class JollyCVMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorControllerAdapter, IEmulatorSettingsAdapter
{
    public IReadOnlyList<PeripheralCategory> GetPeripherals() => [PeripheralCategory.None, PeripheralCategory.Standard, PeripheralCategory.SuperAction, PeripheralCategory.Paddle];
    public IReadOnlyList<InputBindingDefinition> DescribeInput(PeripheralCategory peripheral, int port) => InputSettingsFunctions.Definitions(peripheral);
    public EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => NativeInputSnapshotFunctions.Apply(snapshot, configuration.Input, configuration.Model, Emulator.JollyCV, DescribeInput);

    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => Settings.GetOptions();
}
