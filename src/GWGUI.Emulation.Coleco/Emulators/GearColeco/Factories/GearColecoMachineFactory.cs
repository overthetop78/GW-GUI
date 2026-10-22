using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Coleco.Emulators.GearColeco.Constants;
using Settings = GWGUI.Emulation.Coleco.Emulators.GearColeco.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Coleco.Emulators.GearColeco.Factories;

internal sealed class GearColecoMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorInputMapper, IEmulatorSettings, IEmulatorConfigurationValidation
{
    public IReadOnlyList<PeripheralCategory> GetPeripherals() => [PeripheralCategory.None, PeripheralCategory.Standard, PeripheralCategory.SuperAction, PeripheralCategory.Roller, PeripheralCategory.Driving];
    public IReadOnlyList<InputBindingDefinition> DescribeInput(PeripheralCategory peripheral, int port) => InputSettingsFunctions.Definitions(peripheral)
        .Where(action => port == ControllerConstants.FirstPort || peripheral == PeripheralCategory.Roller || action.Id is not (ControllerActionConstants.SpinnerLeft or ControllerActionConstants.SpinnerRight)).ToArray();
    public EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model) => NativeInputSnapshotFunctions.Apply(snapshot, input, model, Emulator.GearColeco, DescribeInput);

    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions() => Settings.GetOptions();
    public void ValidateConfiguration(MachineConfiguration configuration) => Settings.Validate(configuration);
}
