namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration? configuration, MachineModel model)
    {
        var keyboardMappings = InputSettingsFunctions.DefaultHostKeys(model).ToDictionary(item => item.Key.ToString(), item => item.Value.ToString(), StringComparer.Ordinal);
        var bindings = configuration?.KeyboardBindings ?? configuration?.KeyboardMappings?.ToDictionary(item => item.Key, item => item.Value.ToString(), StringComparer.Ordinal);
        if (bindings is not null)
            foreach (var mapping in bindings)
                keyboardMappings[mapping.Key] = mapping.Value;
        var mapped = snapshot with
        {
            Keys = EmulationInputMappingFunctions.MapKeyboard(snapshot, keyboardMappings)
        };
        // Alt is the native transport for Apple/Command, not an additional immutable alias.
        if (InputSettingsFunctions.DefaultHostKeys(model).Count > 0)
        {
            var keys = new HashSet<EmulationKey>(mapped.Keys);
            keys.Remove(EmulationKey.LeftAlt);
            keys.Remove(EmulationKey.RightAlt);
            mapped = mapped with { Keys = keys };
        }
        if (configuration?.MouseMappings?.TryGetValue(EmulationControllerCommandIds.Left, out var mouseSource) == true)
        {
            var controller = EmulationInputMappingFunctions.ResolveSourceController(mouseSource, snapshot.Controllers, EmulationControllerState.Empty);
            mapped = mapped with
            {
                Pointer = mapped.Pointer with { Left = IsPressed(mouseSource, controller, snapshot) }
            };
        }
        if (configuration?.MouseMappings?.TryGetValue(EmulationControllerCommandIds.Right, out var rightSource) == true)
        {
            var controller = EmulationInputMappingFunctions.ResolveSourceController(rightSource, snapshot.Controllers, EmulationControllerState.Empty);
            mapped = mapped with { Pointer = mapped.Pointer with { Right = IsPressed(rightSource, controller, snapshot) } };
        }
        if (configuration?.Controllers is not { Count: > 0 }) return mapped with { Controllers = [] };
        var controllers = Enumerable.Repeat(EmulationControllerState.Empty, ControllerPortConstants.MaximumControllerPortCount).ToArray();
        foreach (var binding in configuration.Controllers)
        {
            if (binding.Port < ControllerPortConstants.MinimumControllerPort
                || binding.Port >= controllers.Length || binding.Peripheral == PeripheralCategory.None) continue;
            var source = EmulationInputMappingFunctions.ResolveController(binding.DeviceId, snapshot.Controllers, binding.Port);
            if (binding.Mappings is not { Count: > 0 })
            {
                controllers[binding.Port] = EmulationControllerState.Empty;
                continue;
            }
            uint buttons = 0;
            var horizontalMapped = false;
            var verticalMapped = false;
            var horizontal = 0f;
            var vertical = 0f;
            foreach (var mapping in binding.Mappings)
            {
                var controller = EmulationInputMappingFunctions.ResolveSourceController(mapping.Value, snapshot.Controllers, source);
                var target = InputSnapshotDictionary.CommonButtons.GetValueOrDefault(mapping.Key, -1);
                if (target < 0) continue;
                if (IsPressed(mapping.Value, controller, snapshot)) buttons |= 1u << target;
                var magnitude = Magnitude(mapping.Value, controller, snapshot);
                if (string.Equals(mapping.Key, EmulationControllerCommandIds.Left, StringComparison.OrdinalIgnoreCase))
                {
                    horizontalMapped = true;
                    horizontal -= magnitude;
                }
                else if (string.Equals(mapping.Key, EmulationControllerCommandIds.Right, StringComparison.OrdinalIgnoreCase))
                {
                    horizontalMapped = true;
                    horizontal += magnitude;
                }
                else if (string.Equals(mapping.Key, EmulationControllerCommandIds.Up, StringComparison.OrdinalIgnoreCase))
                {
                    verticalMapped = true;
                    vertical -= magnitude;
                }
                else if (string.Equals(mapping.Key, EmulationControllerCommandIds.Down, StringComparison.OrdinalIgnoreCase))
                {
                    verticalMapped = true;
                    vertical += magnitude;
                }
            }
            controllers[binding.Port] = source with
            {
                Buttons = buttons,
                LeftX = horizontalMapped ? Axis(horizontal) : (short)0,
                LeftY = verticalMapped ? Axis(vertical) : (short)0
            };
        }
        // Dead zones are applied once, by the native host, after port/device mapping.
        return mapped with { Controllers = controllers };
    }

    private static short Axis(float value) => (short)Math.Round(Math.Clamp(value, -1f, 1f) * ControllerConstants.MaximumAxisMagnitude);

    private static float Magnitude(string source, EmulationControllerState controller, EmulationInputSnapshot snapshot)
    {
        if (source.Contains('+' ) && !source.EndsWith('+')) return EmulationInputMappingFunctions.InputSourcePressure(source, controller, snapshot);
        if (source.StartsWith(InputSnapshotFunctionsConstants.Keyboard, StringComparison.OrdinalIgnoreCase)
            || source.StartsWith(InputSnapshotFunctionsConstants.Mouse, StringComparison.OrdinalIgnoreCase))
            return IsPressed(source, controller, snapshot) ? 1f : 0f;
        return EmulationInputMappingFunctions.ControllerSourcePressure(source, controller);
    }

    private static bool IsPressed(string source, EmulationControllerState controller, EmulationInputSnapshot snapshot)
    {
        if (source.Contains('+' ) && !source.EndsWith('+')) return EmulationInputMappingFunctions.InputSourcePressure(source, controller, snapshot) > .5f;
        if (TryRemovePrefix(source, InputSnapshotFunctionsConstants.Keyboard, out var keyboard)
            && Enum.TryParse<EmulationKey>(keyboard, true, out var key)) return snapshot.Keys.Contains(key);
        if (TryRemovePrefix(source, InputSnapshotFunctionsConstants.Mouse, out var mouse))
            return mouse switch
            {
                EmulationControllerCommandIds.Left => snapshot.Pointer.Left,
                EmulationControllerCommandIds.Right => snapshot.Pointer.Right,
                InputSnapshotFunctionsConstants.Middle => snapshot.Pointer.Middle,
                InputSnapshotFunctionsConstants.XButton1 => snapshot.Pointer.ExtendedButton1,
                InputSnapshotFunctionsConstants.XButton2 => snapshot.Pointer.ExtendedButton2,
                InputSnapshotFunctionsConstants.WheelUp => snapshot.Pointer.Wheel > 0,
                InputSnapshotFunctionsConstants.WheelDown => snapshot.Pointer.Wheel < 0,
                InputSnapshotFunctionsConstants.WheelLeft => snapshot.Pointer.HorizontalWheel < 0,
                InputSnapshotFunctionsConstants.WheelRight => snapshot.Pointer.HorizontalWheel > 0,
                _ => false
            };
        return EmulationInputMappingFunctions.IsControllerSourcePressed(source, controller);
    }

    private static bool TryRemovePrefix(string value, string prefix, out string source)
    => GWGUI.Emulation.Functions.EmulationInputMappingFunctions.TryRemovePrefix(value, prefix, out source);
}
