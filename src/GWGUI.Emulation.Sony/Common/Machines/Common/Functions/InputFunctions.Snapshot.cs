using GWGUI.Emulation;
namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, InputConfiguration? configuration, bool controllerPointerSwitchPressed, int controllerPortCount = ControllerPortConstants.MaximumGamepadPortCount)
    {
        if (controllerPortCount < ControllerPortConstants.MinimumControllerPort
            || controllerPortCount > ControllerPortConstants.MaximumControllerPortCount)
            throw new ArgumentOutOfRangeException(nameof(controllerPortCount), controllerPortCount, null);
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
            Controllers = MapControllers(snapshot.Controllers, hostKeys, physicalMouse, configuration.ControllerBindings, controllerPortCount)
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
        IReadOnlyList<ControllerBinding>? bindings, int controllerPortCount)
    {
        var result = new EmulationControllerState[controllerPortCount];
        for (var port = ControllerPortConstants.MinimumControllerPort; port < result.Length; port++)
        {
            var binding = bindings?.FirstOrDefault(item => item.Port == port);
            if (binding is { DeadZonePercent: < 0 or > 100 }) throw new ArgumentOutOfRangeException(nameof(bindings), nameof(ControllerBinding.DeadZonePercent));
            if (binding?.Peripheral == PeripheralCategory.None)
            {
                result[port] = EmulationControllerState.Empty;
                continue;
            }
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
            result[port] = EmulationSdk.InputProcessing.ApplyDeadZones(source, binding.DeadZonePercent) with
            {
                Buttons = buttons,
                LeftTrigger = EmulationSdk.InputProcessing.MapControllerPressure(binding.Mappings, EmulationControllerCommandIds.L2, source, physical, keys, physicalMouse, binding.DeadZonePercent),
                RightTrigger = EmulationSdk.InputProcessing.MapControllerPressure(binding.Mappings, EmulationControllerCommandIds.R2, source, physical, keys, physicalMouse, binding.DeadZonePercent),
                Controls = MapControls(binding.Mappings, source, physical, keys, physicalMouse, binding.DeadZonePercent),
                LeftX = MappedAxis(binding.Mappings,
                    binding.Mappings.ContainsKey(InputSettingsFunctionsConstants.LeftStickRight) || binding.Mappings.ContainsKey(InputSettingsFunctionsConstants.LeftStickLeft) ? InputSettingsFunctionsConstants.LeftStickRight : InputSettingsFunctionsConstants.PspStickRight,
                    binding.Mappings.ContainsKey(InputSettingsFunctionsConstants.LeftStickRight) || binding.Mappings.ContainsKey(InputSettingsFunctionsConstants.LeftStickLeft) ? InputSettingsFunctionsConstants.LeftStickLeft : InputSettingsFunctionsConstants.PspStickLeft,
                    source, physical, keys, physicalMouse, binding.DeadZonePercent),
                LeftY = MappedAxis(binding.Mappings,
                    binding.Mappings.ContainsKey(InputSettingsFunctionsConstants.LeftStickDown) || binding.Mappings.ContainsKey(InputSettingsFunctionsConstants.LeftStickUp) ? InputSettingsFunctionsConstants.LeftStickDown : InputSettingsFunctionsConstants.PspStickDown,
                    binding.Mappings.ContainsKey(InputSettingsFunctionsConstants.LeftStickDown) || binding.Mappings.ContainsKey(InputSettingsFunctionsConstants.LeftStickUp) ? InputSettingsFunctionsConstants.LeftStickUp : InputSettingsFunctionsConstants.PspStickUp,
                    source, physical, keys, physicalMouse, binding.DeadZonePercent),
                RightX = MappedAxis(binding.Mappings, InputSettingsFunctionsConstants.RightStickRight, InputSettingsFunctionsConstants.RightStickLeft, source, physical, keys, physicalMouse, binding.DeadZonePercent),
                RightY = MappedAxis(binding.Mappings, InputSettingsFunctionsConstants.RightStickDown, InputSettingsFunctionsConstants.RightStickUp, source, physical, keys, physicalMouse, binding.DeadZonePercent)
            };
        }
        return result;
    }

    private static EmulationControllerControls MapControls(IReadOnlyDictionary<string, string> mappings,
        EmulationControllerState source, IReadOnlyList<EmulationControllerState> physical,
        IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse, int deadZone = 0)
    {
        var values = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
        foreach (var mapping in mappings)
        {
            if (string.IsNullOrWhiteSpace(mapping.Key) || string.IsNullOrWhiteSpace(mapping.Value)) continue;
            var value = SourcePressure(mapping.Value, EmulationSdk.InputProcessing.ResolveSourceController(mapping.Value, physical, source), keys, mouse);
            var analog = !InputSnapshotDictionary.ButtonIndexes.ContainsKey(mapping.Key) || mapping.Key is EmulationControllerCommandIds.L2 or EmulationControllerCommandIds.R2;
            var controllerSource = mapping.Value.Contains(InputSnapshotFunctionsConstants.Controller, StringComparison.OrdinalIgnoreCase)
                || !mapping.Value.Contains(':') && !mapping.Value.Contains('+');
            values[mapping.Key] = analog && controllerSource && deadZone > 0 ? EmulationSdk.InputProcessing.ApplyDeadZone(value, deadZone) : value;
        }
        return new EmulationControllerControls(values);
    }

    private static float SourcePressure(string sourceName, EmulationControllerState controller, IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse)
    {
        if (sourceName.Contains('+' ) && !sourceName.EndsWith('+')) return EmulationSdk.InputProcessing.InputSourcePressure(sourceName, controller, [controller], keys, mouse);
        if (TryRemovePrefix(sourceName, InputSnapshotFunctionsConstants.Controller, out var control))
        {
            return EmulationSdk.InputProcessing.ControllerSourcePressure(control, controller);
        }
        return IsSourcePressed(sourceName, controller, keys, mouse) ? InputSnapshotFunctionsConstants.FullPressure : InputSnapshotFunctionsConstants.ReleasedPressure;
    }

    private static short MappedAxis(IReadOnlyDictionary<string, string> mappings, string positiveCommand, string negativeCommand,
        EmulationControllerState controller, IReadOnlyList<EmulationControllerState> physical, IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse, int deadZone = 0) =>
        EmulationSdk.InputProcessing.MapControllerAxis(mappings, positiveCommand, negativeCommand, controller, physical, keys, mouse, deadZone);

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
