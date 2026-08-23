using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga;

internal static class AmigaInputSnapshotFunctions
{
    private static readonly IReadOnlyDictionary<string, int> ButtonIndexes =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["B"] = 0, ["Y"] = 1, ["Select"] = 2, ["Start"] = 3,
            ["Up"] = 4, ["Down"] = 5, ["Left"] = 6, ["Right"] = 7,
            ["A"] = 8, ["X"] = 9, ["L"] = 10, ["R"] = 11,
            ["L2"] = 12, ["R2"] = 13, ["L3"] = 14, ["R3"] = 15
        };

    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot,
        AmigaInputConfiguration? configuration, bool controllerPointerSwitchPressed)
    {
        configuration ??= new AmigaInputConfiguration();
        var hostKeys = snapshot.Keys;
        var physicalMouse = PhysicalMouse(snapshot.Pointer);
        return snapshot with
        {
            Keys = EmulationInputMappingFunctions.MapKeyboard(hostKeys, configuration.KeyboardMappings),
            Pointer = MapPointer(snapshot.Pointer, snapshot.Controllers, hostKeys, physicalMouse,
                configuration.MouseButtonMappings),
            Controllers = MapControllers(snapshot.Controllers, hostKeys, physicalMouse,
                configuration.ControllerBindings, controllerPointerSwitchPressed)
        };
    }

    private static EmulationPointerState MapPointer(EmulationPointerState pointer,
        IReadOnlyList<EmulationControllerState> controllers, IReadOnlySet<EmulationKey> keys,
        IReadOnlyDictionary<string, bool> physicalMouse,
        IReadOnlyDictionary<string, AmigaMouseAction>? mappings)
    {
        mappings ??= new Dictionary<string, AmigaMouseAction>(StringComparer.OrdinalIgnoreCase)
        {
            ["Mouse:Left"] = AmigaMouseAction.LeftButton,
            ["Mouse:Right"] = AmigaMouseAction.RightButton,
            ["Mouse:Middle"] = AmigaMouseAction.MiddleButton
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
        IReadOnlyList<AmigaControllerBinding>? bindings, bool controllerPointerSwitchPressed)
    {
        var result = new EmulationControllerState[4];
        for (var port = 0; port < result.Length; port++)
        {
            var binding = bindings?.FirstOrDefault(item => item.Port == port + 1)
                ?? bindings?.FirstOrDefault(item => item.Port == port);
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
        if (controllerPointerSwitchPressed)
            result[0] = result[0] with { Buttons = result[0].Buttons | (1u << 2) };
        return result;
    }

    private static IReadOnlyDictionary<string, bool> PhysicalMouse(EmulationPointerState pointer) =>
        new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            ["Left"] = pointer.Left,
            ["Right"] = pointer.Right,
            ["Middle"] = pointer.Middle,
            ["XButton1"] = pointer.ExtendedButton1,
            ["XButton2"] = pointer.ExtendedButton2,
            ["WheelUp"] = pointer.Wheel > 0,
            ["WheelDown"] = pointer.Wheel < 0,
            ["WheelLeft"] = pointer.HorizontalWheel < 0,
            ["WheelRight"] = pointer.HorizontalWheel > 0
        };

    private static bool IsSourcePressed(string sourceName, EmulationControllerState controller,
        IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse)
    {
        if (TryRemovePrefix(sourceName, "Controller:", out var controllerSource))
            return EmulationInputMappingFunctions.IsControllerSourcePressed(controllerSource, controller);
        if (ButtonIndexes.TryGetValue(sourceName, out var legacyIndex))
            return (controller.Buttons & (1u << legacyIndex)) != 0;
        if (TryRemovePrefix(sourceName, "Keyboard:", out var keyboardSource)
            && Enum.TryParse<EmulationKey>(keyboardSource, true, out var key)) return keys.Contains(key);
        return TryRemovePrefix(sourceName, "Mouse:", out var mouseSource) && mouse.GetValueOrDefault(mouseSource);
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
