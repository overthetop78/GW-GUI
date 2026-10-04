namespace GWGUI.Emulation.Nintendo.Common.Machines.Common.Functions;

internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot,
        string modelId, InputConfiguration? configuration, bool controllerPointerSwitchPressed)
    {
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
                configuration.ControllerBindings,
                modelId is ModelConstants.Nes or ModelConstants.FamicomDisk)
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
        IReadOnlyList<ControllerBinding>? bindings, bool nesController)
    {
        var result = new EmulationControllerState[4];
        for (var port = 0; port < result.Length; port++)
        {
            var binding = bindings?.FirstOrDefault(item => item.Port == port);
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
                var targetInKey = InputSnapshotDictionary.Nintendo64ButtonIndexes.TryGetValue(
                    mapping.Key, out var keyTarget)
                    || InputSnapshotDictionary.GameCubeButtonIndexes.TryGetValue(mapping.Key, out keyTarget)
                    || (nesController && InputSnapshotDictionary.NesButtonIndexes.TryGetValue(
                        mapping.Key, out keyTarget))
                    || InputSnapshotDictionary.ButtonIndexes.TryGetValue(mapping.Key, out keyTarget);
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
                LeftX = MappedAxis(binding.ButtonMappings,
                    InputSettingsFunctionsConstants.GameCubeStickRight,
                    InputSettingsFunctionsConstants.GameCubeStickLeft,
                    MappedAxis(binding.ButtonMappings,
                        InputSettingsFunctionsConstants.Nintendo64StickRight,
                        InputSettingsFunctionsConstants.Nintendo64StickLeft,
                        source.LeftX, source, physical, keys, physicalMouse),
                    source, physical, keys, physicalMouse),
                LeftY = MappedAxis(binding.ButtonMappings,
                    InputSettingsFunctionsConstants.GameCubeStickDown,
                    InputSettingsFunctionsConstants.GameCubeStickUp,
                    MappedAxis(binding.ButtonMappings,
                        InputSettingsFunctionsConstants.Nintendo64StickDown,
                        InputSettingsFunctionsConstants.Nintendo64StickUp,
                        source.LeftY, source, physical, keys, physicalMouse),
                    source, physical, keys, physicalMouse),
                RightX = MappedAxis(binding.ButtonMappings,
                    InputSettingsFunctionsConstants.GameCubeCRight,
                    InputSettingsFunctionsConstants.GameCubeCLeft,
                    MappedAxis(binding.ButtonMappings,
                        InputSettingsFunctionsConstants.Nintendo64CLeft,
                        InputSettingsFunctionsConstants.Nintendo64CRight,
                        source.RightX, source, physical, keys, physicalMouse),
                    source, physical, keys, physicalMouse),
                RightY = MappedAxis(binding.ButtonMappings,
                    InputSettingsFunctionsConstants.GameCubeCDown,
                    InputSettingsFunctionsConstants.GameCubeCUp,
                    MappedAxis(binding.ButtonMappings,
                        InputSettingsFunctionsConstants.Nintendo64CDown,
                        InputSettingsFunctionsConstants.Nintendo64CUp,
                        source.RightY, source, physical, keys, physicalMouse),
                    source, physical, keys, physicalMouse)
            };
        }
        return result;
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

        var positive = hasPositive && IsSourcePressed(positiveSource!,
            EmulationInputMappingFunctions.ResolveSourceController(positiveSource!, physical, controller),
            keys, mouse);
        var negative = hasNegative && IsSourcePressed(negativeSource!,
            EmulationInputMappingFunctions.ResolveSourceController(negativeSource!, physical, controller),
            keys, mouse);
        return positive == negative ? default
            : positive ? short.MaxValue : short.MinValue;
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
