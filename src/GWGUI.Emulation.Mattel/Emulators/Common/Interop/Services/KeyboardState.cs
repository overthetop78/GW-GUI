using GWGUI.Emulation.Mattel.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Mattel.Emulators.Common.Interop.Functions;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Mattel.Emulators.Common.Interop.Services;

internal sealed class KeyboardState
{
    private static readonly IReadOnlyDictionary<EmulationKey, uint> KeyMap = KeyboardFunctions.CreateKeyMap();
    private IReadOnlySet<uint> _previous = new HashSet<uint>();

    internal void Publish(IReadOnlySet<EmulationKey> keys, ExternalCoreApi.KeyboardEvent? keyboardEvent)
    {
        if (keyboardEvent is null) return;
        var modifiers = KeyboardFunctions.Modifiers(keys);
        var current = keys.Where(KeyMap.ContainsKey).Select(key => KeyMap[key]).ToHashSet();
        var modifierCodes = keys.Where(KeyboardFunctions.IsModifier).Select(key => KeyMap[key]).ToHashSet();
        foreach (var code in _previous.Except(current).OrderBy(code => IsModifier(code) ? KeyboardConstants.ModifierLastOrder : KeyboardConstants.ModifierFirstOrder))
            keyboardEvent(false, code, KeyboardConstants.NoCharacter, modifiers);
        foreach (var code in current.Except(_previous).OrderBy(code => modifierCodes.Contains(code) ? KeyboardConstants.ModifierFirstOrder : KeyboardConstants.ModifierLastOrder))
            keyboardEvent(true, code, KeyboardFunctions.Character(code, keys), modifiers);
        _previous = current;
    }

    private static bool IsModifier(uint code) => code is KeyboardConstants.LeftShift or KeyboardConstants.RightShift or KeyboardConstants.LeftControl or KeyboardConstants.RightControl or KeyboardConstants.LeftAlt or KeyboardConstants.RightAlt;

    internal static IReadOnlyDictionary<EmulationKey, uint> Mappings => KeyMap;
}
