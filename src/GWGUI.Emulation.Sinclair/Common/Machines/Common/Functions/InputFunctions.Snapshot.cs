namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Functions;
internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration input, MachineModel model, Emulator core)
    {
        var controllers = new EmulationControllerState[ConfigurationFunctions.ControllerPortCount(model)];
        for (var port = ControllerConstants.FirstPort; port < controllers.Length; port++)
        {
            var binding = input.Controllers?.FirstOrDefault(item => item.Port == port) ?? new ControllerBinding(port, InputSettingsFunctions.DefaultPeripheral(model));
            var source = EmulationInputMappingFunctions.ResolveController(binding.DeviceId, snapshot.Controllers, port);
            uint buttons = 0;
            float horizontal = 0, vertical = 0;
            foreach (var action in ControllerProtocolDictionary.Actions(model, binding.Peripheral))
            {
                var mapping = binding.Mappings?.GetValueOrDefault(action.Id) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(mapping))
                    continue;
                var physical = EmulationInputMappingFunctions.ResolveSourceController(mapping, snapshot.Controllers, source);
                if (IsPressed(mapping, physical, snapshot))
                    buttons |= action.ButtonMask;
                var magnitude = Magnitude(mapping, physical, snapshot, binding.DeadZonePercent);
                switch (action.Command)
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

        var pointer = EmulationInputSnapshot.Empty.Pointer;
        if (core == Emulator.Fuse)
        {
            var physical = EmulationInputMappingFunctions.ResolveController(null, snapshot.Controllers, ControllerConstants.FirstPort);
            var left = input.Mouse?.GetValueOrDefault(InputMappingConstants.MouseLeft, InputMappingConstants.MousePrefix + InputMappingConstants.MouseLeft) ?? InputMappingConstants.MousePrefix + InputMappingConstants.MouseLeft;
            var right = input.Mouse?.GetValueOrDefault(InputMappingConstants.MouseRight, InputMappingConstants.MousePrefix + InputMappingConstants.MouseRight) ?? InputMappingConstants.MousePrefix + InputMappingConstants.MouseRight;
            pointer = pointer with
            {
                DeltaX = Scale(snapshot.Pointer.DeltaX, input.MouseSpeedPercent),
                DeltaY = Scale(snapshot.Pointer.DeltaY, input.MouseSpeedPercent),
                Left = IsPressed(left, EmulationInputMappingFunctions.ResolveSourceController(left, snapshot.Controllers, physical), snapshot),
                Right = IsPressed(right, EmulationInputMappingFunctions.ResolveSourceController(right, snapshot.Controllers, physical), snapshot)
            };
        }

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
        if (!mapping.StartsWith(InputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase))
            return IsPressed(mapping, controller, snapshot) ? 1f : 0f;
        var value = EmulationInputMappingFunctions.ControllerSourceValue(mapping, controller);
        if (Math.Abs(value) > 1f)
            value /= short.MaxValue;
        return value <= (float)deadZone / InputMappingConstants.PercentScale ? 0f : Math.Clamp(value, 0f, 1f);
    }

    private static bool IsPressed(string mapping, EmulationControllerState controller, EmulationInputSnapshot snapshot)
    {
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