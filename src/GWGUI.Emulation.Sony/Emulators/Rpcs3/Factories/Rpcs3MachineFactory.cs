using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sony.Emulators.Rpcs3.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Rpcs3.Factories;

internal sealed class Rpcs3MachineFactory : CoreMachineFactory
{
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
    internal const string ControllerCountKey = "input.controllerCount";
    public override int GetControllerPortCount(MachineConfiguration configuration) =>
        int.TryParse(configuration.Options?.GetValueOrDefault(ControllerCountKey), out var count) ? Math.Clamp(count, 1, 7) : 1;
    public override string? GetControllerPortDisplayName(MachineConfiguration configuration, int port) => "RF " + (port + 1);
    public override IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) =>
    [
        .. base.DescribeSettings(configuration, context),
        new("controllerCount", EmulationMachineTab.Controllers, "Emulation.Sony.Controller.Count",
        [
            new(ControllerCountKey, EmulationMachineTab.Controllers, "controllerCount", "Emulation.Sony.Controller.Count",
                EmulationSettingsEditor.Selection, GetControllerPortCount(configuration).ToString(System.Globalization.CultureInfo.InvariantCulture),
                Enumerable.Range(1, 7).Select(number => new EmulationSettingsChoice(number.ToString(), string.Empty, number.ToString())).ToArray(),
                RequiresRestart: true, RefreshSettingsOnChange: true, IsGlobalControllerOption: true)
        ])
    ];
    public override IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => Functions.StorageFunctions.Describe(configuration);
    public override IReadOnlyList<EmulationSettingsField> GetStorageDeviceConfigurationFields(MachineConfiguration configuration, EmulationMediaCategory category) =>
        category == EmulationMediaCategory.CompactDiscDrive ? Functions.StorageFunctions.Fields(configuration, true) : [];

}
