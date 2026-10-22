using GWGUI.Emulation;
namespace GWGUI.Emulation.Nintendo.Common.Machines.Common.Functions;

internal static class InputSnapshotFunctions
{
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, string modelId, InputConfiguration? configuration, bool controllerPointerSwitchPressed)
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
            Controllers = MapControllers(snapshot.Controllers, hostKeys, physicalMouse, configuration.ControllerBindings, modelId is ModelConstants.Nes or ModelConstants.FamicomDisk)
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
        IReadOnlyList<ControllerBinding>? bindings, bool nesController)
    {
        var result = new EmulationControllerState[4];
        for (var port = 0; port < result.Length; port++)
        {
            var binding = bindings?.FirstOrDefault(item => item.Port == port);
            var source = EmulationSdk.InputProcessing.ResolveController(binding?.DeviceId, physical, port);
            if (binding?.ButtonMappings is not { Count: > 0 }
                || !binding.ButtonMappings.Any(mapping => !string.IsNullOrWhiteSpace(mapping.Key) && !string.IsNullOrWhiteSpace(mapping.Value)))
            {
                result[port] = EmulationControllerState.Empty;
                continue;
            }
            uint buttons = 0;
            foreach (var mapping in binding.ButtonMappings)
            {
                var targetInKey = InputSnapshotDictionary.Nintendo64ButtonIndexes.TryGetValue(mapping.Key, out var keyTarget)
                    || InputSnapshotDictionary.GameCubeButtonIndexes.TryGetValue(mapping.Key, out keyTarget)
                    || (nesController && InputSnapshotDictionary.NesButtonIndexes.TryGetValue(mapping.Key, out keyTarget))
                    || InputSnapshotDictionary.ButtonIndexes.TryGetValue(mapping.Key, out keyTarget);
                var target = targetInKey ? keyTarget : InputSnapshotDictionary.ButtonIndexes.GetValueOrDefault(mapping.Value, -1);
                var sourceName = targetInKey ? mapping.Value : mapping.Key;
                var mappingSource = EmulationSdk.InputProcessing.ResolveSourceController(sourceName, physical, source);
                if (target >= 0 && IsSourcePressed(sourceName, mappingSource, keys, physicalMouse))
                    buttons |= 1u << target;
            }
            result[port] = source with
            {
                Buttons = buttons,
                LeftTrigger = (short)Math.Max(
                    EmulationSdk.InputProcessing.MapControllerPressure(binding.ButtonMappings, InputSettingsFunctionsConstants.GameCubeL, source, physical, keys, physicalMouse),
                    Math.Round(EmulationSdk.InputProcessing.MapControllerPressure(binding.ButtonMappings, InputSettingsFunctionsConstants.GameCubeLHalf, source, physical, keys, physicalMouse) / 2f)),
                RightTrigger = (short)Math.Max(
                    EmulationSdk.InputProcessing.MapControllerPressure(binding.ButtonMappings, InputSettingsFunctionsConstants.GameCubeR, source, physical, keys, physicalMouse),
                    Math.Round(EmulationSdk.InputProcessing.MapControllerPressure(binding.ButtonMappings, InputSettingsFunctionsConstants.GameCubeRHalf, source, physical, keys, physicalMouse) / 2f)),
                LeftX = MappedAxis(binding.ButtonMappings,
                    InputSettingsFunctionsConstants.GameCubeStickRight,
                    InputSettingsFunctionsConstants.GameCubeStickLeft,
                    MappedAxis(binding.ButtonMappings, InputSettingsFunctionsConstants.Nintendo64StickRight, InputSettingsFunctionsConstants.Nintendo64StickLeft, 0, source, physical, keys, physicalMouse),
                    source, physical, keys, physicalMouse),
                LeftY = MappedAxis(binding.ButtonMappings,
                    InputSettingsFunctionsConstants.GameCubeStickDown,
                    InputSettingsFunctionsConstants.GameCubeStickUp,
                    MappedAxis(binding.ButtonMappings, InputSettingsFunctionsConstants.Nintendo64StickDown, InputSettingsFunctionsConstants.Nintendo64StickUp, 0, source, physical, keys, physicalMouse),
                    source, physical, keys, physicalMouse),
                RightX = MappedAxis(binding.ButtonMappings,
                    InputSettingsFunctionsConstants.GameCubeCRight,
                    InputSettingsFunctionsConstants.GameCubeCLeft,
                    MappedAxis(binding.ButtonMappings, InputSettingsFunctionsConstants.Nintendo64CLeft, InputSettingsFunctionsConstants.Nintendo64CRight, 0, source, physical, keys, physicalMouse),
                    source, physical, keys, physicalMouse),
                RightY = MappedAxis(binding.ButtonMappings,
                    InputSettingsFunctionsConstants.GameCubeCDown,
                    InputSettingsFunctionsConstants.GameCubeCUp,
                    MappedAxis(binding.ButtonMappings, InputSettingsFunctionsConstants.Nintendo64CDown, InputSettingsFunctionsConstants.Nintendo64CUp, 0, source, physical, keys, physicalMouse),
                    source, physical, keys, physicalMouse)
            };
        }
        return result;
    }

    private static short MappedAxis(IReadOnlyDictionary<string, string> mappings,
        string positiveCommand, string negativeCommand, short fallbackValue,
        EmulationControllerState controller, IReadOnlyList<EmulationControllerState> physical,
        IReadOnlySet<EmulationKey> keys,
        IReadOnlyDictionary<string, bool> mouse)
    {
        var hasPositive = mappings.TryGetValue(positiveCommand, out var positiveSource) && !string.IsNullOrWhiteSpace(positiveSource);
        var hasNegative = mappings.TryGetValue(negativeCommand, out var negativeSource) && !string.IsNullOrWhiteSpace(negativeSource);
        if (!hasPositive && !hasNegative) return fallbackValue;

        return EmulationSdk.InputProcessing.MapControllerAxis(mappings, positiveCommand, negativeCommand, controller, physical, keys, mouse);
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
