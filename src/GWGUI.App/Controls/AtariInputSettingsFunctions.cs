using System.Globalization;
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
        var keyboardDefinitions = hasKeyboard ? AtariKeyboardSettingsFunctions.Definitions(configuration.Model) : [];
        var keyboardBindings = keyboardDefinitions.ToDictionary(value => value.Id, value =>
            configuration.Input.KeyboardMappings?.TryGetValue(value.Id, out var key) == true
                ? key.ToString() : value.DefaultBinding, StringComparer.Ordinal);
        var peripherals = AtariControllerSettingsFunctions.Peripherals(configuration.Model);
        var ports = Enumerable.Range(AtariInputSettingsConstants.FirstPort, compatibility.ControllerPortCount)
            .Select(port =>
            {
                var configured = configuration.Input.Controllers?.FirstOrDefault(value => value.Port == port);
                var selected = NormalizeLegacyPeripheral(configuration.Model,
                    configured?.Peripheral ?? AtariControllerSettingsFunctions.DefaultPeripheral(configuration.Model));
                return new AtariControllerPortView(port, peripherals, selected,
                    configured?.DeadZonePercent ?? AtariControllerConstants.DefaultDeadZonePercent,
                    configured?.DeviceId, AtariControllerSettingsFunctions.Definitions(configuration.Model, selected, port),
                    configured?.Mappings ?? new Dictionary<string, string>());
            }).ToArray();
        var speed = configuration.Options.TryGetValue(AtariMouseSettingsConstants.SpeedOptionKey, out var value)
            && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed : AtariMouseSettingsConstants.DefaultSpeedPercent;
        var mouseDefinitions = hasMouse ? AtariMouseSettingsFunctions.Definitions() : [];
        var mouseBindings = mouseDefinitions.ToDictionary(value => value.Id, value =>
            configuration.Options.GetValueOrDefault(AtariMouseSettingsConstants.MappingOptionPrefix + value.Id,
                value.DefaultBinding), StringComparer.Ordinal);
        var hasEightBitControllerOptions = AtariEightBitSettingsCatalog.SupportsComputerOptions(configuration.Model);
        var paddleMovementSpeed = configuration.Options.GetValueOrDefault(
            AtariEightBitSettingsConstants.PaddleMovementSpeedOptionKey,
            AtariEightBitSettingsConstants.DefaultPaddleMovementSpeed);
        if (!AtariEightBitSettingsCatalog.PaddleMovementSpeeds.Contains(paddleMovementSpeed))
            paddleMovementSpeed = AtariEightBitSettingsConstants.DefaultPaddleMovementSpeed;
        var autofireMode = configuration.Options.GetValueOrDefault(
            AtariEightBitSettingsConstants.AutofireOptionKey, AtariEightBitSettingsConstants.Disabled);
        if (!AtariEightBitSettingsCatalog.AutofireModes.Contains(autofireMode))
            autofireMode = AtariEightBitSettingsConstants.Disabled;
        var controllerCompatibility = Validated(configuration.Options,
            AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey,
            AtariEightBitSettingsCatalog.ControllerCompatibilityModes, AtariEightBitSettingsConstants.None);
        var digitalSensitivity = Validated(configuration.Options,
            AtariEightBitSettingsConstants.DigitalSensitivityOptionKey,
            AtariEightBitSettingsCatalog.Sensitivities, AtariEightBitSettingsConstants.DefaultSensitivity);
        var analogSensitivity = Validated(configuration.Options,
            AtariEightBitSettingsConstants.AnalogSensitivityOptionKey,
            AtariEightBitSettingsCatalog.Sensitivities, AtariEightBitSettingsConstants.DefaultSensitivity);
        return new AtariInputSettingsView(hasKeyboard, hasMouse, keyboardDefinitions, keyboardBindings,
            mouseDefinitions, mouseBindings, ports, speed, hasEightBitControllerOptions,
            paddleMovementSpeed, autofireMode, controllerCompatibility, digitalSensitivity, analogSensitivity);
    }

    internal static AtariMachineConfiguration Apply(AtariMachineConfiguration source,
        IEnumerable<InputBindingRow> keyboardRows, IEnumerable<InputBindingRow> mouseRows,
        IReadOnlyList<AtariControllerBinding> controllers, bool captureMouse,
        EmulationKey releaseMouseKey, int mouseSpeedPercent,
        string paddleMovementSpeed = AtariEightBitSettingsConstants.DefaultPaddleMovementSpeed,
        string autofireMode = AtariEightBitSettingsConstants.Disabled,
        string controllerCompatibility = AtariEightBitSettingsConstants.None,
        string digitalSensitivity = AtariEightBitSettingsConstants.DefaultSensitivity,
        string analogSensitivity = AtariEightBitSettingsConstants.DefaultSensitivity)
    {
        var keyboard = keyboardRows.Select(row => (row.Id, Key: AtariKeyboardSettingsFunctions.Parse(row.Binding)))
            .Where(value => value.Key != EmulationKey.Unknown)
            .ToDictionary(value => value.Id, value => value.Key, StringComparer.Ordinal);
        var input = new AtariInputConfiguration(keyboard, controllers, source.Input.MouseDeviceId,
            captureMouse, releaseMouseKey);
        IEnumerable<KeyValuePair<string, string>> displayed = mouseRows.Select(row => KeyValuePair.Create(
            AtariMouseSettingsConstants.MappingOptionPrefix + row.Id, row.Binding)).Append(
            KeyValuePair.Create(AtariMouseSettingsConstants.SpeedOptionKey,
                mouseSpeedPercent.ToString(CultureInfo.InvariantCulture)));
        if (source.Core == AtariCoreKind.Atari800)
            displayed = displayed
                .Append(KeyValuePair.Create(AtariEightBitSettingsConstants.PaddleActiveOptionKey,
                    controllers.Any(controller => controller.Peripheral == AtariPeripheralKind.Paddle)
                        ? AtariEightBitSettingsConstants.Enabled : AtariEightBitSettingsConstants.Disabled))
                .Append(KeyValuePair.Create(AtariEightBitSettingsConstants.PaddleMovementSpeedOptionKey,
                    AtariEightBitSettingsCatalog.PaddleMovementSpeeds.Contains(paddleMovementSpeed)
                        ? paddleMovementSpeed : AtariEightBitSettingsConstants.DefaultPaddleMovementSpeed))
                .Append(KeyValuePair.Create(AtariEightBitSettingsConstants.AutofireOptionKey,
                    AtariEightBitSettingsCatalog.AutofireModes.Contains(autofireMode)
                        ? autofireMode : AtariEightBitSettingsConstants.Disabled))
                .Append(KeyValuePair.Create(AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey,
                    AtariEightBitSettingsCatalog.ControllerCompatibilityModes.Contains(controllerCompatibility)
                        ? controllerCompatibility : AtariEightBitSettingsConstants.None))
                .Append(KeyValuePair.Create(AtariEightBitSettingsConstants.DigitalSensitivityOptionKey,
                    AtariEightBitSettingsCatalog.Sensitivities.Contains(digitalSensitivity)
                        ? digitalSensitivity : AtariEightBitSettingsConstants.DefaultSensitivity))
                .Append(KeyValuePair.Create(AtariEightBitSettingsConstants.AnalogSensitivityOptionKey,
                    AtariEightBitSettingsCatalog.Sensitivities.Contains(analogSensitivity)
                        ? analogSensitivity : AtariEightBitSettingsConstants.DefaultSensitivity));
        var options = AtariGeneralSettingsFunctions.MergeOptions(source.Options, displayed);
        return new AtariMachineConfiguration(source.Model, source.Firmwares, source.Media, options, input,
            source.Id, source.SchemaVersion, source.AudioEnabled, source.VideoRenderer, source.Folders);
    }

    private static bool IsEditable(AtariCompatibilityDefinition definition, AtariSettingOption option) =>
        definition.Options.Single(value => value.Option == option).Availability == AtariOptionAvailability.Editable;

    private static string Validated(IReadOnlyDictionary<string, string> options, string key,
        IReadOnlyList<string> allowed, string fallback)
    {
        var value = options.GetValueOrDefault(key, fallback);
        return allowed.Contains(value) ? value : fallback;
    }

    private static AtariPeripheralKind NormalizeLegacyPeripheral(AtariMachineModel model,
        AtariPeripheralKind peripheral) =>
        peripheral == AtariPeripheralKind.Automatic
            ? AtariControllerSettingsFunctions.DefaultPeripheral(model)
            : model == AtariMachineModel.Atari5200 && peripheral == AtariPeripheralKind.NumericKeypad
                ? AtariPeripheralKind.AnalogJoystick
                : AtariControllerSettingsFunctions.Peripherals(model).Contains(peripheral)
                    ? peripheral
                    : AtariControllerSettingsFunctions.DefaultPeripheral(model);
}
