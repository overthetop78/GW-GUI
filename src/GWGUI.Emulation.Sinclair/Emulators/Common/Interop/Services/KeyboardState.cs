using GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Constants;
namespace GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Services;

internal sealed class KeyboardState
{
    private readonly IReadOnlyDictionary<EmulationKey, uint> _map;
    private HashSet<uint> _previous = [];

    internal KeyboardState(Emulator emulator) => _map = KeyboardFunctions.CreateKeyMap(emulator);

    internal IReadOnlySet<uint> Codes { get; private set; } = new HashSet<uint>();

    internal void Publish(IReadOnlySet<EmulationKey> keys, ExternalCoreApi.KeyboardEvent? keyboardEvent)
    {
        var current = keys.Where(_map.ContainsKey).Select(key => _map[key]).ToHashSet();
        Codes = current;
        if (keyboardEvent is not null)
        {
            var modifiers = KeyboardFunctions.Modifiers(keys);
            ExternalCoreKeyboardFunctions.PublishCodeTransitions(current, ref _previous, keyboardEvent, modifiers,
                codes => codes,
                codes => codes,
                code => KeyboardFunctions.Character(code, keys));
        }
        _previous = current;
    }
}
