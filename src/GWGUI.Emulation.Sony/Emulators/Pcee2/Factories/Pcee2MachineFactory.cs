using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sony.Emulators.Pcee2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcee2.Functions;
using Controllers = GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.ControllerConstants;

namespace GWGUI.Emulation.Sony.Emulators.Pcee2.Factories;

internal sealed class Pcee2MachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.Pcee2;
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
    public override IEnumerable<int> GetMemoryCardIndexes(MachineConfiguration configuration) => MemoryCardFunctions.ActiveSlots(configuration);
    public override string? GetMemoryCardDisplayLabel(int index) => MemoryCardFunctions.DisplayLabel(index);
    public override void ValidateConfiguration(MachineConfiguration configuration) => CheatFunctions.Validate(configuration);
    public override IReadOnlyList<int> GetAudioLatencyChoices(MachineConfiguration configuration) => AudioConfigurationFunctions.Latencies;
    public override AudioConfiguration GetAudioConfiguration(MachineConfiguration configuration) => AudioConfigurationFunctions.Resolve(configuration);
    protected override MachineConfiguration Configure(MachineConfiguration configuration) => base.Configure(AudioConfigurationFunctions.Configure(ControllerConfigurationFunctions.Configure(configuration)));
    public override int GetControllerPortCount(MachineConfiguration configuration) => Controllers.UsbPortCount + ControllerConfigurationFunctions.GamepadCount(configuration);
    public override string? GetControllerPortDisplayName(MachineConfiguration configuration, int port) => ControllerConfigurationFunctions.DisplayName(port);
    public override int? GetControllerPortDisplayNumber(MachineConfiguration configuration, int port) => port >= Controllers.UsbPortCount ? port - Controllers.UsbPortCount + 1 : null;
    public override IReadOnlyList<PeripheralCategory> GetPeripherals(MachineConfiguration configuration, int port) => port >= Controllers.UsbPortCount ? Controllers.GamepadTypes
        : ControllerConfigurationFunctions.UsbType(configuration, port) == PeripheralCategory.GunCon2 ? Controllers.EnabledUsbTypes : Controllers.DisabledUsbTypes;
    public override IReadOnlyList<PeripheralCategory> GetUsbPeripherals(MachineConfiguration configuration, int port) =>
        port is Controllers.FirstUsbPort or Controllers.SecondUsbPort ? GetPeripherals(configuration, port) : [PeripheralCategory.None];
    public override IReadOnlyList<InputBindingDefinition>? GetControllerBindingDefinitions(MachineConfiguration configuration, PeripheralCategory type, int port) =>
        type == PeripheralCategory.GunCon2 ? ControllerConfigurationFunctions.GunCommands : null;
    public override IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => NetworkFunctions.Describe(configuration, CheatFunctions.Describe(configuration, base.DescribeSettings(configuration, context)
        .Select(block => block with
        {
            Fields = block.Fields.Where(field => field.Id != OptionConstants.LightgunKey).Select(field => field.Id == OptionConstants.CrosshairKey
                ? field with { IsEnabled = Enumerable.Range(0, Controllers.UsbPortCount).Any(port => ControllerConfigurationFunctions.UsbType(configuration, port) == PeripheralCategory.GunCon2) }
                : field).ToArray()
        })
        .Where(block => block.Fields.Count > 0).ToArray()));
}
