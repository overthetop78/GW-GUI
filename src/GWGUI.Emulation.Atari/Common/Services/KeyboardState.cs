using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed class KeyboardState
{
    private static readonly IReadOnlyDictionary<EmulationKey, uint> KeyMap = KeyboardFunctions.CreateKeyMap();
    private IReadOnlySet<EmulationKey> _previous = new HashSet<EmulationKey>();

    internal void Publish(IReadOnlySet<EmulationKey> keys, ExternalCoreApi.KeyboardEvent? keyboardEvent)
    {
        if (keyboardEvent is null) return;
        var modifiers = KeyboardFunctions.Modifiers(keys);
        foreach (var key in _previous.Except(keys).OrderBy(key => KeyboardFunctions.IsModifier(key)
                     ? KeyboardConstants.ModifierLastOrder : KeyboardConstants.ModifierFirstOrder))
            if (KeyMap.TryGetValue(key, out var code))
                keyboardEvent(false, code, KeyboardConstants.NoCharacter, modifiers);
        foreach (var key in keys.Except(_previous).OrderBy(key => KeyboardFunctions.IsModifier(key)
                     ? KeyboardConstants.ModifierFirstOrder : KeyboardConstants.ModifierLastOrder))
            if (KeyMap.TryGetValue(key, out var code))
                keyboardEvent(true, code, KeyboardFunctions.Character(code, keys), modifiers);
        _previous = new HashSet<EmulationKey>(keys);
    }

    internal static IReadOnlyDictionary<EmulationKey, uint> Mappings => KeyMap;
}
