using GWGUI.Emulation;
using CommonControllerFunctions = GWGUI.Emulation.Acorn.Common.Functions.ControllerFunctions;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;

namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Functions;
internal static class InputSettingsFunctions
{
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var input = configuration.Input;
        var definitions = BbcMicroInputSettingsFunctions.KeyboardDefinitions(configuration);
        var keyboard = new EmulationInputBindingSet(definitions, input.KeyboardBindings ?? EmulationSdk.InputProcessing.ToStrings(input.KeyboardMappings, StringComparer.Ordinal), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller);
        var configured = input.Controllers ?? [];
        var choices = CommonControllerFunctions.Peripherals(configuration.Model).Where(category => category != PeripheralCategory.Automatic).OrderBy(category => category).Select(Choice).ToArray();
        var ports = Enumerable.Range(ControllerPortConstants.MinimumControllerPort, ConfigurationFunctions.ControllerPortCount(configuration.Model)).Select(index =>
        {
            var current = configured.FirstOrDefault(binding => binding.Port == index);
            var peripheral = current?.Peripheral ?? PeripheralCategory.AnalogJoystick;
            if (peripheral == PeripheralCategory.Automatic) peripheral = PeripheralCategory.AnalogJoystick;
            return new EmulationControllerPort(
                index + BufferConstants.IndexIncrement,
                choices,
                peripheral.ToString(),
                current?.DeviceId,
                new EmulationInputBindingSet(ControllerDefinitions(peripheral), current?.Mappings ?? new Dictionary<string, string>(), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true),
                current?.DeadZonePercent ?? ControllerConstants.DefaultDeadZonePercent,
                current?.VisualId
            );
        }).ToArray();
        return new EmulationInputSettings(keyboard, null, ports);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        var allowedKeys = BbcMicroInputSettingsFunctions.KeyboardDefinitions(configuration).Select(definition => definition.Id).ToHashSet(StringComparer.Ordinal);
        var keyboardBindings = settings.Keyboard is null 
            ? configuration.Input.KeyboardBindings ?? EmulationSdk.InputProcessing.ToStrings(configuration.Input.KeyboardMappings, StringComparer.Ordinal)
            : settings.Keyboard.Values.Where(item => allowedKeys.Contains(item.Key) && CommonControllerFunctions.IsValidBinding(item.Value)).ToDictionary(item => item.Key,
                item => string.IsNullOrWhiteSpace(item.Value) ? string.Empty : item.Value,
                StringComparer.Ordinal);
        var keyboard = EmulationSdk.InputProcessing.ToKeys(keyboardBindings);
        var controllers = settings.ControllerPorts.Select(port =>
        {
            if (!Enum.TryParse<PeripheralCategory>(port.SelectedControllerId, true, out var peripheral) || !Enum.IsDefined(peripheral))
                throw new ArgumentException(ErrorMessages.UnsupportedControllerDevice, nameof(settings));
            return new ControllerBinding(port.Number - BufferConstants.IndexIncrement, peripheral, port.PhysicalDeviceId, port.Bindings.Values, port.DeadZonePercent, port.VisualId);
        }).ToArray();
        var result = configuration with
        {
            Input = configuration.Input with
            {
                KeyboardMappings = keyboard,
                KeyboardBindings = keyboardBindings,
                Controllers = controllers
            }
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }

    private static EmulationControllerChoice Choice(PeripheralCategory category) => new(
        category.ToString(),
        category switch
        {
            PeripheralCategory.None => InputSettingsFunctionsConstants.ResourceControllerNone,
            PeripheralCategory.Automatic => InputSettingsFunctionsConstants.ResourceControllerAutomatic,
            PeripheralCategory.AnalogJoystick => "Controllers.Enum.Joystick",
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        },
        BindingDefinitions: ControllerDefinitions(category),
        CompatibleVisualIds: category == PeripheralCategory.AnalogJoystick ? [EmulationControllerVisualIds.CompetitionPro5000] : [],
        DefaultVisualId: category == PeripheralCategory.AnalogJoystick ? EmulationControllerVisualIds.CompetitionPro5000 : null,
        VisualCommandIds: category == PeripheralCategory.AnalogJoystick ? new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
            [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
            [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
            [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
            [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.Fire1,
            [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.Fire1
        } : null,
        VisualBindingDefinitions: category == PeripheralCategory.AnalogJoystick ? new Dictionary<string, IReadOnlyList<InputBindingDefinition>>
        {
            [EmulationControllerVisualIds.CompetitionPro5000] = ControllerDefinitions(category)
        } : null
    ) { Category = category == PeripheralCategory.None ? EmulationControllerCategory.None : EmulationControllerCategory.Joystick };
    private static IReadOnlyList<InputBindingDefinition> ControllerDefinitions(PeripheralCategory category) => category == PeripheralCategory.None 
        ? [] 
        : InputSettingsFunctionsConstants.ControllerActions.Select(action => new InputBindingDefinition(action, InputSettingsFunctionsConstants.ResourceActionPrefix + action, string.Empty)
        {
            OppositeDirectionId = category is PeripheralCategory.AnalogJoystick or PeripheralCategory.Automatic ? action switch
            {
                EmulationControllerCommandIds.Up => EmulationControllerCommandIds.Down,
                EmulationControllerCommandIds.Down => EmulationControllerCommandIds.Up,
                EmulationControllerCommandIds.Left => EmulationControllerCommandIds.Right,
                EmulationControllerCommandIds.Right => EmulationControllerCommandIds.Left,
                _ => null
            } : null
        }).ToArray();
}
