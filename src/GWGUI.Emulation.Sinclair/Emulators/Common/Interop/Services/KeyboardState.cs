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
            foreach (var code in _previous.Except(current)) keyboardEvent(false, code, KeyboardConstants.NoCharacter, modifiers);
            foreach (var code in current.Except(_previous)) keyboardEvent(true, code, KeyboardFunctions.Character(code, keys), modifiers);
        }
        _previous = current;
    }
}
