using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Common.Functions;

internal static class InputSnapshotFunctions
{
    private static readonly IReadOnlyDictionary<string, int> ButtonIndexes =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [InputSnapshotFunctionsConstants.B] = 0, [InputSnapshotFunctionsConstants.Y] = 1, [InputSnapshotFunctionsConstants.Select] = 2, [InputSnapshotFunctionsConstants.Start] = 3,
            [InputSnapshotFunctionsConstants.Up] = 4, [InputSnapshotFunctionsConstants.Down] = 5, [InputSnapshotFunctionsConstants.Left] = 6, [InputSnapshotFunctionsConstants.Right] = 7,
            [InputSnapshotFunctionsConstants.A] = 8, [InputSnapshotFunctionsConstants.X] = 9, [InputSnapshotFunctionsConstants.L] = 10, [InputSnapshotFunctionsConstants.R] = 11,
            [InputSnapshotFunctionsConstants.L2] = 12, [InputSnapshotFunctionsConstants.R2] = 13, [InputSnapshotFunctionsConstants.L3] = 14, [InputSnapshotFunctionsConstants.R3] = 15
        };

    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot,
        InputConfiguration? configuration, bool controllerPointerSwitchPressed)
    {
        configuration ??= new InputConfiguration();
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
        IReadOnlyDictionary<string, MouseAction>? mappings)
    {
        mappings ??= new Dictionary<string, MouseAction>(StringComparer.OrdinalIgnoreCase)
        {
            [InputSnapshotFunctionsConstants.MouseLeft] = MouseAction.LeftButton,
            [InputSnapshotFunctionsConstants.MouseRight] = MouseAction.RightButton,
            [InputSnapshotFunctionsConstants.MouseMiddle] = MouseAction.MiddleButton
        };
        var fallbackController = controllers.FirstOrDefault() ?? EmulationControllerState.Empty;
        return pointer with
        {
            Left = IsMouseActionPressed(MouseAction.LeftButton, mappings, controllers, keys,
                physicalMouse, fallbackController),
            Right = IsMouseActionPressed(MouseAction.RightButton, mappings, controllers, keys,
                physicalMouse, fallbackController),
            Middle = IsMouseActionPressed(MouseAction.MiddleButton, mappings, controllers, keys,
                physicalMouse, fallbackController)
        };
    }

    private static bool IsMouseActionPressed(MouseAction action,
        IReadOnlyDictionary<string, MouseAction> mappings,
        IReadOnlyList<EmulationControllerState> controllers, IReadOnlySet<EmulationKey> keys,
        IReadOnlyDictionary<string, bool> physicalMouse, EmulationControllerState fallbackController) =>
        mappings.Any(mapping =>
            mapping.Value == action && IsSourcePressed(mapping.Key,
                ControllerForSource(mapping.Key, controllers, fallbackController), keys, physicalMouse));

    private static IReadOnlyList<EmulationControllerState> MapControllers(
        IReadOnlyList<EmulationControllerState> physical, IReadOnlySet<EmulationKey> keys,
        IReadOnlyDictionary<string, bool> physicalMouse,
        IReadOnlyList<ControllerBinding>? bindings)
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

    private static bool IsSourcePressed(string sourceName, EmulationControllerState controller,
        IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse)
    {
        if (TryRemovePrefix(sourceName, InputSnapshotFunctionsConstants.Controller,
                out var controllerSource))
        {
            var value = EmulationInputMappingFunctions.ControllerSourceValue(
                controllerSource, controller);
            return EmulationInputMappingFunctions.IsControllerSourcePressed(
                controllerSource, controller, value);
        }
        if (ButtonIndexes.TryGetValue(sourceName, out var legacyIndex))
            return (controller.Buttons & (1u << legacyIndex)) != 0;
        if (TryRemovePrefix(sourceName, InputSnapshotFunctionsConstants.Keyboard, out var keyboardSource)
            && Enum.TryParse<EmulationKey>(keyboardSource, true, out var key)) return keys.Contains(key);
        return TryRemovePrefix(sourceName, InputSnapshotFunctionsConstants.Mouse, out var mouseSource) && mouse.GetValueOrDefault(mouseSource);
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
