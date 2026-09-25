using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Functions;

internal static class InputSnapshotFunctions
{
    private static readonly IReadOnlyDictionary<string, int> CommonButtons =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [InputSnapshotFunctionsConstants.Fire1] = 0, [InputSnapshotFunctionsConstants.Fire2] = 8, [InputSnapshotFunctionsConstants.Turbo] = 9,
            [InputSnapshotFunctionsConstants.Up] = 4, [InputSnapshotFunctionsConstants.Down] = 5, [InputSnapshotFunctionsConstants.Left] = 6, [InputSnapshotFunctionsConstants.Right] = 7,
            [InputSnapshotFunctionsConstants.A] = 8, [InputSnapshotFunctionsConstants.B] = 0, [InputSnapshotFunctionsConstants.C] = 1,
            [InputSnapshotFunctionsConstants.Pause] = 2, [InputSnapshotFunctionsConstants.Option] = 3, [InputSnapshotFunctionsConstants.Option1] = 10, [InputSnapshotFunctionsConstants.Option2] = 11,
            [InputSnapshotFunctionsConstants.Start] = 3, [InputSnapshotFunctionsConstants.Reset] = 9,
            [InputSnapshotFunctionsConstants.Key0] = 9, [InputSnapshotFunctionsConstants.Key1] = 10, [InputSnapshotFunctionsConstants.Key2] = 11, [InputSnapshotFunctionsConstants.Key3] = 12,
            [InputSnapshotFunctionsConstants.Key4] = 13, [InputSnapshotFunctionsConstants.Key5] = 14, [InputSnapshotFunctionsConstants.Key6] = 15,
            [InputSnapshotFunctionsConstants.Star] = 1, [InputSnapshotFunctionsConstants.Hash] = 8
        };

    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot,
        InputConfiguration? configuration, MachineModel model)
    {
        var mappedKeyboard = snapshot with
        {
            Keys = EmulationInputMappingFunctions.MapKeyboard(snapshot.Keys, configuration?.KeyboardMappings)
        };
        var withDeadZones = ControllerFunctions.ApplyDeadZones(mappedKeyboard, configuration?.Controllers);
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
        if (TryRemovePrefix(sourceName, InputSnapshotFunctionsConstants.Keyboard,
                out var keyboardSource)
            && Enum.TryParse<EmulationKey>(keyboardSource, true, out var key))
            return keys.Contains(key);
        if (TryRemovePrefix(sourceName, InputSnapshotFunctionsConstants.Mouse,
                out var mouseSource))
            return PhysicalMouse(pointer).GetValueOrDefault(mouseSource);
        return EmulationInputMappingFunctions.IsControllerSourcePressed(sourceName, controller);
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

    private static int ResolveButton(MachineModel model, string action)
    {
        if (model == MachineModel.Lynx)
            return action switch
            {
                InputSnapshotFunctionsConstants.Option1 => 10, InputSnapshotFunctionsConstants.Option2 => 11, InputSnapshotFunctionsConstants.Pause => 3,
                _ => CommonButtons.GetValueOrDefault(action, -1)
            };
        if (model == MachineModel.Atari5200)
            return action switch
            {
                InputSnapshotFunctionsConstants.Pause => 2, InputSnapshotFunctionsConstants.Start => 3, InputSnapshotFunctionsConstants.Key0 => 10, InputSnapshotFunctionsConstants.Key1 => 11, InputSnapshotFunctionsConstants.Key2 => 12,
                InputSnapshotFunctionsConstants.Key3 => 13, InputSnapshotFunctionsConstants.Key7 => 14, InputSnapshotFunctionsConstants.Star => 9, InputSnapshotFunctionsConstants.Hash => 1,
                _ => CommonButtons.GetValueOrDefault(action, -1)
            };
        return CommonButtons.GetValueOrDefault(action, -1);
    }
}
