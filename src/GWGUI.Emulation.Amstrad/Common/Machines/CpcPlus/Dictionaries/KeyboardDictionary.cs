using GWGUI.Emulation;
using KeyboardText = GWGUI.Emulation.Amstrad.Common.Machines.CpcPlus.Constants.KeyboardConstants;

namespace GWGUI.Emulation.Amstrad.Common.Machines.CpcPlus.Dictionaries;

internal static class KeyboardDictionary
{
    internal static readonly IReadOnlyDictionary<EmulationKey, string> Labels =
        new Dictionary<EmulationKey, string>
        {
            [EmulationKey.Escape] = KeyboardText.Escape,
            [EmulationKey.Tab] = KeyboardText.Tab,
            [EmulationKey.Backspace] = KeyboardText.Delete,
            [EmulationKey.Delete] = KeyboardText.Clear,
            [EmulationKey.CapsLock] = KeyboardText.CapsLock,
            [EmulationKey.Up] = KeyboardText.Up,
            [EmulationKey.Down] = KeyboardText.Down,
            [EmulationKey.Left] = KeyboardText.Left,
            [EmulationKey.Right] = KeyboardText.Right,
            [EmulationKey.RightAlt] = KeyboardText.Copy,
            [EmulationKey.Numpad0] = KeyboardText.F0,
            [EmulationKey.Numpad1] = KeyboardText.F1,
            [EmulationKey.Numpad2] = KeyboardText.F2,
            [EmulationKey.Numpad3] = KeyboardText.F3,
            [EmulationKey.Numpad4] = KeyboardText.F4,
            [EmulationKey.Numpad5] = KeyboardText.F5,
            [EmulationKey.Numpad6] = KeyboardText.F6,
            [EmulationKey.Numpad7] = KeyboardText.F7,
            [EmulationKey.Numpad8] = KeyboardText.F8,
            [EmulationKey.Numpad9] = KeyboardText.F9,
            [EmulationKey.NumpadPeriod] = KeyboardText.FPeriod,
            [EmulationKey.NumpadEnter] = KeyboardText.Enter
        };
}
