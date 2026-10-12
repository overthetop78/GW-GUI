namespace GWGUI.Emulation.Bandai.Emulators.Common.Functions;

internal static class InputFunctions
{
    internal static EmulationInputSnapshot Freeze(EmulationInputSnapshot? snapshot)
    {
        snapshot ??= EmulationInputSnapshot.Empty;
        return new(new HashSet<EmulationKey>(snapshot.Keys), snapshot.Pointer with { }, snapshot.Controllers.ToArray());
    }

    internal static short State(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id)
    {
        if (device != InputConstants.JoypadDevice || index != InputConstants.LeftAnalogIndex
            || port >= snapshot.Controllers.Count) return InputConstants.InactiveState;
        var buttons = snapshot.Controllers[checked((int)port)].Buttons;
        if (id == InputConstants.JoypadMaskId) return unchecked((short)(buttons & ushort.MaxValue));
        return id < InputConstants.MaximumJoypadButtonCount && (buttons & (1u << checked((int)id))) != 0 ? InputConstants.ActiveState : InputConstants.InactiveState;
    }

    internal static EmulationInputSnapshot Accumulate(EmulationInputSnapshot current, EmulationInputSnapshot update) => Freeze(update);

    internal static EmulationInputSnapshot ConsumeRelativePointer(EmulationInputSnapshot snapshot) => snapshot;
}
