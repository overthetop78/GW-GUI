namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Functions;

internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration input,
        MachineModel model, Emulator core)
    {
        var keys = snapshot.Keys;
        if (model == MachineModel.Adam)
        {
            var bindings = InputSettingsFunctions.KeyboardBindings(input, model).Where(pair => !string.IsNullOrEmpty(pair.Value)).ToDictionary(
                pair => InputMappingConstants.AdamPrefix + pair.Key, pair => Enum.Parse<EmulationKey>(pair.Value));
            keys = EmulationInputMappingFunctions.MapKeyboard(keys, bindings);
        }
        var controllers = Enumerable.Repeat(EmulationControllerState.Empty, ControllerConstants.PortCount).ToArray();
        var pointer = snapshot.Pointer;
        for (var port = 0; port < controllers.Length; port++)
        {
            var binding = input.Controllers?.FirstOrDefault(item => item.Port == port) ?? new(port, PeripheralCategory.Standard);
            if (binding.Peripheral == PeripheralCategory.None) continue;
            var source = EmulationInputMappingFunctions.ResolveController(binding.DeviceId, snapshot.Controllers, port);
            var pressed = new HashSet<string>();
            foreach (var definition in InputSettingsFunctions.Definitions(binding.Peripheral))
            {
                var mapping = binding.Mappings?.GetValueOrDefault(definition.Id, definition.DefaultBinding) ?? definition.DefaultBinding;
                var physical = EmulationInputMappingFunctions.ResolveSourceController(mapping, snapshot.Controllers, source);
                if (IsPressed(mapping, physical, snapshot)) pressed.Add(definition.Id);
            }
            uint buttons = 0;
            short leftX = 0, leftY = 0, rightX = 0, rightY = 0;
            foreach (var action in pressed)
            {
                var index = ControllerProtocolDictionary.Button(core, action);
                if (index >= 0) buttons |= 1u << index;
                if (action.StartsWith(InputMappingConstants.KeypadPrefix, StringComparison.Ordinal) && int.TryParse(action[InputMappingConstants.KeypadPrefix.Length..], out var digit))
                {
                    if (core == Emulator.JollyCV)
                    {
                        var keyIndex = ControllerProtocolDictionary.JollyKeypad[digit];
                        buttons |= (1u << keyIndex) | (1u << (digit is >= 1 and <= 4 or 9 ? ControllerProtocolDictionary.JollyKeypadFirstModifier : ControllerProtocolDictionary.JollyKeypadSecondModifier));
                    }
                    else if (digit is >= 1 and <= 8)
                    {
                        var indices = core == Emulator.GearColeco ? ControllerProtocolDictionary.GearKeypad : ControllerProtocolDictionary.BlueKeypad;
                        buttons |= 1u << indices[digit - 1];
                    }
                    else if (core == Emulator.GearColeco)
                    {
                        if (digit == 0) leftX = InputMappingConstants.PositiveAxis;
                        else leftY = InputMappingConstants.PositiveAxis;
                    }
                    else rightX = digit == 9 ? InputMappingConstants.NegativeAxis : InputMappingConstants.PositiveAxis;
                }
                if (action == "Purple" && core == Emulator.GearColeco) rightY = InputMappingConstants.PositiveAxis;
                if (action == "Blue" && core == Emulator.GearColeco) rightX = InputMappingConstants.PositiveAxis;
                if (action == "Purple" && core == Emulator.BlueMSX) rightY = InputMappingConstants.NegativeAxis;
                if (action == "Blue" && core == Emulator.BlueMSX) rightY = InputMappingConstants.PositiveAxis;
            }
            if (core == Emulator.GearColeco && port == 0)
            {
                var spinner = (pressed.Contains("SpinnerRight") ? 1 : 0) - (pressed.Contains("SpinnerLeft") ? 1 : 0);
                pointer = pointer with { DeltaX = (int)Math.Clamp((long)pointer.DeltaX + spinner, int.MinValue, int.MaxValue) };
            }
            controllers[port] = source with { Buttons = buttons, LeftX = leftX, LeftY = leftY, RightX = rightX, RightY = rightY };
        }
        return snapshot with { Keys = keys, Controllers = controllers, Pointer = pointer };
    }

    private static bool IsPressed(string source, EmulationControllerState controller, EmulationInputSnapshot snapshot)
    {
        if (source.StartsWith(InputMappingConstants.KeyboardPrefix, StringComparison.OrdinalIgnoreCase))
            return Enum.TryParse<EmulationKey>(source[InputMappingConstants.KeyboardPrefix.Length..], out var key) && snapshot.Keys.Contains(key);
        if (source.StartsWith(InputMappingConstants.MousePrefix, StringComparison.OrdinalIgnoreCase)) return source[InputMappingConstants.MousePrefix.Length..] switch
        {
            "Left" => snapshot.Pointer.Left, "Right" => snapshot.Pointer.Right, "Middle" => snapshot.Pointer.Middle,
            "XButton1" => snapshot.Pointer.ExtendedButton1, "XButton2" => snapshot.Pointer.ExtendedButton2,
            "WheelUp" => snapshot.Pointer.Wheel > 0, "WheelDown" => snapshot.Pointer.Wheel < 0,
            "WheelLeft" => snapshot.Pointer.HorizontalWheel < 0, "WheelRight" => snapshot.Pointer.HorizontalWheel > 0,
            _ => false
        };
        return EmulationInputMappingFunctions.IsControllerSourcePressed(source, controller);
    }
}
