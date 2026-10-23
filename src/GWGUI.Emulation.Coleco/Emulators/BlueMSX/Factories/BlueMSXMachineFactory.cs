using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Coleco.Emulators.BlueMSX.Constants;
using Settings = GWGUI.Emulation.Coleco.Emulators.BlueMSX.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Coleco.Emulators.BlueMSX.Factories;

internal sealed class BlueMSXMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorControllerAdapter, IEmulatorSettingsAdapter, IEmulatorConfigurationValidation, IEmulatorContentFormats
{
    public override IReadOnlyList<PeripheralCategory> GetPeripherals(MachineConfiguration configuration) => [PeripheralCategory.None, PeripheralCategory.Standard, PeripheralCategory.SuperAction];
    public override IReadOnlyList<InputActionDefinition> DescribeInput(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => InputSettingsFunctions.Definitions(peripheral)
        .Where(action => action.Id is not (ControllerActionConstants.SpinnerLeft or ControllerActionConstants.SpinnerRight)).Select(action => new InputActionDefinition(action)).ToArray();
    public override EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => NativeInputSnapshotFunctions.Apply(snapshot, configuration, Emulator.BlueMSX, DescribeInput);

    public IReadOnlySet<string> ResolveContentExtensions(IReadOnlySet<string> declaredExtensions, bool requiresFilePath)
    {
        var extensions = new HashSet<string>(declaredExtensions, StringComparer.OrdinalIgnoreCase);
        // In the forced ColecoVision mode, the native file loader treats unknown suffixes as cartridges.
        extensions.Add(EmulatorConstants.RawCartridgeExtension);
        return extensions;
    }
    public override IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => Settings.GetOptions();
    public void ValidateConfiguration(MachineConfiguration configuration) => Settings.Validate(configuration);
}
