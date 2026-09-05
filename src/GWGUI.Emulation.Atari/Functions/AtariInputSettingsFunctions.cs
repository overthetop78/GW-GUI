using GWGUI.Emulation;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Atari.Functions;

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
                new EmulationInputBindingSet(ControllerDefinitions(configuration.Model, peripheral),
                    current?.Mappings ?? new Dictionary<string, string>(),
                    EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true),
                current?.DeadZonePercent ?? AtariControllerConstants.DefaultDeadZonePercent,
                current?.VisualId);
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
            port.DeadZonePercent, port.VisualId)).ToArray();
        var options = configuration.Options
            .Where(item => !item.Key.StartsWith(AtariMouseSettingsConstants.MappingOptionPrefix,
                StringComparison.Ordinal))
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
        if (settings.Mouse is not null)
            foreach (var binding in settings.Mouse.Values)
                options[AtariMouseSettingsConstants.MappingOptionPrefix + binding.Key] = binding.Value;
        var input = current with { KeyboardMappings = keyboard, Controllers = controllers };
        return configuration with { Options = options, Input = input };
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
        Definition(AtariInputSettingsFunctionsConstants.Left, AtariInputSettingsFunctionsConstants.ResourceMouseButtonLeft, AtariInputSettingsFunctionsConstants.MouseLeft),
        Definition(AtariInputSettingsFunctionsConstants.Right, AtariInputSettingsFunctionsConstants.ResourceMouseButtonRight, AtariInputSettingsFunctionsConstants.MouseRight)
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
        AtariPeripheralCategory peripheral)
    {
        var actions = ControllerActions(model, peripheral);
        return actions.Select(action => Definition(action, ActionResourceKey(action),
            string.Empty, ActionInvariantValue(action))).ToArray();
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
            return AtariControllerConstants.DirectionActions.Concat([AtariInputSettingsFunctionsConstants.Fire1, AtariInputSettingsFunctionsConstants.Fire2, AtariInputSettingsFunctionsConstants.Turbo]);
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

    private static EmulationControllerChoice Choice(
        AtariMachineModel model,
        AtariPeripheralCategory category) => new(category.ToString(), category switch
    {
        AtariPeripheralCategory.None => AtariInputSettingsFunctionsConstants.ResourceControllerNone,
        AtariPeripheralCategory.Automatic => AtariInputSettingsFunctionsConstants.ResourceControllerAutomatic,
        AtariPeripheralCategory.Joystick => AtariInputSettingsFunctionsConstants.ResourceAtariControllerJoystick,
        AtariPeripheralCategory.AnalogJoystick when model == AtariMachineModel.Atari5200
            => AtariInputSettingsFunctionsConstants.ResourceAtariControllerAtari5200,
        AtariPeripheralCategory.AnalogJoystick => AtariInputSettingsFunctionsConstants.ResourceControllerAnalogJoystick,
        AtariPeripheralCategory.Paddle => AtariInputSettingsFunctionsConstants.ResourceAtariControllerPaddleControllers,
        AtariPeripheralCategory.LightGun => AtariInputSettingsFunctionsConstants.ResourceAtariControllerXg1LightGun,
        AtariPeripheralCategory.NumericKeypad when model == AtariMachineModel.Atari5200
            => AtariInputSettingsFunctionsConstants.ResourceAtariControllerAtari5200,
        AtariPeripheralCategory.NumericKeypad => AtariInputSettingsFunctionsConstants.ResourceAtariControllerNumericKeypad,
        AtariPeripheralCategory.DrivingController => AtariInputSettingsFunctionsConstants.ResourceAtariControllerDriving,
        AtariPeripheralCategory.ProLineController => AtariInputSettingsFunctionsConstants.ResourceAtariControllerProLine,
        AtariPeripheralCategory.BoosterGrip => AtariInputSettingsFunctionsConstants.ResourceAtariControllerBoosterGrip,
        AtariPeripheralCategory.GenesisController => AtariInputSettingsFunctionsConstants.ResourceAtariControllerGenesis,
        AtariPeripheralCategory.Joy2BPlus => AtariInputSettingsFunctionsConstants.ResourceAtariControllerJoy2BPlus,
        AtariPeripheralCategory.EnhancedController when model == AtariMachineModel.Lynx
            => AtariInputSettingsFunctionsConstants.ResourceAtariControllerLynx,
        AtariPeripheralCategory.EnhancedController when model is AtariMachineModel.Jaguar
            or AtariMachineModel.JaguarCd => AtariInputSettingsFunctionsConstants.ResourceAtariControllerJaguar,
        _ => category.ToString()
    },
        BindingDefinitions: ControllerDefinitions(model, category),
        CompatibleVisualIds: CompatibleVisualIds(model, category),
        DefaultVisualId: DefaultVisualId(model, category),
        VisualCommandIds: VisualCommandIds(model, category));

    private static IReadOnlyList<string>? CompatibleVisualIds(
        AtariMachineModel model,
        AtariPeripheralCategory category) => category switch
    {
        AtariPeripheralCategory.Joystick when model == AtariMachineModel.Atari2600 =>
            [EmulationControllerVisualIds.AtariCx40],
        AtariPeripheralCategory.Joystick =>
        [
            EmulationControllerVisualIds.QuickShot,
            EmulationControllerVisualIds.QuickShotDeluxe,
            EmulationControllerVisualIds.QuickShotIiTurbo,
            EmulationControllerVisualIds.CompetitionPro5000,
            EmulationControllerVisualIds.ZipstikSuperPro,
            EmulationControllerVisualIds.KonixSpeedkingLeftHand,
            EmulationControllerVisualIds.KonixSpeedkingRightHand,
            EmulationControllerVisualIds.SuncomTac2,
            EmulationControllerVisualIds.PowerplayCruiser,
            EmulationControllerVisualIds.SuzoTheArcadeTurbo,
            EmulationControllerVisualIds.AdvancedGravisGamepad,
            EmulationControllerVisualIds.AtariCx40
        ],
        AtariPeripheralCategory.AnalogJoystick when model == AtariMachineModel.Atari5200 =>
            [EmulationControllerVisualIds.Atari5200Controller],
        AtariPeripheralCategory.Paddle => [EmulationControllerVisualIds.AtariPaddle],
        AtariPeripheralCategory.DrivingController =>
            [EmulationControllerVisualIds.Atari2600DrivingController],
        AtariPeripheralCategory.BoosterGrip => [EmulationControllerVisualIds.AtariBoosterGrip],
        AtariPeripheralCategory.Joy2BPlus => [EmulationControllerVisualIds.AtariJoy2BPlus],
        AtariPeripheralCategory.ProLineController =>
            [EmulationControllerVisualIds.Atari7800ProLineCx24,
                EmulationControllerVisualIds.Atari7800ControlPadEurope],
        AtariPeripheralCategory.LightGun => [EmulationControllerVisualIds.AtariXg1LightGun],
        AtariPeripheralCategory.EnhancedController when model == AtariMachineModel.Lynx =>
            [EmulationControllerVisualIds.AtariLynx, EmulationControllerVisualIds.AtariLynxIi],
        AtariPeripheralCategory.EnhancedController when model is AtariMachineModel.Jaguar
            or AtariMachineModel.JaguarCd =>
            [EmulationControllerVisualIds.AtariJaguarController,
                EmulationControllerVisualIds.AtariJaguarProController],
        _ => null
    };

    private static string? DefaultVisualId(
        AtariMachineModel model,
        AtariPeripheralCategory category) => category switch
    {
        AtariPeripheralCategory.Joystick when model == AtariMachineModel.Atari2600 =>
            EmulationControllerVisualIds.AtariCx40,
        AtariPeripheralCategory.Joystick => EmulationControllerVisualIds.QuickShot,
        AtariPeripheralCategory.ProLineController when model == AtariMachineModel.Atari7800 =>
            EmulationControllerVisualIds.Atari7800ControlPadEurope,
        _ => CompatibleVisualIds(model, category)?.FirstOrDefault()
    };

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>? VisualCommandIds(
        AtariMachineModel model,
        AtariPeripheralCategory category)
    {
        if (category is AtariPeripheralCategory.None or AtariPeripheralCategory.Automatic)
            return null;

        var actions = ControllerActions(model, category).ToHashSet(StringComparer.Ordinal);
        var result = new Dictionary<EmulationControllerVisualControl, string>();
        AddVisualCommand(result, actions, EmulationControllerVisualControl.DirectionUp,
            EmulationControllerCommandIds.Up);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.DirectionDown,
            EmulationControllerCommandIds.Down);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.DirectionLeft,
            EmulationControllerCommandIds.Left);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.DirectionRight,
            EmulationControllerCommandIds.Right);

        var jaguar = model is AtariMachineModel.Jaguar or AtariMachineModel.JaguarCd;
        AddVisualCommand(result, actions, EmulationControllerVisualControl.PrimaryAction,
            jaguar ? EmulationControllerCommandIds.A : EmulationControllerCommandIds.Fire1);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.SecondaryAction,
            jaguar ? EmulationControllerCommandIds.B : EmulationControllerCommandIds.Fire2);
        if (jaguar)
            AddVisualCommand(result, actions, EmulationControllerVisualControl.TertiaryAction,
                EmulationControllerCommandIds.C);

        AddVisualCommand(result, actions, EmulationControllerVisualControl.Turbo,
            EmulationControllerCommandIds.Turbo);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Start,
            EmulationControllerCommandIds.Start);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Pause,
            EmulationControllerCommandIds.Pause);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Reset,
            EmulationControllerCommandIds.Reset);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Option,
            EmulationControllerCommandIds.Option);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key0,
            EmulationControllerCommandIds.Key0);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key1,
            EmulationControllerCommandIds.Key1);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key2,
            EmulationControllerCommandIds.Key2);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key3,
            EmulationControllerCommandIds.Key3);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key4,
            EmulationControllerCommandIds.Key4);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key5,
            EmulationControllerCommandIds.Key5);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key6,
            EmulationControllerCommandIds.Key6);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key7,
            EmulationControllerCommandIds.Key7);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key8,
            EmulationControllerCommandIds.Key8);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key9,
            EmulationControllerCommandIds.Key9);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.KeyStar,
            EmulationControllerCommandIds.Star);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.KeyHash,
            EmulationControllerCommandIds.Hash);
        return result;
    }

    private static void AddVisualCommand(
        IDictionary<EmulationControllerVisualControl, string> result,
        IReadOnlySet<string> actions,
        EmulationControllerVisualControl control,
        string commandId)
    {
        if (actions.Contains(commandId)) result[control] = commandId;
    }

    private static string ActionResourceKey(string action) => action switch
    {
        AtariInputSettingsFunctionsConstants.Turbo => AtariInputSettingsFunctionsConstants.ResourceControllerActionTurboFire,
        _ => $"Emulation.Controller.Action.{action}"
    };

    private static string? ActionInvariantValue(string action) => action switch
    {
        AtariInputSettingsFunctionsConstants.Up or AtariInputSettingsFunctionsConstants.Down or AtariInputSettingsFunctionsConstants.Left or AtariInputSettingsFunctionsConstants.Right or AtariInputSettingsFunctionsConstants.Fire1 or AtariInputSettingsFunctionsConstants.Fire2 or AtariInputSettingsFunctionsConstants.Turbo => null,
        AtariInputSettingsFunctionsConstants.Option1 => AtariInputSettingsFunctionsConstants.Option12,
        AtariInputSettingsFunctionsConstants.Option2 => AtariInputSettingsFunctionsConstants.Option22,
        _ => action
    };

    private static InputBindingDefinition Definition(string id, string resourceKey, string defaultBinding,
        string? invariant = null) => new(id, resourceKey, defaultBinding, invariant);

    private static string KeyResource(EmulationKey key) => key switch
    {
        EmulationKey.Help => AtariInputSettingsFunctionsConstants.ResourceKeyAtariHelp,
        EmulationKey.AtariUndo => AtariInputSettingsFunctionsConstants.ResourceKeyAtariUndo,
        EmulationKey.AtariBreak => AtariInputSettingsFunctionsConstants.ResourceKeyAtariBreak,
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
