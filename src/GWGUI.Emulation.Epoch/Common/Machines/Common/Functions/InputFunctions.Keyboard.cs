namespace GWGUI.Emulation.Epoch.Common.Machines.Common.Functions;

internal static class KeyboardFunctions
{
    internal static IReadOnlyDictionary<EmulationKey, uint> CreateKeyMap()
    {
        var map = new Dictionary<EmulationKey, uint>
        {
            [EmulationKey.Return] = KeyboardConstants.Return,
            [EmulationKey.NumpadEnter] = KeyboardConstants.KeypadEnter,
            [EmulationKey.Backspace] = KeyboardConstants.Backspace,
            [EmulationKey.Delete] = KeyboardConstants.Delete,
            [EmulationKey.EpochPower] = KeyboardConstants.Power,
            [EmulationKey.EpochReset] = KeyboardConstants.Reset
        };
        for (var digit = 0; digit < KeyboardConstants.DigitCount; digit++)
        {
            map[(EmulationKey)((int)EmulationKey.D0 + digit)] = KeyboardConstants.FirstDigit + (uint)digit;
            map[(EmulationKey)((int)EmulationKey.Numpad0 + digit)] = KeyboardConstants.FirstKeypadDigit + (uint)digit;
        }
        return map;
    }

    internal static ushort Modifiers(IReadOnlySet<EmulationKey> keys) => KeyboardConstants.NoModifiers;
    internal static bool IsModifier(EmulationKey key) => false;
    internal static uint Character(uint code, IReadOnlySet<EmulationKey> keys) =>
        code >= KeyboardConstants.FirstDigit && code < KeyboardConstants.FirstDigit + KeyboardConstants.DigitCount
            ? code : KeyboardConstants.NoCharacter;
}
