using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

internal static class AtariInputSettingsFunctions
{
    internal static EmulationInputSettings Describe(AtariMachineConfiguration configuration)
    {
        var compatibility = AtariCompatibilityCatalog.Get(configuration.Model);
        var input = configuration.Input ?? new AtariInputConfiguration();
        var keyboard = compatibility.VisibleTabs.Contains(AtariSettingsTab.Keyboard)
            ? new EmulationInputBindingSet(KeyboardDefinitions(configuration.Model),
                ToStrings(input.KeyboardMappings), EmulationInputSource.Keyboard) : null;
        var mouse = compatibility.VisibleTabs.Contains(AtariSettingsTab.Mouse)
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
                Peripherals(configuration.Model).Select(item => Choice(configuration.Model, item)).ToArray(),
                peripheral.ToString(), current?.DeviceId,
                new EmulationInputBindingSet(ControllerDefinitions(configuration.Model, peripheral, number),
                    current?.Mappings ?? new Dictionary<string, string>(),
                    EmulationInputSource.Keyboard | EmulationInputSource.Controller, true),
                current?.DeadZonePercent ?? AtariControllerConstants.DefaultDeadZonePercent);
        }).ToArray();
        return new EmulationInputSettings(keyboard, mouse, ports);
    }

    internal static AtariMachineConfiguration Apply(AtariMachineConfiguration configuration,
        EmulationInputSettings settings)
    {
        var current = configuration.Input ?? new AtariInputConfiguration();
        var keyboard = settings.Keyboard?.Values
            .Where(item => Enum.TryParse<EmulationKey>(item.Value, true, out _))
            .ToDictionary(item => item.Key, item => Enum.Parse<EmulationKey>(item.Value, true), StringComparer.Ordinal)
            ?? current.KeyboardMappings;
        var controllers = settings.ControllerPorts.Select(port => new AtariControllerBinding(port.Number - 1,
            Enum.TryParse<AtariPeripheralCategory>(port.SelectedControllerId, true, out var peripheral)
                ? peripheral : AtariPeripheralCategory.None, port.PhysicalDeviceId, port.Bindings.Values,
            port.DeadZonePercent)).ToArray();
        var options = configuration.Options
            .Where(item => !item.Key.StartsWith(AtariMouseSettingsConstants.MappingOptionPrefix,
                StringComparison.Ordinal))
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
        if (settings.Mouse is not null)
            foreach (var binding in settings.Mouse.Values)
                options[AtariMouseSettingsConstants.MappingOptionPrefix + binding.Key] = binding.Value;
        var input = current with { KeyboardMappings = keyboard, Controllers = controllers };
        return new AtariMachineConfiguration(configuration.Model, configuration.Firmwares, configuration.Media,
            options, input, configuration.Id, configuration.SchemaVersion,
            configuration.AudioEnabled, configuration.VideoRenderer, configuration.Folders);
    }

    private static IReadOnlyList<InputBindingDefinition> KeyboardDefinitions(AtariMachineModel model)
    {
        var emulator = AtariCompatibilityCatalog.Get(model).Core;
        IEnumerable<EmulationKey> keys = emulator == AtariEmulator.Atari800
            ? AtariInputSettingsConstants.Atari800SpecialKeys
            : AtariInputSettingsConstants.FunctionKeys.Concat(AtariInputSettingsConstants.ComputerSpecialKeys);
        if (model == AtariMachineModel.Atari400) keys = keys.Where(key => key != EmulationKey.Help);
        return keys.Distinct().Select(key => Definition(key.ToString(), KeyResource(key), DefaultKey(key),
            key is EmulationKey.AtariOption or EmulationKey.AtariSelect or EmulationKey.AtariStart
                ? key.ToString()[5..] : null)).ToArray();
    }

    private static IReadOnlyList<InputBindingDefinition> MouseDefinitions() =>
    [
        Definition("Left", "Emulation.Mouse.Button.Left", "Mouse:Left"),
        Definition("Right", "Emulation.Mouse.Button.Right", "Mouse:Right")
    ];

    private static IReadOnlyDictionary<string, string> MouseValues(
        IReadOnlyDictionary<string, string> options) => AtariMouseSettingsConstants.Actions.ToDictionary(
        action => action,
        action => options.GetValueOrDefault(AtariMouseSettingsConstants.MappingOptionPrefix + action,
            $"Mouse:{action}"), StringComparer.Ordinal);

    private static IReadOnlyList<AtariPeripheralCategory> Peripherals(AtariMachineModel model)
    {
        if (model == AtariMachineModel.Atari2600)
            return [AtariPeripheralCategory.Joystick, AtariPeripheralCategory.Paddle,
                AtariPeripheralCategory.DrivingController, AtariPeripheralCategory.BoosterGrip,
                AtariPeripheralCategory.GenesisController, AtariPeripheralCategory.Joy2BPlus,
                AtariPeripheralCategory.None];
        if (AtariCompatibilityCatalog.Get(model).Core == AtariEmulator.Hatari)
            return [AtariPeripheralCategory.Joystick, AtariPeripheralCategory.None];
        return AtariClassicModelCatalog.Get(model).Ports
            .Where(port => port.Capability != AtariClassicPortCapability.Keyboard)
            .Select(port => port.Capability switch
            {
                AtariClassicPortCapability.AnalogJoystick => AtariPeripheralCategory.AnalogJoystick,
                AtariClassicPortCapability.Paddle => AtariPeripheralCategory.Paddle,
                AtariClassicPortCapability.DrivingController => AtariPeripheralCategory.DrivingController,
                AtariClassicPortCapability.LightGun => AtariPeripheralCategory.LightGun,
                AtariClassicPortCapability.NumericKeypad => model == AtariMachineModel.Atari5200
                    ? AtariPeripheralCategory.AnalogJoystick : AtariPeripheralCategory.NumericKeypad,
                AtariClassicPortCapability.ProLineController => AtariPeripheralCategory.ProLineController,
                AtariClassicPortCapability.EnhancedController => AtariPeripheralCategory.EnhancedController,
                _ => AtariPeripheralCategory.Joystick
            }).Append(AtariPeripheralCategory.None).Distinct().ToArray();
    }

    private static IReadOnlyList<InputBindingDefinition> ControllerDefinitions(AtariMachineModel model,
        AtariPeripheralCategory peripheral, int port)
    {
        var actions = ControllerActions(model, peripheral);
        return actions.Select(action => Definition(action, ActionResourceKey(action),
            AtariControllerConstants.DefaultSources.TryGetValue(action, out var source)
                ? $"Controller:{port - 1}:{source}" : string.Empty, ActionInvariantValue(action))).ToArray();
    }

    private static IEnumerable<string> ControllerActions(AtariMachineModel model,
        AtariPeripheralCategory peripheral)
    {
        if (peripheral == AtariPeripheralCategory.None) return [];
        if (model == AtariMachineModel.Atari5200 && peripheral is AtariPeripheralCategory.AnalogJoystick
            or AtariPeripheralCategory.NumericKeypad)
            return AtariControllerConstants.DirectionActions.Concat(AtariControllerConstants.DualFireActions)
                .Concat(AtariControllerConstants.KeypadActions).Distinct(StringComparer.Ordinal);
        if (peripheral == AtariPeripheralCategory.NumericKeypad) return AtariControllerConstants.KeypadActions;
        if (peripheral is AtariPeripheralCategory.Paddle or AtariPeripheralCategory.DrivingController
            or AtariPeripheralCategory.LightGun) return AtariControllerConstants.SingleFireActions;
        if (peripheral == AtariPeripheralCategory.BoosterGrip)
            return AtariControllerConstants.DirectionActions.Concat(["Fire1", "Fire2", "Turbo"]);
        if (peripheral == AtariPeripheralCategory.Joy2BPlus)
            return AtariControllerConstants.DirectionActions.Concat(AtariControllerConstants.DualFireActions);
        if (peripheral == AtariPeripheralCategory.GenesisController)
            return AtariControllerConstants.DirectionActions.Concat(AtariControllerConstants.SingleFireActions);
        if (model is AtariMachineModel.Jaguar or AtariMachineModel.JaguarCd)
            return AtariControllerConstants.DirectionActions.Concat(AtariControllerConstants.JaguarActions);
        if (model == AtariMachineModel.Lynx)
            return AtariControllerConstants.DirectionActions.Concat(AtariControllerConstants.LynxActions);
        if (model is AtariMachineModel.Atari5200 or AtariMachineModel.Atari7800)
            return AtariControllerConstants.DirectionActions.Concat(AtariControllerConstants.DualFireActions);
        if (AtariCompatibilityCatalog.Get(model).Core == AtariEmulator.Hatari)
            return AtariControllerConstants.DirectionActions.Concat(AtariControllerConstants.HatariFireActions);
        return AtariControllerConstants.DirectionActions.Concat(AtariControllerConstants.SingleFireActions);
    }

    private static AtariPeripheralCategory DefaultPeripheral(AtariMachineModel model) => model switch
    {
        AtariMachineModel.Atari5200 => AtariPeripheralCategory.AnalogJoystick,
        AtariMachineModel.Atari7800 => AtariPeripheralCategory.ProLineController,
        AtariMachineModel.Lynx or AtariMachineModel.Jaguar or AtariMachineModel.JaguarCd
            => AtariPeripheralCategory.EnhancedController,
        _ => AtariPeripheralCategory.Joystick
    };

    private static EmulationControllerChoice Choice(AtariMachineModel model,
        AtariPeripheralCategory category) => new(category.ToString(), category switch
    {
        AtariPeripheralCategory.None => "Emulation.Controller.None",
        AtariPeripheralCategory.Automatic => "Emulation.Controller.Automatic",
        AtariPeripheralCategory.Joystick => "Emulation.Atari.Controller.Joystick",
        AtariPeripheralCategory.AnalogJoystick when model == AtariMachineModel.Atari5200
            => "Emulation.Atari.Controller.Atari5200",
        AtariPeripheralCategory.AnalogJoystick => "Emulation.Controller.AnalogJoystick",
        AtariPeripheralCategory.Paddle => "Emulation.Atari.Controller.PaddleControllers",
        AtariPeripheralCategory.LightGun => "Emulation.Atari.Controller.Xg1LightGun",
        AtariPeripheralCategory.NumericKeypad when model == AtariMachineModel.Atari5200
            => "Emulation.Atari.Controller.Atari5200",
        AtariPeripheralCategory.NumericKeypad => "Emulation.Atari.Controller.NumericKeypad",
        AtariPeripheralCategory.DrivingController => "Emulation.Atari.Controller.Driving",
        AtariPeripheralCategory.ProLineController => "Emulation.Atari.Controller.ProLine",
        AtariPeripheralCategory.BoosterGrip => "Emulation.Atari.Controller.BoosterGrip",
        AtariPeripheralCategory.GenesisController => "Emulation.Atari.Controller.Genesis",
        AtariPeripheralCategory.Joy2BPlus => "Emulation.Atari.Controller.Joy2BPlus",
        AtariPeripheralCategory.EnhancedController when model == AtariMachineModel.Lynx
            => "Emulation.Atari.Controller.Lynx",
        AtariPeripheralCategory.EnhancedController when model is AtariMachineModel.Jaguar
            or AtariMachineModel.JaguarCd => "Emulation.Atari.Controller.Jaguar",
        _ => category.ToString()
    });

    private static string ActionResourceKey(string action) => action switch
    {
        "Turbo" => "Emulation.Controller.Action.TurboFire",
        _ => $"Emulation.Controller.Action.{action}"
    };

    private static string? ActionInvariantValue(string action) => action switch
    {
        "Up" or "Down" or "Left" or "Right" or "Fire1" or "Fire2" or "Turbo" => null,
        "Option1" => "Option 1",
        "Option2" => "Option 2",
        _ => action
    };

    private static InputBindingDefinition Definition(string id, string resourceKey, string defaultBinding,
        string? invariant = null) => new(id, resourceKey, defaultBinding, invariant);

    private static string KeyResource(EmulationKey key) => key switch
    {
        EmulationKey.Help => "Emulation.Key.AtariHelp",
        EmulationKey.AtariUndo => "Emulation.Key.AtariUndo",
        EmulationKey.AtariBreak => "Emulation.Key.AtariBreak",
        _ => key.ToString()
    };

    private static string DefaultKey(EmulationKey key) =>
        AtariInputSettingsConstants.DefaultKeys.TryGetValue(key, out var defaultKey)
            ? defaultKey.ToString()
            : key.ToString();

    private static IReadOnlyDictionary<string, string> ToStrings(
        IReadOnlyDictionary<string, EmulationKey>? values) => values?.ToDictionary(item => item.Key,
        item => item.Value.ToString(), StringComparer.Ordinal) ?? new Dictionary<string, string>();
}
