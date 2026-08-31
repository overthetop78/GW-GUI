using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariInputSnapshotFunctions
{
    private static readonly IReadOnlyDictionary<string, int> CommonButtons =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [AtariInputSnapshotFunctionsConstants.Fire1] = 0, [AtariInputSnapshotFunctionsConstants.Fire2] = 8, [AtariInputSnapshotFunctionsConstants.Turbo] = 9,
            [AtariInputSnapshotFunctionsConstants.Up] = 4, [AtariInputSnapshotFunctionsConstants.Down] = 5, [AtariInputSnapshotFunctionsConstants.Left] = 6, [AtariInputSnapshotFunctionsConstants.Right] = 7,
            [AtariInputSnapshotFunctionsConstants.A] = 8, [AtariInputSnapshotFunctionsConstants.B] = 0, [AtariInputSnapshotFunctionsConstants.C] = 1,
            [AtariInputSnapshotFunctionsConstants.Pause] = 2, [AtariInputSnapshotFunctionsConstants.Option] = 3, [AtariInputSnapshotFunctionsConstants.Option1] = 10, [AtariInputSnapshotFunctionsConstants.Option2] = 11,
            [AtariInputSnapshotFunctionsConstants.Start] = 3, [AtariInputSnapshotFunctionsConstants.Reset] = 9,
            [AtariInputSnapshotFunctionsConstants.Key0] = 9, [AtariInputSnapshotFunctionsConstants.Key1] = 10, [AtariInputSnapshotFunctionsConstants.Key2] = 11, [AtariInputSnapshotFunctionsConstants.Key3] = 12,
            [AtariInputSnapshotFunctionsConstants.Key4] = 13, [AtariInputSnapshotFunctionsConstants.Key5] = 14, [AtariInputSnapshotFunctionsConstants.Key6] = 15,
            [AtariInputSnapshotFunctionsConstants.Star] = 1, [AtariInputSnapshotFunctionsConstants.Hash] = 8
        };

    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot,
        AtariInputConfiguration? configuration, AtariMachineModel model)
    {
        var mappedKeyboard = snapshot with
        {
            Keys = EmulationInputMappingFunctions.MapKeyboard(snapshot.Keys, configuration?.KeyboardMappings)
        };
        var withDeadZones = AtariControllerFunctions.ApplyDeadZones(mappedKeyboard, configuration?.Controllers);
        if (configuration?.Controllers is not { Count: > 0 }) return withDeadZones;
        var count = Math.Max(withDeadZones.Controllers.Count,
            configuration.Controllers.Max(binding => binding.Port) + 1);
        var controllers = Enumerable.Repeat(EmulationControllerState.Empty, count).ToArray();
        foreach (var binding in configuration.Controllers)
        {
            if (binding.Port < 0 || binding.Port >= controllers.Length) continue;
            var source = EmulationInputMappingFunctions.ResolveController(binding.DeviceId,
                withDeadZones.Controllers, binding.Port);
            if (binding.Mappings is not { Count: > 0 })
            {
                controllers[binding.Port] = source;
                continue;
            }
            uint buttons = 0;
            foreach (var mapping in binding.Mappings)
            {
                var target = ResolveButton(model, mapping.Key);
                var mappingController = ControllerForSource(
                    mapping.Value, withDeadZones.Controllers, source);
                if (target >= 0 && IsPressed(mapping.Value, mappingController,
                        withDeadZones.Keys, withDeadZones.Pointer))
                    buttons |= 1u << target;
            }
            controllers[binding.Port] = source with { Buttons = buttons };
        }
        return withDeadZones with { Controllers = controllers };
    }

    private static bool IsPressed(string sourceName, EmulationControllerState controller,
        IReadOnlySet<EmulationKey> keys, EmulationPointerState pointer)
    {
        if (TryRemovePrefix(sourceName, AtariInputSnapshotFunctionsConstants.Keyboard,
                out var keyboardSource)
            && Enum.TryParse<EmulationKey>(keyboardSource, true, out var key))
            return keys.Contains(key);
        if (TryRemovePrefix(sourceName, AtariInputSnapshotFunctionsConstants.Mouse,
                out var mouseSource))
            return PhysicalMouse(pointer).GetValueOrDefault(mouseSource);
        return EmulationInputMappingFunctions.IsControllerSourcePressed(sourceName, controller);
    }

    private static IReadOnlyDictionary<string, bool> PhysicalMouse(EmulationPointerState pointer) =>
        new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            [AtariInputSnapshotFunctionsConstants.Left] = pointer.Left,
            [AtariInputSnapshotFunctionsConstants.Right] = pointer.Right,
            [AtariInputSnapshotFunctionsConstants.Middle] = pointer.Middle,
            [AtariInputSnapshotFunctionsConstants.XButton1] = pointer.ExtendedButton1,
            [AtariInputSnapshotFunctionsConstants.XButton2] = pointer.ExtendedButton2,
            [AtariInputSnapshotFunctionsConstants.WheelUp] = pointer.Wheel > 0,
            [AtariInputSnapshotFunctionsConstants.WheelDown] = pointer.Wheel < 0,
            [AtariInputSnapshotFunctionsConstants.WheelLeft] = pointer.HorizontalWheel < 0,
            [AtariInputSnapshotFunctionsConstants.WheelRight] = pointer.HorizontalWheel > 0
        };

    private static bool TryRemovePrefix(string? value, string prefix, out string source)
    {
        source = string.Empty;
        if (string.IsNullOrWhiteSpace(value)
            || !value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;
        source = value[prefix.Length..];
        return source.Length > 0;
    }

    private static EmulationControllerState ControllerForSource(
        string source,
        IReadOnlyList<EmulationControllerState> controllers,
        EmulationControllerState fallback)
    {
        var deviceId = EmulationInputMappingFunctions.ParseControllerDeviceId(source);
        return deviceId is null ? fallback : controllers.FirstOrDefault(controller =>
            string.Equals(controller.DeviceId, deviceId, StringComparison.OrdinalIgnoreCase))
            ?? fallback;
    }

    private static int ResolveButton(AtariMachineModel model, string action)
    {
        if (model == AtariMachineModel.Lynx)
            return action switch
            {
                AtariInputSnapshotFunctionsConstants.Option1 => 10, AtariInputSnapshotFunctionsConstants.Option2 => 11, AtariInputSnapshotFunctionsConstants.Pause => 3,
                _ => CommonButtons.GetValueOrDefault(action, -1)
            };
        if (model == AtariMachineModel.Atari5200)
            return action switch
            {
                AtariInputSnapshotFunctionsConstants.Pause => 2, AtariInputSnapshotFunctionsConstants.Start => 3, AtariInputSnapshotFunctionsConstants.Key0 => 10, AtariInputSnapshotFunctionsConstants.Key1 => 11, AtariInputSnapshotFunctionsConstants.Key2 => 12,
                AtariInputSnapshotFunctionsConstants.Key3 => 13, AtariInputSnapshotFunctionsConstants.Key7 => 14, AtariInputSnapshotFunctionsConstants.Star => 9, AtariInputSnapshotFunctionsConstants.Hash => 1,
                _ => CommonButtons.GetValueOrDefault(action, -1)
            };
        return CommonButtons.GetValueOrDefault(action, -1);
    }
}
