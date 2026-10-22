using GWGUI.Emulation;
using CommonControllerFunctions = GWGUI.Emulation.Apple.Common.Functions.ControllerFunctions;
using GWGUI.Emulation.Apple.Common.Machines.AppleII.Dictionaries;
using GWGUI.Emulation.Apple.Common.Machines.Macintosh.Dictionaries;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;
internal static class InputSettingsFunctions
{
    internal static bool SupportsPointer(MachineConfiguration configuration) => new Engine().SelectedAdapter(configuration) is IEmulatorPointerCapabilities capabilities && capabilities.SupportsPointer(configuration);
    internal static IReadOnlyDictionary<EmulationKey, EmulationKey> DefaultHostKeys(MachineModel model) => model switch
    {
        MachineModel.AppleIIe or MachineModel.AppleIIeEnhanced => AppleIIKeyboardDictionary.DefaultHostKeys,
        MachineModel.Macintosh128K or MachineModel.Macintosh512K or MachineModel.MacintoshPlus or MachineModel.MacintoshClassic or MachineModel.MacintoshII => MacintoshKeyboardDictionary.DefaultHostKeys,
        _ => new Dictionary<EmulationKey, EmulationKey>()
    };
    private static IReadOnlyList<InputBindingDefinition> KeyboardDefinitions(MachineConfiguration configuration) => DefaultHostKeys(configuration.Model).Select(pair => new InputBindingDefinition(pair.Key.ToString(),
        InputSettingsFunctionsConstants.ResourceKeyboardPrefix + pair.Key,
        pair.Value.ToString())).ToArray();
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var input = configuration.Input;
        var definitions = KeyboardDefinitions(configuration);
        var keyboard = new EmulationInputBindingSet(definitions, input.KeyboardBindings ?? EmulationSdk.InputProcessing.ToStrings(input.KeyboardMappings, StringComparer.Ordinal), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller);
        var configured = input.Controllers ?? [];
        var choices = configuration.Family == MachineFamily.AppleII ? CommonControllerFunctions.Peripherals(configuration.Model).Where(category => category != PeripheralCategory.Automatic).OrderBy(category => category).Select(Choice).ToArray() : Array.Empty<EmulationControllerChoice>();
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
        var mouse = SupportsPointer(configuration)
            ? new EmulationInputBindingSet(
            configuration.Family == MachineFamily.Macintosh
                ? [new InputBindingDefinition(EmulationControllerCommandIds.Left, InputSettingsFunctionsConstants.ResourceMouseButtonLeft, InputSettingsFunctionsConstants.MouseLeft)]
                : [new InputBindingDefinition(EmulationControllerCommandIds.Left, InputSettingsFunctionsConstants.ResourceMouseButtonLeft, InputSettingsFunctionsConstants.MouseLeft),
                   new InputBindingDefinition(EmulationControllerCommandIds.Right, InputSettingsFunctionsConstants.ResourceMouseButtonRight, InputSettingsFunctionsConstants.MouseRight)],
            input.MouseMappings ?? new Dictionary<string, string>(),
            EmulationInputSource.Mouse | EmulationInputSource.Keyboard | EmulationInputSource.Controller
        )
            : null;
        return new EmulationInputSettings(keyboard, mouse, ports);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        var allowedKeys = KeyboardDefinitions(configuration).Select(definition => definition.Id).ToHashSet(StringComparer.Ordinal);
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
                Controllers = controllers,
                MouseMappings = settings.Mouse is null ? configuration.Input.MouseMappings : settings.Mouse.Values
                    .Where(pair => pair.Key == EmulationControllerCommandIds.Left || configuration.Family == MachineFamily.AppleII && pair.Key == EmulationControllerCommandIds.Right)
                    .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal)
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
            PeripheralCategory.AnalogJoystick => InputSettingsFunctionsConstants.ResourceControllerAnalogJoystick,
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        },
        BindingDefinitions: ControllerDefinitions(category),
        CompatibleVisualIds: category == PeripheralCategory.AnalogJoystick ?
        [
            EmulationControllerVisualIds.AppleStandardJ665,
            EmulationControllerVisualIds.AppleA2M2002,
            EmulationControllerVisualIds.AppleChMachII,
            EmulationControllerVisualIds.AppleChMachIII,
            EmulationControllerVisualIds.AppleKraftPremiumII
        ] : null,
        DefaultVisualId: category == PeripheralCategory.AnalogJoystick ? EmulationControllerVisualIds.AppleStandardJ665 : null,
        VisualCommandIds: category == PeripheralCategory.AnalogJoystick ? new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
            [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
            [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
            [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
            [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.Fire1,
            [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.Fire2
        } : null,
        VisualBindingDefinitions: category == PeripheralCategory.AnalogJoystick ? new Dictionary<string, IReadOnlyList<InputBindingDefinition>>
        {
            [EmulationControllerVisualIds.AppleKraftPremiumII] = ControllerDefinitions(category).Where(definition => definition.Id != EmulationControllerCommandIds.Fire2).ToArray()
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
