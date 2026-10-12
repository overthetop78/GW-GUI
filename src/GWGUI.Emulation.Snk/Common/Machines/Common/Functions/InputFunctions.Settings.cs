namespace GWGUI.Emulation.Snk.Common.Machines.Common.Functions;
internal static class InputSettingsFunctions
{
    internal static IReadOnlyList<PeripheralCategory> Peripherals(Emulator core)
    {
        if (!Enum.IsDefined(core))
            throw new ArgumentOutOfRangeException(nameof(core));
        IEnumerable<MachineModel> models = core == Emulator.None ? Enum.GetValues<MachineModel>() : EmulatorCatalog.Get(core).Models;
        return models.SelectMany(Choices).Distinct().ToArray();
    }

    internal static PeripheralCategory DefaultPeripheral(MachineModel model) => model switch
    {
        MachineModel.NeoGeoPocket => PeripheralCategory.NeoGeoPocket,
        MachineModel.NeoGeoPocketColor => PeripheralCategory.NeoGeoPocketColor,
        MachineModel.NeoGeoCd or MachineModel.NeoGeoCdz => PeripheralCategory.NeoGeoCdPad,
        MachineModel.NeoGeoAes or MachineModel.NeoGeoMvs => PeripheralCategory.NeoGeoArcadeStick,
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
    private static IReadOnlyList<PeripheralCategory> Choices(MachineModel model) => model switch
    {
        MachineModel.NeoGeoPocket => [PeripheralCategory.NeoGeoPocket],
        MachineModel.NeoGeoPocketColor => [PeripheralCategory.NeoGeoPocketColor],
        MachineModel.NeoGeoAes or MachineModel.NeoGeoMvs or MachineModel.NeoGeoCd or MachineModel.NeoGeoCdz => [PeripheralCategory.NeoGeoArcadeStick, PeripheralCategory.NeoGeoCdPad],
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
    internal static bool SupportsPeripheral(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => port >= ControllerConstants.FirstPort && port < ConfigurationFunctions.ControllerPortCount(configuration) && Choices(configuration.Model).Contains(peripheral);
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var ports = Enumerable.Range(ControllerConstants.FirstPort, ConfigurationFunctions.ControllerPortCount(configuration)).Select(port =>
        {
            var binding = configuration.Input.Controllers?.FirstOrDefault(item => item.Port == port);
            var peripheral = binding?.Peripheral ?? DefaultPeripheral(configuration.Model);
            var choices = Choices(configuration.Model).Select(category => new EmulationControllerChoice(
                    category.ToString(),
                    "Emulation.Snk.Controller." + category,
                    BindingDefinitions: ControllerProtocolDictionary.Actions(configuration.Model, category).Select(action => action.Definition).ToArray(),
                    CompatibleVisualIds: ControllerVisualDictionary.VisualIds(category),
                    DefaultVisualId: ControllerVisualDictionary.DefaultVisualId(category),
                    VisualCommandIds: ControllerVisualDictionary.Commands(category)
                )).ToArray();
            return new EmulationControllerPort(
                port + 1,
                choices,
                peripheral.ToString(),
                binding?.DeviceId,
                new(
                    ControllerProtocolDictionary.Actions(configuration.Model, peripheral).Select(action => action.Definition).ToArray(),
                    binding?.Mappings ?? new Dictionary<string, string>(),
                    EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller,
                    true
                ),
                binding?.DeadZonePercent ?? ControllerConstants.DefaultDeadZonePercent,
                binding?.VisualId
            );
        }).ToArray();
        return new(null, null, ports);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        var count = ConfigurationFunctions.ControllerPortCount(configuration);
        if (settings.Keyboard is not null || settings.Mouse is not null || settings.ControllerPorts.Count != count || !settings.ControllerPorts.Select(port => port.Number).Order().SequenceEqual(Enumerable.Range(1, count)))
            throw new ArgumentException(nameof(settings));
        var controllers = settings.ControllerPorts.Select(port =>
        {
            if (!Enum.TryParse<PeripheralCategory>(port.SelectedControllerId, out var peripheral) || !SupportsPeripheral(configuration, port.Number - 1, peripheral))
                throw new ArgumentException(nameof(settings));
            return new ControllerBinding(port.Number - 1, peripheral, port.PhysicalDeviceId, new Dictionary<string, string>(port.Bindings.Values, StringComparer.Ordinal), port.DeadZonePercent, port.VisualId);
        }).ToArray();
        var result = configuration with
        {
            Input = configuration.Input with
            {
                Controllers = controllers
            }
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }

    internal static void Validate(MachineConfiguration configuration)
    {
        foreach (var binding in configuration.Input.Controllers ?? [])
        {
            if (!SupportsPeripheral(configuration, binding.Port, binding.Peripheral))
                throw new ArgumentException(nameof(configuration.Input));
            var ids = ControllerProtocolDictionary.Actions(configuration.Model, binding.Peripheral).Select(action => action.Id).ToHashSet(StringComparer.Ordinal);
            if (binding.Mappings?.Any(pair => !ids.Contains(pair.Key) || !IsSource(pair.Value)) == true)
                throw new ArgumentException(nameof(configuration.Input));
            if (!string.IsNullOrEmpty(binding.VisualId) && !ControllerVisualDictionary.VisualIds(binding.Peripheral).Contains(binding.VisualId, StringComparer.Ordinal))
                throw new ArgumentException(nameof(configuration.Input));
        }
    }

    private static bool IsSource(string source)
    {
        if (string.IsNullOrEmpty(source))
            return true;
        if (source.StartsWith(InputMappingConstants.KeyboardPrefix, StringComparison.OrdinalIgnoreCase))
            return Enum.TryParse<EmulationKey>(source[InputMappingConstants.KeyboardPrefix.Length..], out var key) && Enum.IsDefined(key) && key != EmulationKey.Unknown;
        if (source.StartsWith(InputMappingConstants.MousePrefix, StringComparison.OrdinalIgnoreCase))
            return source[InputMappingConstants.MousePrefix.Length..] is InputMappingConstants.MouseLeft or InputMappingConstants.MouseRight or InputMappingConstants.MouseMiddle or InputMappingConstants.MouseExtended1 or InputMappingConstants.MouseExtended2 or InputMappingConstants.MouseWheelUp or InputMappingConstants.MouseWheelDown or InputMappingConstants.MouseWheelLeft or InputMappingConstants.MouseWheelRight;
        return source.Length > InputMappingConstants.ControllerPrefix.Length && source.StartsWith(InputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase);
    }
}