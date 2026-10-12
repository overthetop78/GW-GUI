namespace GWGUI.Emulation.Bandai.Common.Machines.Common.Functions;

internal static class InputSettingsFunctions
{
    internal static EmulationInputSettings Describe(MachineConfiguration configuration)
    {
        var peripheral = ConfigurationFunctions.DefaultPeripheral(configuration.Model);
        var current = configuration.Input.Controllers?.SingleOrDefault();
        var definitions = Definitions(configuration.Model);
        var visualId = configuration.Model switch
        {
            MachineModel.WonderSwan => EmulationControllerVisualIds.BandaiWonderSwan,
            MachineModel.WonderSwanColor => EmulationControllerVisualIds.BandaiWonderSwanColor,
            MachineModel.SwanCrystal => EmulationControllerVisualIds.BandaiSwanCrystal,
            MachineModel.Playdia => EmulationControllerVisualIds.BandaiPlaydia,
            MachineModel.TamagotchiP1 => EmulationControllerVisualIds.BandaiTamagotchiP1,
            _ => throw new ArgumentOutOfRangeException(nameof(configuration))
        };
        EmulationControllerChoice[] choices =
        [
            new(peripheral.ToString(), SettingsConstants.ResourceControllerPrefix + peripheral,
                InvariantDisplayValue: peripheral.ToString(),
                BindingDefinitions: definitions, CompatibleVisualIds: [visualId], DefaultVisualId: visualId,
                VisualCommandIds: VisualCommands(configuration.Model))
        ];
        var port = new EmulationControllerPort(ControllerConstants.FirstPort + BufferConstants.IndexIncrement,
            choices, peripheral.ToString(), current?.DeviceId,
            new EmulationInputBindingSet(definitions, current?.Mappings ?? new Dictionary<string, string>(), EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true),
            current?.DeadZonePercent ?? ControllerConstants.DefaultDeadZonePercent, visualId);
        return new(null, null, [port]);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationInputSettings settings)
    {
        var controllers = settings.ControllerPorts.Select(port =>
        {
            if (!Enum.TryParse<PeripheralCategory>(port.SelectedControllerId, true, out var peripheral)
                || peripheral != ConfigurationFunctions.DefaultPeripheral(configuration.Model))
                throw new ArgumentException(ErrorMessages.UnsupportedControllerDevice, nameof(settings));
            return new ControllerBinding(port.Number - BufferConstants.IndexIncrement, peripheral, port.PhysicalDeviceId, port.Bindings.Values, port.DeadZonePercent, port.VisualId);
        }).ToArray();
        var result = configuration with { Input = new InputConfiguration(controllers) };
        ConfigurationFunctions.Validate(result);
        return result;
    }

    private static IReadOnlyList<InputBindingDefinition> Definitions(MachineModel model) =>
        InputSnapshotDictionary.ForModel(model).Keys.Select(action => new InputBindingDefinition(
            action, action switch
            {
                "Up" or "Down" or "Left" or "Right" => "Emulation.Controller.Action." + action,
                "Execute" => "Common.Execute",
                "Cancel" => "Common.Cancel",
                _ => SettingsConstants.ResourceActionPrefix + action
            },
            string.Empty, action switch
            {
                "X1" => "X ↑", "X2" => "X →", "X3" => "X ↓", "X4" => "X ←",
                "Y1" => "Y ↑", "Y2" => "Y →", "Y3" => "Y ↓", "Y4" => "Y ←",
                "A" or "B" or "Start" or "Select" => action,
                _ => null
            })).ToArray();

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string> VisualCommands(MachineModel model)
    {
        if (model == MachineModel.TamagotchiP1) return new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.Option] = EmulationInputMappingConstants.Select,
            [EmulationControllerVisualControl.PrimaryAction] = InputSnapshotFunctionsConstants.Execute,
            [EmulationControllerVisualControl.SecondaryAction] = InputSnapshotFunctionsConstants.Cancel
        };
        if (model == MachineModel.Playdia) return new Dictionary<EmulationControllerVisualControl, string>();
        return new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = InputSnapshotFunctionsConstants.X1,
            [EmulationControllerVisualControl.DirectionRight] = InputSnapshotFunctionsConstants.X2,
            [EmulationControllerVisualControl.DirectionDown] = InputSnapshotFunctionsConstants.X3,
            [EmulationControllerVisualControl.DirectionLeft] = InputSnapshotFunctionsConstants.X4,
            [EmulationControllerVisualControl.CUp] = InputSnapshotFunctionsConstants.Y1,
            [EmulationControllerVisualControl.CRight] = InputSnapshotFunctionsConstants.Y2,
            [EmulationControllerVisualControl.CDown] = InputSnapshotFunctionsConstants.Y3,
            [EmulationControllerVisualControl.CLeft] = InputSnapshotFunctionsConstants.Y4,
            [EmulationControllerVisualControl.PrimaryAction] = EmulationInputMappingConstants.A,
            [EmulationControllerVisualControl.SecondaryAction] = EmulationInputMappingConstants.B,
            [EmulationControllerVisualControl.Start] = EmulationInputMappingConstants.Start
        };
    }
}
