namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Functions;
internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model, Emulator core)
    {
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
            uint buttons = 0;
            float horizontal = 0, vertical = 0;
            foreach (var command in ControllerCatalog.Commands(binding.Peripheral))
            {
                var mapping = binding.Mappings?.GetValueOrDefault(command.ToString()) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(mapping))
                    continue;
                var physical = EmulationInputMappingFunctions.ResolveSourceController(mapping, snapshot.Controllers, source);
                if (command == ControllerCommand.Fire && IsPressed(mapping, physical, snapshot))
                    buttons |= ControllerConstants.FireButtonMask;
                var magnitude = Magnitude(mapping, physical, snapshot, binding.DeadZonePercent);
                switch (command)
                {
                    case ControllerCommand.Left:
                        horizontal -= magnitude;
                        break;
                    case ControllerCommand.Right:
                        horizontal += magnitude;
                        break;
                    case ControllerCommand.Up:
                        vertical -= magnitude;
                        break;
                    case ControllerCommand.Down:
                        vertical += magnitude;
                        break;
                }
            }

            controllers[port] = EmulationControllerState.Empty with
            {
                DeviceId = source.DeviceId,
                Buttons = buttons,
                LeftX = Axis(horizontal),
                LeftY = Axis(vertical)
            };
        }

        var physicalPointerSource = EmulationInputMappingFunctions.ResolveController(null, snapshot.Controllers, ControllerConstants.FirstPort);
        var left = input.Mouse?.GetValueOrDefault(InputMappingConstants.MouseLeft, InputMappingConstants.MousePrefix + InputMappingConstants.MouseLeft) ?? InputMappingConstants.MousePrefix + InputMappingConstants.MouseLeft;
        var right = input.Mouse?.GetValueOrDefault(InputMappingConstants.MouseRight, InputMappingConstants.MousePrefix + InputMappingConstants.MouseRight) ?? InputMappingConstants.MousePrefix + InputMappingConstants.MouseRight;
        var pointer = snapshot.Pointer with
        {
            DeltaX = Scale(snapshot.Pointer.DeltaX, input.MouseSpeedPercent),
            DeltaY = Scale(snapshot.Pointer.DeltaY, input.MouseSpeedPercent),
            Left = IsPressed(left, EmulationInputMappingFunctions.ResolveSourceController(left, snapshot.Controllers, physicalPointerSource), snapshot),
            Right = IsPressed(right, EmulationInputMappingFunctions.ResolveSourceController(right, snapshot.Controllers, physicalPointerSource), snapshot)
        };
        return snapshot with
        {
            Keys = KeyboardDictionary.Apply(snapshot.Keys, input, model),
            Pointer = pointer,
            Controllers = controllers
        };
    }

    private static int Scale(int value, int percent) => (int)Math.Clamp(Math.Round((double)value * percent / InputMappingConstants.PercentScale), int.MinValue, int.MaxValue);
    private static short Axis(float value) => (short)(Math.Clamp(value, -1f, 1f) * short.MaxValue);
    private static float Magnitude(string mapping, EmulationControllerState controller, EmulationInputSnapshot snapshot, int deadZone)
    {
        if (mapping.Contains('+') && !mapping.EndsWith('+'))
        {
            var combined = EmulationInputMappingFunctions.InputSourcePressure(mapping, controller, snapshot);
            return combined <= (float)deadZone / InputMappingConstants.PercentScale ? 0f : Math.Clamp(combined, 0f, 1f);
        }
        if (!mapping.StartsWith(InputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase))
            return IsPressed(mapping, controller, snapshot) ? 1f : 0f;
        var value = EmulationInputMappingFunctions.ControllerSourcePressure(mapping, controller);
        return value <= (float)deadZone / InputMappingConstants.PercentScale ? 0f : Math.Clamp(value, 0f, 1f);
    }

    private static bool IsPressed(string mapping, EmulationControllerState controller, EmulationInputSnapshot snapshot)
    {
        if (mapping.Contains('+' ) && !mapping.EndsWith('+')) return EmulationInputMappingFunctions.InputSourcePressure(mapping, controller, snapshot) > .5f;
        if (mapping.StartsWith(InputMappingConstants.KeyboardPrefix, StringComparison.OrdinalIgnoreCase))
            return Enum.TryParse<EmulationKey>(mapping[InputMappingConstants.KeyboardPrefix.Length..], out var key) && snapshot.Keys.Contains(key);
        if (mapping.StartsWith(InputMappingConstants.MousePrefix, StringComparison.OrdinalIgnoreCase))
            return mapping[InputMappingConstants.MousePrefix.Length..] switch
            {
                InputMappingConstants.MouseLeft => snapshot.Pointer.Left,
                InputMappingConstants.MouseRight => snapshot.Pointer.Right,
                "Middle" => snapshot.Pointer.Middle,
                "XButton1" => snapshot.Pointer.ExtendedButton1,
                "XButton2" => snapshot.Pointer.ExtendedButton2,
                "WheelUp" => snapshot.Pointer.Wheel > 0,
                "WheelDown" => snapshot.Pointer.Wheel < 0,
                "WheelLeft" => snapshot.Pointer.HorizontalWheel < 0,
                "WheelRight" => snapshot.Pointer.HorizontalWheel > 0,
                _ => false
            };
        return mapping.StartsWith(InputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase) && EmulationInputMappingFunctions.IsControllerSourcePressed(mapping, controller);
    }
}