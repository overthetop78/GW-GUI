using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Services;

internal sealed class AtariKeyboardState
{
    private static readonly IReadOnlyDictionary<EmulationKey, uint> KeyMap = AtariKeyboardFunctions.CreateKeyMap();
    private IReadOnlySet<EmulationKey> _previous = new HashSet<EmulationKey>();

    internal void Publish(IReadOnlySet<EmulationKey> keys, ExternalCoreApi.KeyboardEvent? keyboardEvent)
    {
        if (keyboardEvent is null) return;
        var modifiers = AtariKeyboardFunctions.Modifiers(keys);
        foreach (var key in _previous.Except(keys).OrderBy(key => AtariKeyboardFunctions.IsModifier(key)
                     ? AtariKeyboardConstants.ModifierLastOrder : AtariKeyboardConstants.ModifierFirstOrder))
            if (KeyMap.TryGetValue(key, out var code))
                keyboardEvent(false, code, AtariKeyboardConstants.NoCharacter, modifiers);
        foreach (var key in keys.Except(_previous).OrderBy(key => AtariKeyboardFunctions.IsModifier(key)
                     ? AtariKeyboardConstants.ModifierFirstOrder : AtariKeyboardConstants.ModifierLastOrder))
            if (KeyMap.TryGetValue(key, out var code))
                keyboardEvent(true, code, AtariKeyboardFunctions.Character(code, keys), modifiers);
        _previous = new HashSet<EmulationKey>(keys);
    }

    internal static IReadOnlyDictionary<EmulationKey, uint> Mappings => KeyMap;
}
