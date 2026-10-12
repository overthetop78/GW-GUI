using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Constants;
using GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Functions;

namespace GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Services;

internal sealed class KeyboardState
{
    private readonly IReadOnlyDictionary<EmulationKey, uint> _map;
    private HashSet<uint> _previous = [];
    internal KeyboardState(MachineModel model) => _map = KeyboardFunctions.CreateKeyMap(model);
    internal void Publish(IReadOnlySet<EmulationKey> keys, ExternalCoreApi.KeyboardEvent? callback)
    {
        if (callback is null) return;
        var current = keys.Where(_map.ContainsKey).Select(key => _map[key]).ToHashSet();
        foreach (var code in _previous.Except(current)) callback(false, code, KeyboardConstants.NoCharacter, KeyboardConstants.NoModifiers);
        foreach (var code in current.Except(_previous)) callback(true, code, KeyboardConstants.NoCharacter, KeyboardConstants.NoModifiers);
        _previous = current;
    }
    internal void Release(ExternalCoreApi.KeyboardEvent? callback)
    {
        try { if (callback is not null) foreach (var code in _previous) callback(false, code, KeyboardConstants.NoCharacter, KeyboardConstants.NoModifiers); }
        finally { _previous.Clear(); }
    }
}