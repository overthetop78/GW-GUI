namespace GWGUI.Emulation.Bandai.Common.Machines.Common.Functions;

internal static class InputSettingsFunctions
{
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var peripheral = ConfigurationFunctions.DefaultPeripheral(configuration.Model);
        var current = configuration.Input.Controllers?.SingleOrDefault();
        var definitions = Definitions(configuration.Model);
        EmulationControllerChoice[] choices =
        [
            new(PeripheralCategory.None.ToString(), SettingsConstants.ResourceControllerNone, BindingDefinitions: []),
            new(peripheral.ToString(), SettingsConstants.ResourceControllerPrefix + peripheral,
                BindingDefinitions: definitions)
        ];
        var port = new EmulationControllerPort(ControllerConstants.FirstPort + BufferConstants.IndexIncrement,
            choices, current?.Peripheral == PeripheralCategory.Automatic ? peripheral.ToString()
                : (current?.Peripheral ?? peripheral).ToString(), current?.DeviceId,
            new EmulationInputBindingSet(current?.Peripheral == PeripheralCategory.None ? [] : definitions,
                current?.Mappings ?? new Dictionary<string, string>(),
                EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true),
            current?.DeadZonePercent ?? ControllerConstants.DefaultDeadZonePercent, current?.VisualId);
        return new(null, null, [port]);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        var controllers = settings.ControllerPorts.Select(port =>
        {
            if (!Enum.TryParse<PeripheralCategory>(port.SelectedControllerId, true, out var peripheral)
                || !Enum.IsDefined(peripheral))
                throw new ArgumentException(ErrorMessages.UnsupportedControllerDevice, nameof(settings));
            return new ControllerBinding(port.Number - BufferConstants.IndexIncrement,
                peripheral, port.PhysicalDeviceId, port.Bindings.Values, port.DeadZonePercent, port.VisualId);
        }).ToArray();
        var result = configuration with { Input = new InputConfiguration(controllers) };
        ConfigurationFunctions.Validate(result);
        return result;
    }

    private static IReadOnlyList<InputBindingDefinition> Definitions(MachineModel model) =>
        InputSnapshotDictionary.ForModel(model).Keys.Select(action => new InputBindingDefinition(
            action, SettingsConstants.ResourceActionPrefix + action, string.Empty)).ToArray();
}
