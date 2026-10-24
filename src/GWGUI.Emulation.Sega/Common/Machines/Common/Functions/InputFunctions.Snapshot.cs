using GWGUI.Emulation;
namespace GWGUI.Emulation.Sega.Common.Machines.Common.Functions;

internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration? configuration, bool controllerPointerSwitchPressed)
    {
        configuration ??= new InputConfiguration();
        var hostKeys = snapshot.Keys;
        var physicalMouse = PhysicalMouse(snapshot.Pointer);
        var mappedKeys = EmulationSdk.InputProcessing.MapKeyboard(snapshot, configuration.KeyboardBindings ?? configuration.KeyboardMappings?.ToDictionary(item => item.Key, item => item.Value.ToString(), StringComparer.Ordinal));
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
        IReadOnlyDictionary<string, bool> physicalMouse,
        EmulationControllerState fallbackController) => mappings.Any(mapping => mapping.Value == action && IsSourcePressed(mapping.Key, ControllerForSource(mapping.Key, controllers, fallbackController), keys, physicalMouse));

    private static IReadOnlyList<EmulationControllerState> MapControllers(
        IReadOnlyList<EmulationControllerState> physical, IReadOnlySet<EmulationKey> keys,
        IReadOnlyDictionary<string, bool> physicalMouse,
        IReadOnlyList<ControllerBinding>? bindings)
    {
        var result = new EmulationControllerState[4];
        for (var port = 0; port < result.Length; port++)
        {
            var binding = bindings?.FirstOrDefault(item => item.Port == port);
            var source = EmulationSdk.InputProcessing.ResolveController(binding?.DeviceId, physical, port);
            if (binding?.Mappings is not { Count: > 0 }
                || !binding.Mappings.Any(mapping => !string.IsNullOrWhiteSpace(mapping.Key) && !string.IsNullOrWhiteSpace(mapping.Value)))
            {
                result[port] = EmulationControllerState.Empty;
                continue;
            }
            uint buttons = 0;
            foreach (var mapping in binding.Mappings)
            {
                var targetInKey = InputSnapshotDictionary.ButtonIndexes.TryGetValue(mapping.Key, out var keyTarget);
                var target = targetInKey ? keyTarget : InputSnapshotDictionary.ButtonIndexes.GetValueOrDefault(mapping.Value, -1);
                var sourceName = targetInKey ? mapping.Value : mapping.Key;
                var mappingSource = EmulationSdk.InputProcessing.ResolveSourceController(sourceName, physical, source);
                if (target >= 0 && IsSourcePressed(sourceName, mappingSource, keys, physicalMouse))
                    buttons |= 1u << target;
            }
            var analog = binding.Peripheral is PeripheralCategory.SegaSaturnThreeDControlPad or PeripheralCategory.SegaDreamcastController or PeripheralCategory.SegaPaddleControl
                or PeripheralCategory.SegaSaturnMissionStick or PeripheralCategory.SegaSaturnArcadeRacer or PeripheralCategory.SegaXe1Ap or PeripheralCategory.SegaSportsPad
                or PeripheralCategory.SegaDreamcastFishingController or PeripheralCategory.SegaDreamcastMaracas;
            var vertical = analog && binding.Peripheral is not PeripheralCategory.SegaPaddleControl and not PeripheralCategory.SegaSaturnArcadeRacer;
            var throttle = binding.Peripheral is PeripheralCategory.SegaSaturnMissionStick or PeripheralCategory.SegaXe1Ap
                or PeripheralCategory.SegaDreamcastFishingController or PeripheralCategory.SegaDreamcastMaracas;
            result[port] = source with
            {
                Buttons = buttons,
                LeftTrigger = EmulationSdk.InputProcessing.MapControllerPressure(binding.Mappings, EmulationControllerCommandIds.L2, source, physical, keys, physicalMouse),
                RightTrigger = EmulationSdk.InputProcessing.MapControllerPressure(binding.Mappings, EmulationControllerCommandIds.R2, source, physical, keys, physicalMouse),
                LeftX = analog ? EmulationSdk.InputProcessing.MapControllerAxis(binding.Mappings, EmulationControllerCommandIds.LeftStickRight, EmulationControllerCommandIds.LeftStickLeft, source, physical, keys, physicalMouse) : (short)0,
                LeftY = vertical ? EmulationSdk.InputProcessing.MapControllerAxis(binding.Mappings, EmulationControllerCommandIds.LeftStickDown, EmulationControllerCommandIds.LeftStickUp, source, physical, keys, physicalMouse) : (short)0,
                RightX = throttle && binding.Peripheral != PeripheralCategory.SegaSaturnMissionStick
                    ? EmulationSdk.InputProcessing.MapControllerAxis(binding.Mappings, EmulationControllerCommandIds.RightStickRight, EmulationControllerCommandIds.RightStickLeft, source, physical, keys, physicalMouse) : (short)0,
                RightY = throttle ? EmulationSdk.InputProcessing.MapControllerAxis(binding.Mappings, EmulationControllerCommandIds.RightStickDown, EmulationControllerCommandIds.RightStickUp, source, physical, keys, physicalMouse) : (short)0
            };
        }
        return result;
    }

    private static IReadOnlyDictionary<string, bool> PhysicalMouse(EmulationPointerState pointer) => EmulationSdk.InputProcessing.PhysicalMouse(pointer);

    private static bool IsSourcePressed(string sourceName, EmulationControllerState controller, IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse)
    {
        if (sourceName.Contains('+' ) && !sourceName.EndsWith('+')) return EmulationSdk.InputProcessing.InputSourcePressure(sourceName, controller, [controller], keys, mouse) > .5f;
        if (TryRemovePrefix(sourceName, InputSnapshotFunctionsConstants.Controller, out var controllerSource))
        {
            var value = EmulationSdk.InputProcessing.ControllerSourceValue(controllerSource, controller);
            return EmulationSdk.InputProcessing.IsControllerSourcePressed(controllerSource, controller, value);
        }
        if (InputSnapshotDictionary.ButtonIndexes.TryGetValue(sourceName, out var legacyIndex))
            return (controller.Buttons & (1u << legacyIndex)) != 0;
        if (TryRemovePrefix(sourceName, InputSnapshotFunctionsConstants.Keyboard, out var keyboardSource)
            && Enum.TryParse<EmulationKey>(keyboardSource, true, out var key))
            return keys.Contains(key);
        return TryRemovePrefix(sourceName, InputSnapshotFunctionsConstants.Mouse, out var mouseSource) && mouse.GetValueOrDefault(mouseSource);
    }

    private static EmulationControllerState ControllerForSource(string source, IReadOnlyList<EmulationControllerState> controllers, EmulationControllerState fallback)
    => EmulationSdk.InputProcessing.ResolveSourceController(source, controllers, fallback, fallbackWhenMissing: true);

    private static bool TryRemovePrefix(string? value, string prefix, out string source)
    => EmulationSdk.InputProcessing.TryRemovePrefix(value, prefix, out source);
}
