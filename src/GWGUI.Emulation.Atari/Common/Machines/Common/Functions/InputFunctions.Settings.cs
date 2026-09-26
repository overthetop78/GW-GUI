using GWGUI.Emulation;
using GWGUI.Emulation.Atari.Common.Machines.Common.Constants;
using GWGUI.Emulation.Enums;
using Atari8BitKeyboard = GWGUI.Emulation.Atari.Common.Machines.Atari8Bit.Constants.Atari8BitKeyboardConstants;
using Atari8BitKeyboardDictionary = GWGUI.Emulation.Atari.Common.Machines.Atari8Bit.Dictionaries.Atari8BitKeyboardDictionary;
using AtariStKeyboard = GWGUI.Emulation.Atari.Common.Machines.AtariST.Constants.AtariStKeyboardConstants;
using AtariStKeyboardDictionary = GWGUI.Emulation.Atari.Common.Machines.AtariST.Dictionaries.AtariStKeyboardDictionary;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var compatibility = CompatibilityCatalog.Get(configuration.Model);
        var input = configuration.Input ?? new InputConfiguration();
        var keyboard = compatibility.VisibleTabs.Contains(SettingsTab.Keyboard)
            ? new EmulationInputBindingSet(KeyboardDefinitions(configuration.Model),
                ToStrings(input.KeyboardMappings), EmulationInputSource.Keyboard) : null;
        var mouse = compatibility.VisibleTabs.Contains(SettingsTab.Mouse)
            ? new EmulationInputBindingSet(MouseDefinitions(), MouseValues(configuration.Options),
                EmulationInputSource.Mouse | EmulationInputSource.Keyboard | EmulationInputSource.Controller,
                true)
            : null;
        var configured = input.Controllers ?? [];
        var ports = Enumerable.Range(0, compatibility.ControllerPortCount).Select(index =>
        {
            var number = index + 1;
            var current = configured.FirstOrDefault(item => item.Port == index);
            var peripheral = current?.Peripheral ?? DefaultPeripheral(configuration.Model);
            return new EmulationControllerPort(number,
                ControllerCatalog.Types(configuration.Model).Select(item => Choice(configuration.Model, item)).ToArray(),
                peripheral.ToString(), current?.DeviceId,
                new EmulationInputBindingSet(ControllerDefinitions(configuration.Model, peripheral),
                    current?.Mappings ?? new Dictionary<string, string>(),
                    EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true),
                current?.DeadZonePercent ?? ControllerConstants.DefaultDeadZonePercent,
                current?.VisualId);
        }).ToArray();
        return new EmulationInputSettings(keyboard, mouse, ports);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration,
        EmulationInputSettings settings)
    {
        var current = configuration.Input ?? new InputConfiguration();
        var keyboard = settings.Keyboard?.Values
            .Where(item => Enum.TryParse<EmulationKey>(item.Value, true, out _))
            .ToDictionary(item => item.Key, item => Enum.Parse<EmulationKey>(item.Value, true), StringComparer.Ordinal)
            ?? current.KeyboardMappings;
        var controllers = settings.ControllerPorts.Select(port => new ControllerBinding(port.Number - 1,
            Enum.TryParse<PeripheralCategory>(port.SelectedControllerId, true, out var peripheral)
                ? peripheral : PeripheralCategory.None, port.PhysicalDeviceId, port.Bindings.Values,
            port.DeadZonePercent, port.VisualId)).ToArray();
        var options = configuration.Options
            .Where(item => !item.Key.StartsWith(MouseSettingsConstants.MappingOptionPrefix,
                StringComparison.Ordinal))
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
        if (settings.Mouse is not null)
            foreach (var binding in settings.Mouse.Values)
                options[MouseSettingsConstants.MappingOptionPrefix + binding.Key] = binding.Value;
        var input = current with { KeyboardMappings = keyboard, Controllers = controllers };
        return configuration with { Options = options, Input = input };
    }

    private static IReadOnlyList<InputBindingDefinition> KeyboardDefinitions(MachineModel model)
    {
        var emulator = CompatibilityCatalog.Get(model).Core;
        IEnumerable<EmulationKey> keys = emulator == Emulator.Atari800
            ? Atari8BitKeyboard.SpecialKeys
            : AtariStKeyboard.SpecialKeys;
        if (model == MachineModel.Atari400) keys = keys.Where(key => key != EmulationKey.Help);
        var defaults = emulator == Emulator.Atari800
            ? Atari8BitKeyboardDictionary.DefaultHostKeys
            : AtariStKeyboardDictionary.DefaultHostKeys;
        return keys.Distinct().Select(key => Definition(key.ToString(), KeyResource(key), DefaultKey(key, defaults),
            key is EmulationKey.AtariOption or EmulationKey.AtariSelect or EmulationKey.AtariStart
                ? key.ToString()[5..] : null)).ToArray();
    }

    private static IReadOnlyList<InputBindingDefinition> MouseDefinitions() =>
    [
        Definition(InputSettingsFunctionsConstants.Left, InputSettingsFunctionsConstants.ResourceMouseButtonLeft, InputSettingsFunctionsConstants.MouseLeft),
        Definition(InputSettingsFunctionsConstants.Right, InputSettingsFunctionsConstants.ResourceMouseButtonRight, InputSettingsFunctionsConstants.MouseRight)
    ];

    private static IReadOnlyDictionary<string, string> MouseValues(
        IReadOnlyDictionary<string, string> options) => MouseSettingsConstants.Actions.ToDictionary(
        action => action,
        action => options.GetValueOrDefault(MouseSettingsConstants.MappingOptionPrefix + action,
            $"Mouse:{action}"), StringComparer.Ordinal);

    private static IReadOnlyList<InputBindingDefinition> ControllerDefinitions(MachineModel model,
        PeripheralCategory peripheral)
    {
        var actions = ControllerActions(model, peripheral);
        return actions.Select(action => Definition(action, ActionResourceKey(action),
            string.Empty, ActionInvariantValue(action))).ToArray();
    }

    private static IEnumerable<string> ControllerActions(MachineModel model,
        PeripheralCategory peripheral)
    {
        if (peripheral == PeripheralCategory.None) return [];
        if (model == MachineModel.Atari5200 && peripheral is PeripheralCategory.AnalogJoystick
            or PeripheralCategory.NumericKeypad)
            return ControllerActionConstants.DirectionActions.Concat(ControllerActionConstants.DualFireActions)
                .Concat(ControllerActionConstants.KeypadActions).Distinct(StringComparer.Ordinal);
        if (peripheral == PeripheralCategory.NumericKeypad) return ControllerActionConstants.KeypadActions;
        if (peripheral is PeripheralCategory.Paddle or PeripheralCategory.DrivingController
            or PeripheralCategory.LightGun) return ControllerActionConstants.SingleFireActions;
        if (peripheral == PeripheralCategory.BoosterGrip)
            return ControllerActionConstants.DirectionActions.Concat([InputSettingsFunctionsConstants.Fire1, InputSettingsFunctionsConstants.Fire2, InputSettingsFunctionsConstants.Turbo]);
        if (peripheral == PeripheralCategory.Joy2BPlus)
            return ControllerActionConstants.DirectionActions.Concat(ControllerActionConstants.DualFireActions);
        if (peripheral == PeripheralCategory.GenesisController)
            return ControllerActionConstants.DirectionActions.Concat(ControllerActionConstants.SingleFireActions);
        if (model is MachineModel.Jaguar or MachineModel.JaguarCd)
            return ControllerActionConstants.DirectionActions.Concat(ControllerActionConstants.JaguarActions);
        if (model == MachineModel.Lynx)
            return ControllerActionConstants.DirectionActions.Concat(ControllerActionConstants.LynxActions);
        if (model is MachineModel.Atari5200 or MachineModel.Atari7800)
            return ControllerActionConstants.DirectionActions.Concat(ControllerActionConstants.DualFireActions);
        if (CompatibilityCatalog.Get(model).Core == Emulator.Hatari)
            return ControllerActionConstants.DirectionActions.Concat(ControllerActionConstants.HatariFireActions);
        return ControllerActionConstants.DirectionActions.Concat(ControllerActionConstants.SingleFireActions);
    }

    private static PeripheralCategory DefaultPeripheral(MachineModel model) => model switch
    {
        MachineModel.Atari5200 => PeripheralCategory.AnalogJoystick,
        MachineModel.Atari7800 => PeripheralCategory.ProLineController,
        MachineModel.Lynx or MachineModel.Jaguar or MachineModel.JaguarCd
            => PeripheralCategory.EnhancedController,
        _ => PeripheralCategory.Joystick
    };

    private static EmulationControllerChoice Choice(
        MachineModel model,
        PeripheralCategory category) => new(category.ToString(), category switch
    {
        PeripheralCategory.None => InputSettingsFunctionsConstants.ResourceControllerNone,
        PeripheralCategory.Automatic => InputSettingsFunctionsConstants.ResourceControllerAutomatic,
        PeripheralCategory.Joystick => InputSettingsFunctionsConstants.ResourceAtariControllerJoystick,
        PeripheralCategory.AnalogJoystick when model == MachineModel.Atari5200
            => InputSettingsFunctionsConstants.ResourceAtariControllerAtari5200,
        PeripheralCategory.AnalogJoystick => InputSettingsFunctionsConstants.ResourceControllerAnalogJoystick,
        PeripheralCategory.Paddle => InputSettingsFunctionsConstants.ResourceAtariControllerPaddleControllers,
        PeripheralCategory.LightGun => InputSettingsFunctionsConstants.ResourceAtariControllerXg1LightGun,
        PeripheralCategory.NumericKeypad when model == MachineModel.Atari5200
            => InputSettingsFunctionsConstants.ResourceAtariControllerAtari5200,
        PeripheralCategory.NumericKeypad => InputSettingsFunctionsConstants.ResourceAtariControllerNumericKeypad,
        PeripheralCategory.DrivingController => InputSettingsFunctionsConstants.ResourceAtariControllerDriving,
        PeripheralCategory.ProLineController => InputSettingsFunctionsConstants.ResourceAtariControllerProLine,
        PeripheralCategory.BoosterGrip => InputSettingsFunctionsConstants.ResourceAtariControllerBoosterGrip,
        PeripheralCategory.GenesisController => InputSettingsFunctionsConstants.ResourceAtariControllerGenesis,
        PeripheralCategory.Joy2BPlus => InputSettingsFunctionsConstants.ResourceAtariControllerJoy2BPlus,
        PeripheralCategory.EnhancedController when model == MachineModel.Lynx
            => InputSettingsFunctionsConstants.ResourceAtariControllerLynx,
        PeripheralCategory.EnhancedController when model is MachineModel.Jaguar
            or MachineModel.JaguarCd => InputSettingsFunctionsConstants.ResourceAtariControllerJaguar,
        _ => category.ToString()
    },
        BindingDefinitions: ControllerDefinitions(model, category),
        CompatibleVisualIds: CompatibleVisualIds(model, category),
        DefaultVisualId: DefaultVisualId(model, category),
        VisualCommandIds: VisualCommandIds(model, category));
}
