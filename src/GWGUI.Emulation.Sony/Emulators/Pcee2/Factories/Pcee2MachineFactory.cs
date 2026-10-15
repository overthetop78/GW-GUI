using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sony.Emulators.Pcee2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcee2.Functions;
using Controllers = GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.ControllerConstants;

namespace GWGUI.Emulation.Sony.Emulators.Pcee2.Factories;

internal sealed class Pcee2MachineFactory : CoreMachineFactory
{
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
    public override IReadOnlyList<int> GetAudioLatencyChoices(MachineConfiguration configuration) => AudioConfigurationFunctions.Latencies;
    public override AudioConfiguration GetAudioConfiguration(MachineConfiguration configuration) => AudioConfigurationFunctions.Resolve(configuration);
    protected override MachineConfiguration Configure(MachineConfiguration configuration) => base.Configure(AudioConfigurationFunctions.Configure(ControllerConfigurationFunctions.Configure(configuration)));
    public override int GetControllerPortCount(MachineConfiguration configuration) => Controllers.UsbPortCount + ControllerConfigurationFunctions.GamepadCount(configuration);
    public override string? GetControllerPortDisplayName(MachineConfiguration configuration, int port) => ControllerConfigurationFunctions.DisplayName(port);
    public override IReadOnlyList<ControllerType> GetControllerTypes(MachineConfiguration configuration, int port) => port >= Controllers.UsbPortCount ? Controllers.GamepadTypes
        : ControllerConfigurationFunctions.UsbType(configuration, port) == ControllerType.GunCon2 ? Controllers.EnabledUsbTypes : Controllers.DisabledUsbTypes;
    public override IReadOnlyList<InputBindingDefinition>? GetControllerBindingDefinitions(MachineConfiguration configuration, ControllerType type, int port) =>
        type == ControllerType.GunCon2 ? ControllerConfigurationFunctions.GunCommands : null;
    public override IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration) => base.GetSettingsBlocks(configuration)
        .Select(block => block with
        {
            Fields = block.Fields.Where(field => field.Id != OptionConstants.LightgunKey).Select(field => field.Id == OptionConstants.CrosshairKey
                ? field with { IsEnabled = Enumerable.Range(0, Controllers.UsbPortCount).Any(port => ControllerConfigurationFunctions.UsbType(configuration, port) == ControllerType.GunCon2) }
                : field).ToArray()
        })
        .Where(block => block.Fields.Count > 0).ToArray();
}
