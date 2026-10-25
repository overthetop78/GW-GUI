using CommonControllerFunctions = GWGUI.Emulation.Atari.Common.Functions.ControllerFunctions;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static class KeyboardFunctions
{
    internal static IReadOnlyDictionary<EmulationKey, uint> CreateKeyMap() => CommonControllerFunctions.CreateNativeKeyMap();

    internal static ushort Modifiers(IReadOnlySet<EmulationKey> keys) => CommonControllerFunctions.Modifiers(keys);

    internal static uint Character(uint code, IReadOnlySet<EmulationKey> keys)
        => CommonControllerFunctions.Character(code, keys);

    internal static bool IsModifier(EmulationKey key) => key is EmulationKey.LeftShift or EmulationKey.RightShift or EmulationKey.LeftControl or EmulationKey.RightControl or EmulationKey.LeftAlt or EmulationKey.RightAlt or EmulationKey.LeftAmiga or EmulationKey.RightAmiga;

    internal static bool IsConsoleKeyActive(IReadOnlySet<EmulationKey> keys, uint buttonId) => buttonId switch
    {
        InputConstants.JoypadLeftShoulderId => keys.Contains(EmulationKey.AtariOption),
        InputConstants.JoypadSelectId => keys.Contains(EmulationKey.AtariSelect),
        InputConstants.JoypadStartId => keys.Contains(EmulationKey.AtariStart),
        InputConstants.JoypadRightTriggerId => keys.Contains(EmulationKey.Help),
        _ => false
    };
}
