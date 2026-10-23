using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Coleco.Emulators.GearColeco.Constants;
using Settings = GWGUI.Emulation.Coleco.Emulators.GearColeco.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Coleco.Emulators.GearColeco.Factories;

internal sealed class GearColecoMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorControllerAdapter, IEmulatorSettingsAdapter, IEmulatorConfigurationValidation
{
    public override IReadOnlyList<PeripheralCategory> GetPeripherals(MachineConfiguration configuration) => [PeripheralCategory.None, PeripheralCategory.Standard, PeripheralCategory.SuperAction, PeripheralCategory.Roller, PeripheralCategory.Driving];
    public override IReadOnlyList<InputActionDefinition> DescribeInput(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => InputSettingsFunctions.Definitions(peripheral)
        .Where(action => port == ControllerConstants.FirstPort || peripheral == PeripheralCategory.Roller || action.Id is not (ControllerActionConstants.SpinnerLeft or ControllerActionConstants.SpinnerRight)).Select(action => new InputActionDefinition(action)).ToArray();
    public override EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => NativeInputSnapshotFunctions.Apply(snapshot, configuration, Emulator.GearColeco, DescribeInput);

    public override IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => Settings.GetOptions();
    public void ValidateConfiguration(MachineConfiguration configuration) => Settings.Validate(configuration);
}
