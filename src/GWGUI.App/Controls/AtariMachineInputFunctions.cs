using System.Windows.Input;
using GWGUI.App.Input;
using GWGUI.Emulation;

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
        IReadOnlyList<EmulationControllerState> controllers) => new(keys,
        new EmulationPointerState(
            mouseCaptured ? deltaX : RelativeMouseCaptureConstants.NoMovement,
            mouseCaptured ? deltaY : RelativeMouseCaptureConstants.NoMovement,
            mouseCaptured ? wheel : RelativeMouseCaptureConstants.NoMovement,
            mouseCaptured && RelativeMouseCapture.IsButtonPressed(WindowsInputMessages.LeftMouseVirtualKey),
            mouseCaptured && RelativeMouseCapture.IsButtonPressed(WindowsInputMessages.RightMouseVirtualKey),
            mouseCaptured && RelativeMouseCapture.IsButtonPressed(WindowsInputMessages.MiddleMouseVirtualKey)),
        controllers);

    internal static EmulationKey Resolve(EmulationKey hostKey,
        IReadOnlyDictionary<string, EmulationKey>? mappings)
    {
        if (mappings is null) return hostKey;
        var configured = mappings.FirstOrDefault(item => item.Value == hostKey);
        return !string.IsNullOrWhiteSpace(configured.Key)
            && Enum.TryParse<EmulationKey>(configured.Key, out var mapped) ? mapped : hostKey;
    }
}
