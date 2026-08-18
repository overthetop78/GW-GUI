using System.Windows.Input;
using GWGUI.App.Input;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal static class AtariMachineInputFunctions
{
    internal static bool TryMap(Key source, out EmulationKey key)
    {
        key = source switch
        {
            Key.Back => EmulationKey.Backspace,
            Key.Tab => EmulationKey.Tab,
            Key.Enter => EmulationKey.Return,
            Key.Escape => EmulationKey.Escape,
            Key.Space => EmulationKey.Space,
            Key.Left => EmulationKey.Left,
            Key.Right => EmulationKey.Right,
            Key.Up => EmulationKey.Up,
            Key.Down => EmulationKey.Down,
            Key.LeftShift => EmulationKey.LeftShift,
            Key.RightShift => EmulationKey.RightShift,
            Key.LeftCtrl => EmulationKey.LeftControl,
            Key.RightCtrl => EmulationKey.RightControl,
            Key.LeftAlt => EmulationKey.LeftAlt,
            Key.RightAlt => EmulationKey.RightAlt,
            Key.Delete => EmulationKey.Delete,
            Key.Insert => EmulationKey.Insert,
            Key.Home => EmulationKey.Home,
            Key.End => EmulationKey.End,
            Key.PageUp => EmulationKey.PageUp,
            Key.PageDown => EmulationKey.PageDown,
            >= Key.A and <= Key.Z => (EmulationKey)((int)EmulationKey.A + source - Key.A),
            >= Key.D0 and <= Key.D9 => (EmulationKey)((int)EmulationKey.D0 + source - Key.D0),
            >= Key.F1 and <= Key.F10 => (EmulationKey)((int)EmulationKey.F1 + source - Key.F1),
            _ => EmulationKey.Unknown
        };
        return key != EmulationKey.Unknown;
    }

    internal static EmulationInputSnapshot Snapshot(IReadOnlySet<EmulationKey> keys,
        int deltaX, int deltaY, int wheel, bool mouseCaptured,
        IReadOnlyList<EmulationControllerState> controllers,
        AtariInputConfiguration? configuration = null,
        AtariMachineModel model = AtariMachineModel.St) => new(keys,
        new EmulationPointerState(
            mouseCaptured ? deltaX : RelativeMouseCaptureConstants.NoMovement,
            mouseCaptured ? deltaY : RelativeMouseCaptureConstants.NoMovement,
            mouseCaptured ? wheel : RelativeMouseCaptureConstants.NoMovement,
            mouseCaptured && RelativeMouseCapture.IsButtonPressed(WindowsInputMessages.LeftMouseVirtualKey),
            mouseCaptured && RelativeMouseCapture.IsButtonPressed(WindowsInputMessages.RightMouseVirtualKey),
            mouseCaptured && RelativeMouseCapture.IsButtonPressed(WindowsInputMessages.MiddleMouseVirtualKey)),
        ApplyControllerMappings(keys, controllers, configuration, model));

    internal static IReadOnlyList<EmulationControllerState> ApplyControllerMappings(
        IReadOnlySet<EmulationKey> keys, IReadOnlyList<EmulationControllerState> physical,
        AtariInputConfiguration? configuration, AtariMachineModel model = AtariMachineModel.St)
    {
        if (configuration?.Controllers is not { Count: > 0 }) return physical;
        var count = Math.Max(physical.Count, configuration.Controllers.Max(binding => binding.Port) + 1);
        var result = Enumerable.Repeat(EmulationControllerState.Empty, count).ToArray();
        foreach (var binding in configuration.Controllers)
        {
            if (binding.Port < 0 || binding.Port >= result.Length) continue;
            var sourcePort = ControllerInputMap.ParseXInputPort(binding.DeviceId, binding.Port);
            var source = sourcePort < physical.Count ? physical[sourcePort] : EmulationControllerState.Empty;
            if (binding.Mappings is not { Count: > 0 })
            {
                result[binding.Port] = source;
                continue;
            }
            uint buttons = 0;
            foreach (var mapping in binding.Mappings)
            {
                var target = ActionButtonId(model, mapping.Key);
                if (target < 0
                    || !IsPressed(mapping.Value, source, keys)) continue;
                buttons |= 1u << target;
            }
            result[binding.Port] = source with { Buttons = buttons };
        }
        return result;
    }

    private static bool IsPressed(string binding, EmulationControllerState controller,
        IReadOnlySet<EmulationKey> keys)
    {
        if (InputBindingSyntax.TryRemovePrefix(binding, InputBindingSyntax.ControllerPrefix, out var source))
            return ControllerInputMap.IsModernSourcePressed(source, controller);
        if (InputBindingSyntax.TryRemovePrefix(binding, InputBindingSyntax.KeyboardPrefix, out var keyName)
            && Enum.TryParse<EmulationKey>(keyName, true, out var key)) return keys.Contains(key);
        var legacy = Array.IndexOf(ControllerInputMap.LegacyButtonNames, binding);
        return legacy >= 0 && (controller.Buttons & (1u << legacy)) != 0;
    }

    private static int ActionButtonId(AtariMachineModel model, string action)
    {
        if (model == AtariMachineModel.Lynx)
            return action switch { "Option1" => 10, "Option2" => 11, "Pause" => 3, _ => CommonActionButtonIds.GetValueOrDefault(action, -1) };
        if (model == AtariMachineModel.Atari5200)
            return action switch
            {
                "Pause" => 2, "Start" => 3, "Key0" => 10, "Key1" => 11, "Key2" => 12,
                "Key3" => 13, "Key7" => 14, "Star" => 9, "Hash" => 1,
                _ => CommonActionButtonIds.GetValueOrDefault(action, -1)
            };
        return CommonActionButtonIds.GetValueOrDefault(action, -1);
    }

    private static readonly IReadOnlyDictionary<string, int> CommonActionButtonIds =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Fire1"] = 0, ["Fire2"] = 8, ["Turbo"] = 9,
            ["Up"] = 4, ["Down"] = 5, ["Left"] = 6, ["Right"] = 7,
            ["A"] = 8, ["B"] = 0, ["C"] = 1,
            ["Pause"] = 2, ["Option"] = 3, ["Option1"] = 10, ["Option2"] = 11,
            ["Start"] = 3, ["Reset"] = 9,
            ["Key0"] = 9, ["Key1"] = 10, ["Key2"] = 11, ["Key3"] = 12,
            ["Key4"] = 13, ["Key5"] = 14, ["Key6"] = 15,
            ["Star"] = 1, ["Hash"] = 8
        };

    internal static EmulationKey Resolve(EmulationKey hostKey,
        IReadOnlyDictionary<string, EmulationKey>? mappings)
    {
        if (mappings is null) return hostKey;
        var configured = mappings.FirstOrDefault(item => item.Value == hostKey);
        return !string.IsNullOrWhiteSpace(configured.Key)
            && Enum.TryParse<EmulationKey>(configured.Key, out var mapped) ? mapped : hostKey;
    }
}
