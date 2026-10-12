namespace GWGUI.Emulation.Sharp.Emulators.Common.Interop.Services;
internal sealed class KeyboardState
{
    private static readonly EmulationKey[] ComposeKeys = [
        EmulationKey.SharpXf1,
        EmulationKey.SharpXf2,
        EmulationKey.SharpXf3,
        EmulationKey.SharpXf4,
        EmulationKey.SharpXf5,
        EmulationKey.SharpKana,
        EmulationKey.SharpRoman,
        EmulationKey.SharpCode,
        EmulationKey.SharpHiragana,
        EmulationKey.SharpFullWidth
    ];
    private readonly IReadOnlyDictionary<EmulationKey, uint> _map;
    private readonly bool _supportsCompose;
    private HashSet<uint> _previous = [];
    private bool _compose;
    internal KeyboardState(Emulator emulator)
    {
        _map = KeyboardFunctions.CreateKeyMap(emulator);
        _supportsCompose = emulator == Emulator.Px68k;
    }

    internal IReadOnlySet<uint> Codes { get; private set; } = new HashSet<uint>();

    internal void Publish(IReadOnlySet<EmulationKey> keys, ExternalCoreApi.KeyboardEvent? keyboardEvent)
    {
        var composed = new HashSet<uint>();
        if (_supportsCompose)
        {
            for (var index = 0; index < ComposeKeys.Length; index++)
            {
                if (keys.Contains(ComposeKeys[index]))
                    composed.Add(KeyboardConstants.FirstFunctionKey + checked((uint)index));
            }
        }

        var desiredCompose = composed.Count != 0;
        HashSet<uint> current;
        if (_compose != desiredCompose && _previous.Any(code => code != KeyboardConstants.Compose))
        {
            // The native mode must remain active for the frame that releases its keys.
            current = _compose ? [KeyboardConstants.Compose] : [];
        }
        else
        {
            _compose = desiredCompose;
            current = _compose ? composed : keys.Where(_map.ContainsKey).Select(key => _map[key]).ToHashSet();
            if (_compose)
                current.Add(KeyboardConstants.Compose);
        }

        Codes = current;
        if (keyboardEvent is not null)
        {
            var modifiers = KeyboardFunctions.Modifiers(keys);
            foreach (var code in _previous.Except(current))
                keyboardEvent(false, code, KeyboardConstants.NoCharacter, modifiers);
            foreach (var code in current.Except(_previous))
                keyboardEvent(true, code, _compose ? KeyboardConstants.NoCharacter : KeyboardFunctions.Character(code, keys), modifiers);
        }

        _previous = current;
    }
}