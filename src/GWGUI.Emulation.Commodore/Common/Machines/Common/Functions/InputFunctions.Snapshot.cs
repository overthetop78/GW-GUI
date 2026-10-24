using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Functions;

internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration? configuration, bool controllerPointerSwitchPressed)
    {
        configuration ??= new InputConfiguration();
        var hostKeys = snapshot.Keys;
        var physicalMouse = PhysicalMouse(snapshot.Pointer);
        var mappedKeys = EmulationSdk.InputProcessing.MapKeyboard(hostKeys, configuration.KeyboardMappings);
        if (controllerPointerSwitchPressed)
            mappedKeys = new HashSet<EmulationKey>(mappedKeys) { EmulationKey.RightControl };
        return snapshot with
        {
            Keys = mappedKeys,
            Pointer = MapPointer(snapshot.Pointer, snapshot.Controllers, hostKeys, physicalMouse, configuration.MouseButtonMappings),
            Controllers = MapControllers(snapshot.Controllers, hostKeys, physicalMouse, configuration.ControllerBindings)
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
            Left = IsMouseActionPressed(MouseAction.LeftButton, mappings, controllers, keys, physicalMouse, fallbackController),
            Right = IsMouseActionPressed(MouseAction.RightButton, mappings, controllers, keys, physicalMouse, fallbackController),
            Middle = IsMouseActionPressed(MouseAction.MiddleButton, mappings, controllers, keys, physicalMouse, fallbackController)
        };
    }

    private static bool IsMouseActionPressed(MouseAction action,
        IReadOnlyDictionary<string, MouseAction> mappings,
        IReadOnlyList<EmulationControllerState> controllers, IReadOnlySet<EmulationKey> keys,
        IReadOnlyDictionary<string, bool> physicalMouse, EmulationControllerState fallbackController) =>
        mappings.Any(mapping => mapping.Value == action && IsSourcePressed(mapping.Key, ControllerForSource(mapping.Key, controllers, fallbackController), keys, physicalMouse));

    private static IReadOnlyList<EmulationControllerState> MapControllers(
        IReadOnlyList<EmulationControllerState> physical, IReadOnlySet<EmulationKey> keys,
        IReadOnlyDictionary<string, bool> physicalMouse,
        IReadOnlyList<ControllerBinding>? bindings)
    {
        var result = new EmulationControllerState[ControllerPortConstants.MaximumControllerPortCount];
        for (var port = ControllerPortConstants.MinimumControllerPort; port < result.Length; port++)
        {
            var binding = bindings?.FirstOrDefault(item => item.Port == port);
            if (binding is { DeadZonePercent: < 0 or > 100 }) throw new ArgumentOutOfRangeException(nameof(bindings), nameof(ControllerBinding.DeadZonePercent));
            var source = EmulationSdk.InputProcessing.ResolveController(binding?.DeviceId, physical, port);
            if (binding?.Mappings is not { Count: > BufferConstants.EmptyCollectionCount })
            {
                result[port] = EmulationControllerState.Empty;
                continue;
            }
            uint buttons = InputSnapshotFunctionsConstants.NoPressedButtons;
            foreach (var mapping in binding.Mappings)
            {
                var targetInKey = InputSnapshotDictionary.ButtonIndexes.TryGetValue(mapping.Key, out var keyTarget);
                var target = targetInKey ? keyTarget : InputSnapshotDictionary.ButtonIndexes.GetValueOrDefault(mapping.Value, InputSnapshotFunctionsConstants.UnknownButtonIndex);
                var sourceName = targetInKey ? mapping.Value : mapping.Key;
                var mappingSource = EmulationSdk.InputProcessing.ResolveSourceController(sourceName, physical, source);
                if (target >= InputSnapshotFunctionsConstants.MinimumButtonIndex && IsSourcePressed(sourceName, mappingSource, keys, physicalMouse))
                    buttons |= InputSnapshotFunctionsConstants.SingleButtonMask << target;
            }
            result[port] = EmulationSdk.InputProcessing.ApplyDeadZones(source, binding.DeadZonePercent) with
            {
                Buttons = buttons,
                LeftX = binding.Peripheral == PeripheralCategory.AnalogJoystick
                    ? EmulationSdk.InputProcessing.MapControllerAxis(binding.Mappings, EmulationControllerCommandIds.Right, EmulationControllerCommandIds.Left, source, physical, keys, physicalMouse, binding.DeadZonePercent) : source.LeftX,
                LeftY = binding.Peripheral == PeripheralCategory.AnalogJoystick
                    ? EmulationSdk.InputProcessing.MapControllerAxis(binding.Mappings, EmulationControllerCommandIds.Down, EmulationControllerCommandIds.Up, source, physical, keys, physicalMouse, binding.DeadZonePercent) : source.LeftY
            };
        }
        return result;
    }

    private static IReadOnlyDictionary<string, bool> PhysicalMouse(EmulationPointerState pointer) =>
        EmulationSdk.InputProcessing.PhysicalMouse(pointer);

    private static bool IsSourcePressed(string sourceName, EmulationControllerState controller, IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse)
    {
        if (sourceName.Contains('+' ) && !sourceName.EndsWith('+')) return EmulationSdk.InputProcessing.InputSourcePressure(sourceName, controller, [controller], keys, mouse) > .5f;
        if (TryRemovePrefix(sourceName, InputSnapshotFunctionsConstants.Controller, out var controllerSource))
        {
            var value = EmulationSdk.InputProcessing.ControllerSourceValue(controllerSource, controller);
            return EmulationSdk.InputProcessing.IsControllerSourcePressed(controllerSource, controller, value);
        }
        if (InputSnapshotDictionary.ButtonIndexes.TryGetValue(sourceName, out var legacyIndex))
            return (controller.Buttons & (InputSnapshotFunctionsConstants.SingleButtonMask << legacyIndex)) != InputSnapshotFunctionsConstants.NoPressedButtons;
        if (TryRemovePrefix(sourceName, InputSnapshotFunctionsConstants.Keyboard, out var keyboardSource)
            && Enum.TryParse<EmulationKey>(keyboardSource, true, out var key)) return keys.Contains(key);
        return TryRemovePrefix(sourceName, InputSnapshotFunctionsConstants.Mouse, out var mouseSource) && mouse.GetValueOrDefault(mouseSource);
    }

    private static EmulationControllerState ControllerForSource(string source, IReadOnlyList<EmulationControllerState> controllers, EmulationControllerState fallback)
    => EmulationSdk.InputProcessing.ResolveSourceController(source, controllers, fallback, fallbackWhenMissing: true);

    private static bool TryRemovePrefix(string? value, string prefix, out string source)
    {
        source = string.Empty;
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;
        source = value[prefix.Length..];
        return !string.IsNullOrEmpty(source);
    }
}
