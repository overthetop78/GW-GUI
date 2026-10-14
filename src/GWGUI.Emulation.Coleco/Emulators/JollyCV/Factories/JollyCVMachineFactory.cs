using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Coleco.Emulators.JollyCV.Constants;
using Settings = GWGUI.Emulation.Coleco.Emulators.JollyCV.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Coleco.Emulators.JollyCV.Factories;

internal sealed class JollyCVMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorInputMapper, IEmulatorSettings
{
    public IReadOnlyList<PeripheralCategory> GetPeripherals() => [PeripheralCategory.None, PeripheralCategory.Standard, PeripheralCategory.SuperAction, PeripheralCategory.Paddle];
    public IReadOnlyList<InputBindingDefinition> DescribeInput(PeripheralCategory peripheral, int port) => InputSettingsFunctions.Definitions(peripheral);
    public EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model) => NativeInputSnapshotFunctions.Apply(snapshot, input, model, Emulator.JollyCV, DescribeInput);

    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions() => Settings.GetOptions();
}
