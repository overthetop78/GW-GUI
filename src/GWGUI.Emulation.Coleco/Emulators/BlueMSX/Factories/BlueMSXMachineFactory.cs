using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Coleco.Emulators.BlueMSX.Constants;
using Settings = GWGUI.Emulation.Coleco.Emulators.BlueMSX.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Coleco.Emulators.BlueMSX.Factories;

internal sealed class BlueMSXMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorControllerAdapter, IEmulatorSettingsAdapter, IEmulatorConfigurationValidation, IEmulatorContentFormats
{
    public IReadOnlyList<PeripheralCategory> GetPeripherals() => [PeripheralCategory.None, PeripheralCategory.Standard, PeripheralCategory.SuperAction];
    public IReadOnlyList<InputBindingDefinition> DescribeInput(PeripheralCategory peripheral, int port) => InputSettingsFunctions.Definitions(peripheral)
        .Where(action => action.Id is not (ControllerActionConstants.SpinnerLeft or ControllerActionConstants.SpinnerRight)).ToArray();
    public EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => NativeInputSnapshotFunctions.Apply(snapshot, configuration.Input, configuration.Model, Emulator.BlueMSX, DescribeInput);

    public IReadOnlySet<string> ResolveContentExtensions(IReadOnlySet<string> declaredExtensions, bool requiresFilePath)
    {
        var extensions = new HashSet<string>(declaredExtensions, StringComparer.OrdinalIgnoreCase);
        // In the forced ColecoVision mode, the native file loader treats unknown suffixes as cartridges.
        extensions.Add(EmulatorConstants.RawCartridgeExtension);
        return extensions;
    }
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions() => Settings.GetOptions();
    public void ValidateConfiguration(MachineConfiguration configuration) => Settings.Validate(configuration);
}
