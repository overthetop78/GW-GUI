using System.Globalization;
using GWGUI.App.Input;
using GWGUI.App.Localization;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal static class AtariInputSettingsFunctions
{
    internal static AtariInputSettingsView Create(AtariMachineConfiguration configuration)
    {
        var compatibility = AtariCompatibilityCatalog.Get(configuration.Model);
        var hasKeyboard = IsEditable(compatibility, AtariSettingOption.KeyboardMappings);
        var hasMouse = IsEditable(compatibility, AtariSettingOption.MouseMappings);
        var definitions = hasKeyboard ? KeyboardDefinitions(configuration.Model) : [];
        var bindings = definitions.ToDictionary(value => value.Id, value =>
            configuration.Input.KeyboardMappings?.TryGetValue(value.Id, out var key) == true
                ? key.ToString() : value.DefaultBinding, StringComparer.Ordinal);
        var peripherals = Peripherals(configuration.Model);
        var ports = Enumerable.Range(AtariInputSettingsConstants.FirstPort, compatibility.ControllerPortCount)
            .Select(port =>
            {
                var configured = configuration.Input.Controllers?.FirstOrDefault(value => value.Port == port);
                return new AtariControllerPortView(port, peripherals,
                    configured?.Peripheral ?? AtariPeripheralKind.Automatic,
                    configured?.DeadZonePercent ?? AtariControllerConstants.DefaultDeadZonePercent,
                    configured?.DeviceId,
                    ControllerDefinitions(configuration.Model, port),
                    configured?.Mappings ?? new Dictionary<string, string>());
            }).ToArray();
        var speed = configuration.Options.TryGetValue(AtariInputSettingsConstants.MouseSpeedOptionKey, out var value)
            && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed : AtariInputSettingsConstants.DefaultMouseSpeedPercent;
        var mouseDefinitions = hasMouse ? AtariInputSettingsConstants.MouseActions.Select(action =>
            new InputBindingDefinition(action, MouseActionLabel(action), InputBindingSyntax.Mouse(action))).ToArray() : [];
        var mouseBindings = mouseDefinitions.ToDictionary(value => value.Id, value =>
            configuration.Options.GetValueOrDefault(AtariInputSettingsConstants.MouseMappingOptionPrefix + value.Id,
                value.DefaultBinding), StringComparer.Ordinal);
        return new AtariInputSettingsView(hasKeyboard, hasMouse, definitions, bindings,
            mouseDefinitions, mouseBindings, ports, speed);
    }

    internal static AtariMachineConfiguration Apply(AtariMachineConfiguration source,
        IEnumerable<InputBindingRow> keyboardRows, IEnumerable<InputBindingRow> mouseRows,
        IReadOnlyList<AtariControllerBinding> controllers,
        bool captureMouse, EmulationKey releaseMouseKey, int mouseSpeedPercent)
    {
        var keyboard = keyboardRows.Select(row => (row.Id, Key: ParseKey(row.Binding)))
            .Where(value => value.Key != EmulationKey.Unknown)
            .ToDictionary(value => value.Id, value => value.Key, StringComparer.Ordinal);
        var input = new AtariInputConfiguration(keyboard, controllers, source.Input.MouseDeviceId,
            captureMouse, releaseMouseKey);
        var displayed = mouseRows.Select(row => KeyValuePair.Create(
            AtariInputSettingsConstants.MouseMappingOptionPrefix + row.Id, row.Binding)).Append(
            KeyValuePair.Create(AtariInputSettingsConstants.MouseSpeedOptionKey,
                mouseSpeedPercent.ToString(CultureInfo.InvariantCulture)));
        var options = AtariGeneralSettingsFunctions.MergeOptions(source.Options, displayed);
        return new AtariMachineConfiguration(source.Model, source.Firmwares, source.Media, options, input,
            source.Id, source.SchemaVersion, source.AudioEnabled, source.VideoRenderer, source.Folders);
    }

    internal static IReadOnlyList<int> MouseSpeeds() => Enumerable.Range(
            AtariInputSettingsConstants.MinimumMouseSpeedPercent / AtariInputSettingsConstants.MouseSpeedStepPercent,
            (AtariInputSettingsConstants.MaximumMouseSpeedPercent - AtariInputSettingsConstants.MinimumMouseSpeedPercent)
            / AtariInputSettingsConstants.MouseSpeedStepPercent + AtariInputSettingsConstants.InclusiveEndpointCount)
        .Select(value => value * AtariInputSettingsConstants.MouseSpeedStepPercent).ToArray();

    private static IReadOnlyList<InputBindingDefinition> KeyboardDefinitions(AtariMachineModel model)
    {
        var core = AtariCompatibilityCatalog.Get(model).Core;
        var keys = core == AtariCoreKind.Atari800
            ? AtariInputSettingsConstants.Atari800SpecialKeys
            : AtariInputSettingsConstants.ComputerSpecialKeys;
        var machineKeys = core == AtariCoreKind.Atari800
            ? keys : AtariInputSettingsConstants.FunctionKeys.Concat(keys);
        return machineKeys.Distinct()
            .Select(key => new InputBindingDefinition(key.ToString(), KeyboardKeyLabel(key),
                AtariInputSettingsConstants.SpecialKeyDefaults.TryGetValue(key, out var hostKey)
                    ? hostKey.ToString() : key.ToString()))
            .ToArray();
    }

    private static IReadOnlyList<InputBindingDefinition> ControllerDefinitions(AtariMachineModel model, int port)
    {
        var actions = AtariInputSettingsConstants.StandardControllerActions.ToList();
        if (model == AtariMachineModel.Atari5200)
            actions.AddRange(AtariInputSettingsConstants.KeypadControllerActions);
        if (model is AtariMachineModel.Jaguar or AtariMachineModel.JaguarCd)
            actions.AddRange(AtariInputSettingsConstants.JaguarControllerActions);
        return actions.Select(action => new InputBindingDefinition(action, ControllerActionLabel(action),
            InputBindingSyntax.Controller(port, action))).ToArray();
    }

    private static string KeyboardKeyLabel(EmulationKey key) => key switch
    {
        EmulationKey.Help => LocExtension.Get("Emulation.Key.Help"),
        _ => key.ToString()
    };

    private static string MouseActionLabel(string action) => action switch
    {
        "Left" => LocExtension.Get("Emulation.MouseLeftButton"),
        "Right" => LocExtension.Get("Emulation.MouseRightButton"),
        _ => action
    };

    private static string ControllerActionLabel(string action) => action switch
    {
        "Up" => LocExtension.Get("Emulation.DirectionUp"),
        "Down" => LocExtension.Get("Emulation.DirectionDown"),
        "Left" => LocExtension.Get("Emulation.DirectionLeft"),
        "Right" => LocExtension.Get("Emulation.DirectionRight"),
        "Fire1" => LocExtension.Get("Emulation.FireButton1"),
        "Fire2" => LocExtension.Get("Emulation.FireButton2"),
        "Turbo" => LocExtension.Get("Emulation.TurboFire"),
        _ => action
    };

    private static EmulationKey ParseKey(string binding)
    {
        if (Enum.TryParse<EmulationKey>(binding, true, out var direct)) return direct;
        if (!KeyboardChord.TryParse(binding, out var chord) || chord.Keys.Count != AtariInputSettingsConstants.InclusiveEndpointCount)
            return EmulationKey.Unknown;
        return EmulationKeyMapper.TryMap(chord.Keys[AtariInputSettingsConstants.FirstPort], out var mapped)
            ? mapped : EmulationKey.Unknown;
    }

    private static IReadOnlyList<AtariPeripheralKind> Peripherals(AtariMachineModel model)
    {
        if (AtariCompatibilityCatalog.Get(model).Core == AtariCoreKind.Hatari)
            return [AtariPeripheralKind.None, AtariPeripheralKind.Automatic,
                AtariPeripheralKind.Mouse, AtariPeripheralKind.Joystick];
        return AtariClassicModelCatalog.Get(model).Ports
            .Where(port => port.Capability != AtariClassicPortCapability.Keyboard)
            .Select(port => port.Capability switch
            {
                AtariClassicPortCapability.Joystick => AtariPeripheralKind.Joystick,
                AtariClassicPortCapability.AnalogJoystick => AtariPeripheralKind.AnalogJoystick,
                AtariClassicPortCapability.Paddle => AtariPeripheralKind.Paddle,
                AtariClassicPortCapability.DrivingController => AtariPeripheralKind.DrivingController,
                AtariClassicPortCapability.NumericKeypad => AtariPeripheralKind.NumericKeypad,
                AtariClassicPortCapability.LightGun => AtariPeripheralKind.LightGun,
                AtariClassicPortCapability.ProLineController => AtariPeripheralKind.ProLineController,
                AtariClassicPortCapability.EnhancedController => AtariPeripheralKind.EnhancedController,
                _ => AtariPeripheralKind.None
            }).Append(AtariPeripheralKind.None).Append(AtariPeripheralKind.Automatic).Distinct().ToArray();
    }

    private static bool IsEditable(AtariCompatibilityDefinition definition, AtariSettingOption option) =>
        definition.Options.Single(value => value.Option == option).Availability == AtariOptionAvailability.Editable;
}
