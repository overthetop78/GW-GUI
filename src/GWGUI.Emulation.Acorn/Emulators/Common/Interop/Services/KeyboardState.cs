using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Constants;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Acorn.Emulators.Common.Interop.Services;

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
        ExternalCoreKeyboardFunctions.PublishCodeTransitions(current, ref _previous, keyboardEvent, modifiers,
                codes => codes.OrderBy(code => IsModifier(code) ? ExternalCoreKeyboardConstants.ModifierLastOrder : ExternalCoreKeyboardConstants.ModifierFirstOrder),
                codes => codes.OrderBy(code => modifierCodes.Contains(code) ? ExternalCoreKeyboardConstants.ModifierFirstOrder : ExternalCoreKeyboardConstants.ModifierLastOrder),
                code => KeyboardFunctions.Character(code, keys));
        _previous = current;
    }

    private static bool IsModifier(uint code) => code is ExternalCoreKeyboardConstants.LeftShift or ExternalCoreKeyboardConstants.RightShift or ExternalCoreKeyboardConstants.LeftControl or ExternalCoreKeyboardConstants.RightControl;

    internal static IReadOnlyDictionary<EmulationKey, uint> Mappings => KeyMap;
}

