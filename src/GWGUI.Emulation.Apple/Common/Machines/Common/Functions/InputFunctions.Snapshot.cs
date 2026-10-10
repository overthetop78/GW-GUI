namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot,
        InputConfiguration? configuration, MachineModel model)
    {
        var keyboardMappings = InputSettingsFunctions.DefaultHostKeys(model)
            .ToDictionary(item => item.Key.ToString(), item => item.Value, StringComparer.Ordinal);
        foreach (var mapping in configuration?.KeyboardMappings
            ?? new Dictionary<string, EmulationKey>())
            keyboardMappings[mapping.Key] = mapping.Value;
        var mapped = snapshot with
        {
            Keys = EmulationInputMappingFunctions.MapKeyboard(snapshot.Keys, keyboardMappings)
        };
        // Alt is the native transport for Apple/Command, not an additional immutable alias.
        if (keyboardMappings.Count > 0)
        {
            var keys = new HashSet<EmulationKey>(mapped.Keys);
            keys.Remove(EmulationKey.LeftAlt);
            keys.Remove(EmulationKey.RightAlt);
            mapped = mapped with { Keys = keys };
        }
        if (configuration?.MouseMappings?.TryGetValue(InputSnapshotFunctionsConstants.Left,
                out var mouseSource) == true)
        {
            var controller = EmulationInputMappingFunctions.ResolveSourceController(mouseSource,
                snapshot.Controllers, EmulationControllerState.Empty);
            mapped = mapped with
            {
                Pointer = mapped.Pointer with { Left = IsPressed(mouseSource, controller, snapshot) }
            };
        }
        if (configuration?.Controllers is not { Count: > 0 }) return mapped;
        var controllers = Enumerable.Repeat(EmulationControllerState.Empty,
            ControllerPortConstants.MaximumControllerPortCount).ToArray();
        foreach (var binding in configuration.Controllers)
        {
            if (binding.Port < ControllerPortConstants.MinimumControllerPort
                || binding.Port >= controllers.Length || binding.Peripheral == PeripheralCategory.None) continue;
            var source = EmulationInputMappingFunctions.ResolveController(binding.DeviceId,
                snapshot.Controllers, binding.Port);
            if (binding.Mappings is not { Count: > 0 })
            {
                controllers[binding.Port] = source;
                continue;
            }
            uint buttons = 0;
            var horizontalMapped = false;
            var verticalMapped = false;
            var horizontal = 0f;
            var vertical = 0f;
            foreach (var mapping in binding.Mappings)
            {
                var controller = EmulationInputMappingFunctions.ResolveSourceController(mapping.Value,
                    snapshot.Controllers, source);
                var target = InputSnapshotDictionary.CommonButtons.GetValueOrDefault(mapping.Key, -1);
                if (target < 0) continue;
                if (IsPressed(mapping.Value, controller, snapshot)) buttons |= 1u << target;
                var magnitude = Magnitude(mapping.Value, controller, snapshot);
                if (string.Equals(mapping.Key, InputSnapshotFunctionsConstants.Left, StringComparison.OrdinalIgnoreCase))
                {
                    horizontalMapped = true;
                    horizontal -= magnitude;
                }
                else if (string.Equals(mapping.Key, InputSnapshotFunctionsConstants.Right, StringComparison.OrdinalIgnoreCase))
                {
                    horizontalMapped = true;
                    horizontal += magnitude;
                }
                else if (string.Equals(mapping.Key, InputSnapshotFunctionsConstants.Up, StringComparison.OrdinalIgnoreCase))
                {
                    verticalMapped = true;
                    vertical -= magnitude;
                }
                else if (string.Equals(mapping.Key, InputSnapshotFunctionsConstants.Down, StringComparison.OrdinalIgnoreCase))
                {
                    verticalMapped = true;
                    vertical += magnitude;
                }
            }
            controllers[binding.Port] = source with
            {
                Buttons = buttons,
                LeftX = horizontalMapped ? Axis(horizontal) : source.LeftX,
                LeftY = verticalMapped ? Axis(vertical) : source.LeftY
            };
        }
        // Dead zones are applied once, by the native host, after port/device mapping.
        return mapped with { Controllers = controllers };
    }

    private static short Axis(float value) => (short)Math.Round(
        Math.Clamp(value, -1f, 1f) * ControllerConstants.MaximumAxisMagnitude);

    private static float Magnitude(string source, EmulationControllerState controller,
        EmulationInputSnapshot snapshot)
    {
        if (source.StartsWith(InputSnapshotFunctionsConstants.Keyboard, StringComparison.OrdinalIgnoreCase)
            || source.StartsWith(InputSnapshotFunctionsConstants.Mouse, StringComparison.OrdinalIgnoreCase))
            return IsPressed(source, controller, snapshot) ? 1f : 0f;
        var value = EmulationInputMappingFunctions.ControllerSourceValue(source, controller);
        var name = source.Split(EmulationInputMappingConstants.SourceSeparator,
            StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;
        if (!controller.Controls.ContainsKey(name) && name is
            EmulationInputMappingConstants.LeftStickLeft or EmulationInputMappingConstants.LeftStickRight
            or EmulationInputMappingConstants.LeftStickUp or EmulationInputMappingConstants.LeftStickDown
            or EmulationInputMappingConstants.RightStickLeft or EmulationInputMappingConstants.RightStickRight
            or EmulationInputMappingConstants.RightStickUp or EmulationInputMappingConstants.RightStickDown)
            value /= ControllerConstants.MaximumAxisMagnitude;
        return Math.Clamp(value, 0f, 1f);
    }

    private static bool IsPressed(string source, EmulationControllerState controller,
        EmulationInputSnapshot snapshot)
    {
        if (TryRemovePrefix(source, InputSnapshotFunctionsConstants.Keyboard, out var keyboard)
            && Enum.TryParse<EmulationKey>(keyboard, true, out var key)) return snapshot.Keys.Contains(key);
        if (TryRemovePrefix(source, InputSnapshotFunctionsConstants.Mouse, out var mouse))
            return mouse switch
            {
                InputSnapshotFunctionsConstants.Left => snapshot.Pointer.Left,
                InputSnapshotFunctionsConstants.Right => snapshot.Pointer.Right,
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
    {
        source = string.Empty;
        if (!value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return false;
        source = value[prefix.Length..];
        return source.Length > 0;
    }
}
