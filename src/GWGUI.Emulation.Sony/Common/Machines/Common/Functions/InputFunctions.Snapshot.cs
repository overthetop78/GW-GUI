namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot,
        InputConfiguration? configuration, bool controllerPointerSwitchPressed,
        int controllerPortCount = ControllerPortConstants.MaximumControllerPortCount)
    {
        if (controllerPortCount < ControllerPortConstants.MinimumControllerPort
            || controllerPortCount > ControllerPortConstants.MaximumControllerPortCount)
            throw new ArgumentOutOfRangeException(nameof(controllerPortCount), controllerPortCount, null);
        configuration ??= new InputConfiguration();
        var hostKeys = snapshot.Keys;
        var physicalMouse = PhysicalMouse(snapshot.Pointer);
        var mappedKeys = EmulationInputMappingFunctions.MapKeyboard(
            hostKeys, configuration.KeyboardMappings);
        if (controllerPointerSwitchPressed)
            mappedKeys = new HashSet<EmulationKey>(mappedKeys) { EmulationKey.RightControl };
        return snapshot with
        {
            Keys = mappedKeys,
            Pointer = MapPointer(snapshot.Pointer, snapshot.Controllers, hostKeys,
                physicalMouse, configuration.MouseButtonMappings),
            Controllers = MapControllers(snapshot.Controllers, hostKeys, physicalMouse,
                configuration.ControllerBindings, controllerPortCount)
        };
    }

    private static EmulationPointerState MapPointer(EmulationPointerState pointer,
        IReadOnlyList<EmulationControllerState> controllers, IReadOnlySet<EmulationKey> keys,
        IReadOnlyDictionary<string, bool> physicalMouse,
        IReadOnlyDictionary<string, MouseAction>? mappings)
    {
        mappings ??= InputSnapshotDictionary.DefaultMouseMappings;
        var fallbackController = controllers.FirstOrDefault() ?? EmulationControllerState.Empty;
        return pointer with
        {
            Left = IsMouseActionPressed(MouseAction.LeftButton, mappings, controllers,
                keys, physicalMouse, fallbackController),
            Right = IsMouseActionPressed(MouseAction.RightButton, mappings, controllers,
                keys, physicalMouse, fallbackController),
            Middle = IsMouseActionPressed(MouseAction.MiddleButton, mappings, controllers,
                keys, physicalMouse, fallbackController)
        };
    }

    private static bool IsMouseActionPressed(MouseAction action,
        IReadOnlyDictionary<string, MouseAction> mappings,
        IReadOnlyList<EmulationControllerState> controllers, IReadOnlySet<EmulationKey> keys,
        IReadOnlyDictionary<string, bool> physicalMouse,
        EmulationControllerState fallbackController) => mappings.Any(mapping =>
            mapping.Value == action && IsSourcePressed(mapping.Key,
                ControllerForSource(mapping.Key, controllers, fallbackController),
                keys, physicalMouse));

    private static IReadOnlyList<EmulationControllerState> MapControllers(
        IReadOnlyList<EmulationControllerState> physical, IReadOnlySet<EmulationKey> keys,
        IReadOnlyDictionary<string, bool> physicalMouse,
        IReadOnlyList<ControllerBinding>? bindings, int controllerPortCount)
    {
        var result = new EmulationControllerState[controllerPortCount];
        for (var port = ControllerPortConstants.MinimumControllerPort; port < result.Length; port++)
        {
            var binding = bindings?.FirstOrDefault(item => item.Port == port);
            if (binding?.Type == ControllerType.None)
            {
                result[port] = EmulationControllerState.Empty;
                continue;
            }
            var source = EmulationInputMappingFunctions.ResolveController(
                binding?.DeviceId, physical, port);
            if (binding?.ButtonMappings is not { Count: > 0 }
                || !binding.ButtonMappings.Any(mapping =>
                    !string.IsNullOrWhiteSpace(mapping.Key)
                    && !string.IsNullOrWhiteSpace(mapping.Value)))
            {
                result[port] = source;
                continue;
            }
            uint buttons = 0;
            foreach (var mapping in binding.ButtonMappings)
            {
                var targetInKey = InputSnapshotDictionary.ButtonIndexes.TryGetValue(mapping.Key, out var keyTarget);
                var target = targetInKey ? keyTarget
                    : InputSnapshotDictionary.ButtonIndexes.GetValueOrDefault(mapping.Value, -1);
                var sourceName = targetInKey ? mapping.Value : mapping.Key;
                var mappingSource = EmulationInputMappingFunctions.ResolveSourceController(
                    sourceName, physical, source);
                if (target >= 0 && IsSourcePressed(sourceName, mappingSource, keys, physicalMouse))
                    buttons |= 1u << target;
            }
            result[port] = source with
            {
                Buttons = buttons,
                Controls = MapControls(binding.ButtonMappings, source, physical, keys, physicalMouse),
                LeftX = MappedAxis(binding.ButtonMappings,
                    binding.ButtonMappings.ContainsKey(InputSettingsFunctionsConstants.LeftStickRight)
                        || binding.ButtonMappings.ContainsKey(InputSettingsFunctionsConstants.LeftStickLeft)
                        ? InputSettingsFunctionsConstants.LeftStickRight : InputSettingsFunctionsConstants.PspStickRight,
                    binding.ButtonMappings.ContainsKey(InputSettingsFunctionsConstants.LeftStickRight)
                        || binding.ButtonMappings.ContainsKey(InputSettingsFunctionsConstants.LeftStickLeft)
                        ? InputSettingsFunctionsConstants.LeftStickLeft : InputSettingsFunctionsConstants.PspStickLeft,
                    source.LeftX, source, physical, keys, physicalMouse),
                LeftY = MappedAxis(binding.ButtonMappings,
                    binding.ButtonMappings.ContainsKey(InputSettingsFunctionsConstants.LeftStickDown)
                        || binding.ButtonMappings.ContainsKey(InputSettingsFunctionsConstants.LeftStickUp)
                        ? InputSettingsFunctionsConstants.LeftStickDown : InputSettingsFunctionsConstants.PspStickDown,
                    binding.ButtonMappings.ContainsKey(InputSettingsFunctionsConstants.LeftStickDown)
                        || binding.ButtonMappings.ContainsKey(InputSettingsFunctionsConstants.LeftStickUp)
                        ? InputSettingsFunctionsConstants.LeftStickUp : InputSettingsFunctionsConstants.PspStickUp,
                    source.LeftY, source, physical, keys, physicalMouse),
                RightX = MappedAxis(binding.ButtonMappings,
                    InputSettingsFunctionsConstants.RightStickRight,
                    InputSettingsFunctionsConstants.RightStickLeft,
                    source.RightX, source, physical, keys, physicalMouse),
                RightY = MappedAxis(binding.ButtonMappings,
                    InputSettingsFunctionsConstants.RightStickDown,
                    InputSettingsFunctionsConstants.RightStickUp,
                    source.RightY, source, physical, keys, physicalMouse)
            };
        }
        return result;
    }

    private static EmulationControllerControls MapControls(IReadOnlyDictionary<string, string> mappings,
        EmulationControllerState source, IReadOnlyList<EmulationControllerState> physical,
        IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse)
    {
        var values = source.Controls.ToDictionary(item => item.Key, item => item.Value,
            StringComparer.OrdinalIgnoreCase);
        foreach (var mapping in mappings)
        {
            if (string.IsNullOrWhiteSpace(mapping.Key) || string.IsNullOrWhiteSpace(mapping.Value)) continue;
            values[mapping.Key] = SourcePressure(mapping.Value,
                EmulationInputMappingFunctions.ResolveSourceController(mapping.Value, physical, source), keys, mouse);
        }
        return new EmulationControllerControls(values);
    }

    private static float SourcePressure(string sourceName, EmulationControllerState controller,
        IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse)
    {
        if (TryRemovePrefix(sourceName, InputSnapshotFunctionsConstants.Controller, out var control))
        {
            var value = EmulationInputMappingFunctions.ControllerSourceValue(control, controller);
            if (Math.Abs(value) > InputSnapshotFunctionsConstants.FullPressure) value /= short.MaxValue;
            return Math.Clamp(value, InputSnapshotFunctionsConstants.ReleasedPressure,
                InputSnapshotFunctionsConstants.FullPressure);
        }
        return IsSourcePressed(sourceName, controller, keys, mouse)
            ? InputSnapshotFunctionsConstants.FullPressure : InputSnapshotFunctionsConstants.ReleasedPressure;
    }

    private static short MappedAxis(IReadOnlyDictionary<string, string> mappings,
        string positiveCommand, string negativeCommand, short physicalValue,
        EmulationControllerState controller, IReadOnlyList<EmulationControllerState> physical,
        IReadOnlySet<EmulationKey> keys,
        IReadOnlyDictionary<string, bool> mouse)
    {
        var hasPositive = mappings.TryGetValue(positiveCommand, out var positiveSource)
            && !string.IsNullOrWhiteSpace(positiveSource);
        var hasNegative = mappings.TryGetValue(negativeCommand, out var negativeSource)
            && !string.IsNullOrWhiteSpace(negativeSource);
        if (!hasPositive && !hasNegative) return physicalValue;

        var positive = hasPositive ? SourcePressure(positiveSource!,
            EmulationInputMappingFunctions.ResolveSourceController(positiveSource!, physical, controller),
            keys, mouse) : InputSnapshotFunctionsConstants.ReleasedPressure;
        var negative = hasNegative ? SourcePressure(negativeSource!,
            EmulationInputMappingFunctions.ResolveSourceController(negativeSource!, physical, controller),
            keys, mouse) : InputSnapshotFunctionsConstants.ReleasedPressure;
        var value = positive - negative;
        return (short)Math.Round(value >= InputSnapshotFunctionsConstants.ReleasedPressure
            ? value * short.MaxValue : -value * short.MinValue);
    }

    private static IReadOnlyDictionary<string, bool> PhysicalMouse(
        EmulationPointerState pointer) => new Dictionary<string, bool>(
            StringComparer.OrdinalIgnoreCase)
        {
            [InputSnapshotFunctionsConstants.Left] = pointer.Left,
            [InputSnapshotFunctionsConstants.Right] = pointer.Right,
            [InputSnapshotFunctionsConstants.Middle] = pointer.Middle,
            [InputSnapshotFunctionsConstants.XButton1] = pointer.ExtendedButton1,
            [InputSnapshotFunctionsConstants.XButton2] = pointer.ExtendedButton2,
            [InputSnapshotFunctionsConstants.WheelUp] = pointer.Wheel > 0,
            [InputSnapshotFunctionsConstants.WheelDown] = pointer.Wheel < 0,
            [InputSnapshotFunctionsConstants.WheelLeft] = pointer.HorizontalWheel < 0,
            [InputSnapshotFunctionsConstants.WheelRight] = pointer.HorizontalWheel > 0
        };

    private static bool IsSourcePressed(string sourceName,
        EmulationControllerState controller, IReadOnlySet<EmulationKey> keys,
        IReadOnlyDictionary<string, bool> mouse)
    {
        if (TryRemovePrefix(sourceName, InputSnapshotFunctionsConstants.Controller,
                out var controllerSource))
        {
            var value = EmulationInputMappingFunctions.ControllerSourceValue(
                controllerSource, controller);
            return EmulationInputMappingFunctions.IsControllerSourcePressed(
                controllerSource, controller, value);
        }
        if (InputSnapshotDictionary.ButtonIndexes.TryGetValue(sourceName, out var legacyIndex))
            return (controller.Buttons & (1u << legacyIndex)) != 0;
        if (TryRemovePrefix(sourceName, InputSnapshotFunctionsConstants.Keyboard,
                out var keyboardSource)
            && Enum.TryParse<EmulationKey>(keyboardSource, true, out var key))
            return keys.Contains(key);
        return TryRemovePrefix(sourceName, InputSnapshotFunctionsConstants.Mouse,
            out var mouseSource) && mouse.GetValueOrDefault(mouseSource);
    }

    private static EmulationControllerState ControllerForSource(string source,
        IReadOnlyList<EmulationControllerState> controllers,
        EmulationControllerState fallback)
    {
        var deviceId = EmulationInputMappingFunctions.ParseControllerDeviceId(source);
        return deviceId is null ? fallback : controllers.FirstOrDefault(controller =>
            string.Equals(controller.DeviceId, deviceId,
                StringComparison.OrdinalIgnoreCase)) ?? fallback;
    }

    private static bool TryRemovePrefix(string? value, string prefix, out string source)
    {
        source = string.Empty;
        if (string.IsNullOrWhiteSpace(value)
            || !value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return false;
        source = value[prefix.Length..];
        return source.Length > 0;
    }
}
