using CommonControllerFunctions = GWGUI.Emulation.Thomson.Common.Functions.ControllerFunctions;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Thomson.Emulators.Theodore.Interop.Constants;
using GWGUI.Emulation.Thomson.Emulators.Theodore.Interop.Functions;

namespace GWGUI.Emulation.Thomson.Emulators.Theodore.Interop.Services;

internal sealed class KeyboardState
{
    private const int ShiftedFunctionKeyCount = 5;
    private readonly MachineModel _model;
    private readonly IReadOnlyDictionary<EmulationKey, uint> _map;
    private HashSet<uint> _previous = [];
    internal KeyboardState(MachineModel model) { _model = model; _map = KeyboardFunctions.CreateKeyMap(model); }

    internal void Publish(IReadOnlySet<EmulationKey> keys, ExternalCoreApi.KeyboardEvent? callback)
    {
        if (callback is null) return;
        var current = new HashSet<uint>();
        foreach (var key in keys)
        {
            var shiftedFunction = (int)key - (int)EmulationKey.F6;
            var functionModifier = _model switch { MachineModel.Mo6 => KeyboardConstants.RightShift, MachineModel.To8 or MachineModel.To8D or MachineModel.To9 or MachineModel.To9Plus => KeyboardConstants.LeftShift, _ => (uint?)null };
            if (functionModifier is uint modifier && shiftedFunction >= 0 && shiftedFunction < ShiftedFunctionKeyCount) { current.Add(modifier); current.Add(KeyboardConstants.FirstFunctionKey + (uint)shiftedFunction); }
            else if (_map.TryGetValue(key, out var code)) current.Add(code);
        }
        var modifiers = KeyboardFunctions.Modifiers(keys);
        CommonControllerFunctions.PublishCodeTransitions(current, ref _previous, callback, modifiers,
                codes => codes.OrderBy(IsModifier),
                codes => codes.OrderByDescending(IsModifier),
                code => KeyboardConstants.NoCharacter);
        _previous = current;
    }

    internal void Release(ExternalCoreApi.KeyboardEvent? callback)
    {
        if (callback is not null) foreach (var code in _previous.OrderBy(IsModifier)) callback(false, code, KeyboardConstants.NoCharacter, KeyboardConstants.NoModifiers);
        _previous.Clear();
    }

    private static bool IsModifier(uint code) => code is KeyboardConstants.LeftShift or KeyboardConstants.RightShift or KeyboardConstants.LeftControl or KeyboardConstants.RightControl;
}
