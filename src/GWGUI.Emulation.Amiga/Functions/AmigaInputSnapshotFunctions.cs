using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Functions;

internal static class AmigaInputSnapshotFunctions
{
    private static readonly IReadOnlyDictionary<string, int> ButtonIndexes =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [AmigaInputSnapshotFunctionsConstants.B] = 0, [AmigaInputSnapshotFunctionsConstants.Y] = 1, [AmigaInputSnapshotFunctionsConstants.Select] = 2, [AmigaInputSnapshotFunctionsConstants.Start] = 3,
            [AmigaInputSnapshotFunctionsConstants.Up] = 4, [AmigaInputSnapshotFunctionsConstants.Down] = 5, [AmigaInputSnapshotFunctionsConstants.Left] = 6, [AmigaInputSnapshotFunctionsConstants.Right] = 7,
            [AmigaInputSnapshotFunctionsConstants.A] = 8, [AmigaInputSnapshotFunctionsConstants.X] = 9, [AmigaInputSnapshotFunctionsConstants.L] = 10, [AmigaInputSnapshotFunctionsConstants.R] = 11,
            [AmigaInputSnapshotFunctionsConstants.L2] = 12, [AmigaInputSnapshotFunctionsConstants.R2] = 13, [AmigaInputSnapshotFunctionsConstants.L3] = 14, [AmigaInputSnapshotFunctionsConstants.R3] = 15
        };

    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot,
        AmigaInputConfiguration? configuration, bool controllerPointerSwitchPressed)
    {
        configuration ??= new AmigaInputConfiguration();
        var hostKeys = snapshot.Keys;
        var physicalMouse = PhysicalMouse(snapshot.Pointer);
        var mappedKeys = EmulationInputMappingFunctions.MapKeyboard(hostKeys, configuration.KeyboardMappings);
        if (controllerPointerSwitchPressed)
            mappedKeys = new HashSet<EmulationKey>(mappedKeys) { EmulationKey.RightControl };
        return snapshot with
        {
            Keys = mappedKeys,
            Pointer = MapPointer(snapshot.Pointer, snapshot.Controllers, hostKeys, physicalMouse,
                configuration.MouseButtonMappings),
            Controllers = MapControllers(snapshot.Controllers, hostKeys, physicalMouse,
                configuration.ControllerBindings)
        };
    }

    private static EmulationPointerState MapPointer(EmulationPointerState pointer,
        IReadOnlyList<EmulationControllerState> controllers, IReadOnlySet<EmulationKey> keys,
        IReadOnlyDictionary<string, bool> physicalMouse,
        IReadOnlyDictionary<string, AmigaMouseAction>? mappings)
    {
        mappings ??= new Dictionary<string, AmigaMouseAction>(StringComparer.OrdinalIgnoreCase)
        {
            [AmigaInputSnapshotFunctionsConstants.MouseLeft] = AmigaMouseAction.LeftButton,
            [AmigaInputSnapshotFunctionsConstants.MouseRight] = AmigaMouseAction.RightButton,
            [AmigaInputSnapshotFunctionsConstants.MouseMiddle] = AmigaMouseAction.MiddleButton
        };
        var fallbackController = controllers.FirstOrDefault() ?? EmulationControllerState.Empty;
        return pointer with
        {
            Left = IsMouseActionPressed(AmigaMouseAction.LeftButton, mappings, controllers, keys,
                physicalMouse, fallbackController),
            Right = IsMouseActionPressed(AmigaMouseAction.RightButton, mappings, controllers, keys,
                physicalMouse, fallbackController),
            Middle = IsMouseActionPressed(AmigaMouseAction.MiddleButton, mappings, controllers, keys,
                physicalMouse, fallbackController)
        };
    }

    private static bool IsMouseActionPressed(AmigaMouseAction action,
        IReadOnlyDictionary<string, AmigaMouseAction> mappings,
        IReadOnlyList<EmulationControllerState> controllers, IReadOnlySet<EmulationKey> keys,
        IReadOnlyDictionary<string, bool> physicalMouse, EmulationControllerState fallbackController) =>
        mappings.Any(mapping =>
            mapping.Value == action && IsSourcePressed(mapping.Key,
                ControllerForSource(mapping.Key, controllers, fallbackController), keys, physicalMouse));

    private static IReadOnlyList<EmulationControllerState> MapControllers(
        IReadOnlyList<EmulationControllerState> physical, IReadOnlySet<EmulationKey> keys,
        IReadOnlyDictionary<string, bool> physicalMouse,
        IReadOnlyList<AmigaControllerBinding>? bindings)
    {
        var result = new EmulationControllerState[4];
        for (var port = 0; port < result.Length; port++)
        {
            var binding = bindings?.FirstOrDefault(item => item.Port == port);
            var source = EmulationInputMappingFunctions.ResolveController(binding?.DeviceId, physical, port);
            if (binding?.ButtonMappings is not { Count: > 0 })
            {
                result[port] = source;
                continue;
            }
            uint buttons = 0;
            foreach (var mapping in binding.ButtonMappings)
            {
                var targetInKey = ButtonIndexes.TryGetValue(mapping.Key, out var keyTarget);
                var target = targetInKey ? keyTarget : ButtonIndexes.GetValueOrDefault(mapping.Value, -1);
                var sourceName = targetInKey ? mapping.Value : mapping.Key;
                if (target >= 0 && IsSourcePressed(sourceName, source, keys, physicalMouse))
                    buttons |= 1u << target;
            }
            result[port] = source with { Buttons = buttons };
        }
        return result;
    }

    private static IReadOnlyDictionary<string, bool> PhysicalMouse(EmulationPointerState pointer) =>
        new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            [AmigaInputSnapshotFunctionsConstants.Left] = pointer.Left,
            [AmigaInputSnapshotFunctionsConstants.Right] = pointer.Right,
            [AmigaInputSnapshotFunctionsConstants.Middle] = pointer.Middle,
            [AmigaInputSnapshotFunctionsConstants.XButton1] = pointer.ExtendedButton1,
            [AmigaInputSnapshotFunctionsConstants.XButton2] = pointer.ExtendedButton2,
            [AmigaInputSnapshotFunctionsConstants.WheelUp] = pointer.Wheel > 0,
            [AmigaInputSnapshotFunctionsConstants.WheelDown] = pointer.Wheel < 0,
            [AmigaInputSnapshotFunctionsConstants.WheelLeft] = pointer.HorizontalWheel < 0,
            [AmigaInputSnapshotFunctionsConstants.WheelRight] = pointer.HorizontalWheel > 0
        };

    private static bool IsSourcePressed(string sourceName, EmulationControllerState controller,
        IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse)
    {
        if (TryRemovePrefix(sourceName, AmigaInputSnapshotFunctionsConstants.Controller,
                out var controllerSource))
        {
            var value = EmulationInputMappingFunctions.ControllerSourceValue(
                controllerSource, controller);
            return EmulationInputMappingFunctions.IsControllerSourcePressed(
                controllerSource, controller, value);
        }
        if (ButtonIndexes.TryGetValue(sourceName, out var legacyIndex))
            return (controller.Buttons & (1u << legacyIndex)) != 0;
        if (TryRemovePrefix(sourceName, AmigaInputSnapshotFunctionsConstants.Keyboard, out var keyboardSource)
            && Enum.TryParse<EmulationKey>(keyboardSource, true, out var key)) return keys.Contains(key);
        return TryRemovePrefix(sourceName, AmigaInputSnapshotFunctionsConstants.Mouse, out var mouseSource) && mouse.GetValueOrDefault(mouseSource);
    }

    private static EmulationControllerState ControllerForSource(string source,
        IReadOnlyList<EmulationControllerState> controllers, EmulationControllerState fallback)
    {
        var deviceId = EmulationInputMappingFunctions.ParseControllerDeviceId(source);
        return deviceId is null ? fallback : controllers.FirstOrDefault(controller =>
            string.Equals(controller.DeviceId, deviceId, StringComparison.OrdinalIgnoreCase)) ?? fallback;
    }

    private static bool TryRemovePrefix(string? value, string prefix, out string source)
    {
        source = string.Empty;
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;
        source = value[prefix.Length..];
        return source.Length > 0;
    }
}
