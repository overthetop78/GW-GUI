using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

internal static class AtariInputFunctions
{
    internal static EmulationInputSnapshot Freeze(EmulationInputSnapshot? snapshot)
    {
        snapshot ??= EmulationInputSnapshot.Empty;
        return new EmulationInputSnapshot(new HashSet<EmulationKey>(snapshot.Keys), snapshot.Pointer with { },
            snapshot.Controllers.ToArray());
    }

    internal static short State(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id)
    {
        if (device == AtariInputConstants.MouseDevice)
            return MouseState(snapshot.Pointer, port, index, id);
        if (port >= snapshot.Controllers.Count) return AtariInputConstants.InactiveState;
        var controller = snapshot.Controllers[checked((int)port)];
        if (device == AtariInputConstants.JoypadDevice)
        {
            if (index != AtariInputConstants.LeftAnalogIndex) return AtariInputConstants.InactiveState;
            if (id == AtariInputConstants.JoypadMaskId)
                return unchecked((short)(controller.Buttons & ushort.MaxValue));
            return id < AtariInputConstants.MaximumJoypadButtonCount &&
                   ((controller.Buttons & (1u << checked((int)id))) != AtariConstants.NoInputState ||
                    AtariKeyboardFunctions.IsConsoleKeyActive(snapshot.Keys, id))
                ? AtariInputConstants.ActiveState
                : AtariInputConstants.InactiveState;
        }
        if (device != AtariInputConstants.AnalogDevice) return AtariInputConstants.InactiveState;
        return (index, id) switch
        {
            (AtariInputConstants.LeftAnalogIndex, AtariInputConstants.AnalogXId) => controller.LeftX,
            (AtariInputConstants.LeftAnalogIndex, AtariInputConstants.AnalogYId) => controller.LeftY,
            (AtariInputConstants.RightAnalogIndex, AtariInputConstants.AnalogXId) => controller.RightX,
            (AtariInputConstants.RightAnalogIndex, AtariInputConstants.AnalogYId) => controller.RightY,
            _ => AtariInputConstants.InactiveState
        };
    }

    internal static EmulationInputSnapshot Accumulate(EmulationInputSnapshot current,
        EmulationInputSnapshot update) => new(new HashSet<EmulationKey>(update.Keys), update.Pointer with
    {
        DeltaX = SaturatingAdd(current.Pointer.DeltaX, update.Pointer.DeltaX),
        DeltaY = SaturatingAdd(current.Pointer.DeltaY, update.Pointer.DeltaY),
        Wheel = SaturatingAdd(current.Pointer.Wheel, update.Pointer.Wheel)
    }, update.Controllers.ToArray());

    internal static EmulationInputSnapshot ConsumeRelativePointer(EmulationInputSnapshot snapshot) =>
        snapshot with
        {
            Pointer = snapshot.Pointer with
            {
                DeltaX = AtariInputConstants.ConsumedRelativeValue,
                DeltaY = AtariInputConstants.ConsumedRelativeValue,
                Wheel = AtariInputConstants.ConsumedRelativeValue
            }
        };

    private static short MouseState(EmulationPointerState pointer, uint port, uint index, uint id)
    {
        if (port != AtariInputConstants.PrimaryPort || index != AtariInputConstants.LeftAnalogIndex)
            return AtariInputConstants.InactiveState;
        return id switch
        {
            AtariInputConstants.MouseXId => Clamp(pointer.DeltaX),
            AtariInputConstants.MouseYId => Clamp(pointer.DeltaY),
            AtariInputConstants.MouseLeftId => Boolean(pointer.Left),
            AtariInputConstants.MouseRightId => Boolean(pointer.Right),
            AtariInputConstants.MouseWheelUpId => Boolean(pointer.Wheel > AtariInputConstants.ConsumedRelativeValue),
            AtariInputConstants.MouseWheelDownId => Boolean(pointer.Wheel < AtariInputConstants.ConsumedRelativeValue),
            AtariInputConstants.MouseMiddleId => Boolean(pointer.Middle),
            _ => AtariInputConstants.InactiveState
        };
    }

    private static short Boolean(bool value) => value ? AtariInputConstants.ActiveState : AtariInputConstants.InactiveState;
    private static short Clamp(int value) => (short)Math.Clamp(value, short.MinValue, short.MaxValue);
    private static int SaturatingAdd(int left, int right) =>
        (int)Math.Clamp((long)left + right, int.MinValue, int.MaxValue);
}
