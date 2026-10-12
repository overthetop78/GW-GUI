namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Functions;
internal static class InputSnapshotFunctions
{
    // Named controls contain normalized command pressure; the .Delta suffix carries relative displacement separately.
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model, Emulator core)
    {
        if (!Enum.IsDefined(model) || !Enum.IsDefined(core))
            throw new ArgumentOutOfRangeException(nameof(model));
        var controllers = new EmulationControllerState[ControllerConstants.PortCount];
        for (var port = ControllerConstants.FirstPort; port < controllers.Length; port++)
        {
            var binding = input.Controllers?.FirstOrDefault(item => item.Port == port);
            if (binding is null || binding.Peripheral == PeripheralCategory.None)
            {
                controllers[port] = EmulationControllerState.Empty;
                continue;
            }

            var source = EmulationInputMappingFunctions.ResolveController(binding.DeviceId, snapshot.Controllers, port);
            var values = new Dictionary<string, float>(StringComparer.Ordinal);
            foreach (var command in ControllerCatalog.Commands(binding.Peripheral))
            {
                var mapping = binding.Mappings?.GetValueOrDefault(command.ToString()) ?? string.Empty;
                if (string.IsNullOrEmpty(mapping))
                    continue;
                var physical = EmulationInputMappingFunctions.ResolveSourceController(mapping, snapshot.Controllers, source);
                var value = Value(mapping, physical, snapshot, binding.DeadZonePercent);
                var relative = IsMotion(command) && mapping.StartsWith(InputMappingConstants.MousePrefix, StringComparison.OrdinalIgnoreCase) && mapping[InputMappingConstants.MousePrefix.Length..] is "X+" or "X−" or "Y+" or "Y−";
                values[command.ToString() + (relative ? InputMappingConstants.RelativeMotionSuffix : string.Empty)] = value;
            }

            controllers[port] = EmulationControllerState.Empty with
            {
                DeviceId = source.DeviceId,
                Controls = new(values)
            };
        }

        return snapshot with
        {
            Keys = EmulationInputSnapshot.Empty.Keys,
            Pointer = EmulationInputSnapshot.Empty.Pointer,
            Controllers = controllers
        };
    }

    private static bool IsMotion(ControllerCommand command) => command is ControllerCommand.MoveUp or ControllerCommand.MoveDown
        or ControllerCommand.MoveLeft or ControllerCommand.MoveRight or ControllerCommand.AimUp or ControllerCommand.AimDown or ControllerCommand.AimLeft
        or ControllerCommand.AimRight;
    private static float Value(string mapping, EmulationControllerState controller, EmulationInputSnapshot snapshot, int deadZone)
    {
        if (mapping.StartsWith(InputMappingConstants.KeyboardPrefix, StringComparison.OrdinalIgnoreCase))
            return Enum.TryParse<EmulationKey>(mapping[InputMappingConstants.KeyboardPrefix.Length..], out var key) && snapshot.Keys.Contains(key) ? 1 : 0;
        if (mapping.StartsWith(InputMappingConstants.MousePrefix, StringComparison.OrdinalIgnoreCase))
            return mapping[InputMappingConstants.MousePrefix.Length..] switch
            {
                "X+" => Math.Max(0, snapshot.Pointer.DeltaX),
                "X−" => Math.Max(0, -snapshot.Pointer.DeltaX),
                "Y+" => Math.Max(0, snapshot.Pointer.DeltaY),
                "Y−" => Math.Max(0, -snapshot.Pointer.DeltaY),
                "Left" => snapshot.Pointer.Left ? 1 : 0,
                "Right" => snapshot.Pointer.Right ? 1 : 0,
                "Middle" => snapshot.Pointer.Middle ? 1 : 0,
                "XButton1" => snapshot.Pointer.ExtendedButton1 ? 1 : 0,
                "XButton2" => snapshot.Pointer.ExtendedButton2 ? 1 : 0,
                "WheelUp" => snapshot.Pointer.Wheel > 0 ? 1 : 0,
                "WheelDown" => snapshot.Pointer.Wheel < 0 ? 1 : 0,
                "WheelLeft" => snapshot.Pointer.HorizontalWheel < 0 ? 1 : 0,
                "WheelRight" => snapshot.Pointer.HorizontalWheel > 0 ? 1 : 0,
                _ => 0
            };
        if (!mapping.StartsWith(InputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase))
            return 0;
        var value = EmulationInputMappingFunctions.ControllerSourceValue(mapping, controller);
        if (Math.Abs(value) > 1)
            value /= short.MaxValue;
        return value <= (float)deadZone / InputMappingConstants.PercentScale ? 0 : Math.Clamp(value, 0, 1);
    }
}