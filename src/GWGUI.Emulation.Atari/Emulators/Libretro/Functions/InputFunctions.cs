using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Emulators.Libretro.Functions;

internal static class InputFunctions
{
    private static readonly IReadOnlyDictionary<uint, EmulationKey> KeyboardMap =
        KeyboardFunctions.CreateKeyMap().ToDictionary(pair => pair.Value, pair => pair.Key);

    internal static EmulationInputSnapshot Freeze(EmulationInputSnapshot? snapshot)
    {
        snapshot ??= EmulationInputSnapshot.Empty;
        return new EmulationInputSnapshot(new HashSet<EmulationKey>(snapshot.Keys), snapshot.Pointer with { },
            snapshot.Controllers.ToArray());
    }

    internal static short State(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id)
    {
        if (device == InputConstants.KeyboardDevice)
            return KeyboardMap.TryGetValue(id, out var key) && snapshot.Keys.Contains(key)
                ? InputConstants.ActiveState
                : InputConstants.InactiveState;
        if (device == InputConstants.MouseDevice)
            return MouseState(snapshot.Pointer, port, index, id);
        if (port >= snapshot.Controllers.Count) return InputConstants.InactiveState;
        var controller = snapshot.Controllers[checked((int)port)];
        if (device == InputConstants.JoypadDevice)
        {
            if (index != InputConstants.LeftAnalogIndex) return InputConstants.InactiveState;
            if (id == InputConstants.JoypadMaskId)
                return unchecked((short)(controller.Buttons & ushort.MaxValue));
            return id < InputConstants.MaximumJoypadButtonCount &&
                   ((controller.Buttons & (1u << checked((int)id))) != ExternalCoreInteropConstants.NoInputState ||
                    KeyboardFunctions.IsConsoleKeyActive(snapshot.Keys, id))
                ? InputConstants.ActiveState
                : InputConstants.InactiveState;
        }
        if (device != InputConstants.AnalogDevice) return InputConstants.InactiveState;
        return (index, id) switch
        {
            (InputConstants.LeftAnalogIndex, InputConstants.AnalogXId) => controller.LeftX,
            (InputConstants.LeftAnalogIndex, InputConstants.AnalogYId) => controller.LeftY,
            (InputConstants.RightAnalogIndex, InputConstants.AnalogXId) => controller.RightX,
            (InputConstants.RightAnalogIndex, InputConstants.AnalogYId) => controller.RightY,
            (ControllerConstants.TriggerAnalogIndex, ControllerConstants.LeftTriggerId) => controller.LeftTrigger,
            (ControllerConstants.TriggerAnalogIndex, ControllerConstants.RightTriggerId) => controller.RightTrigger,
            _ => InputConstants.InactiveState
        };
    }

    internal static EmulationInputSnapshot Accumulate(EmulationInputSnapshot current,
        EmulationInputSnapshot update) => new(new HashSet<EmulationKey>(update.Keys), update.Pointer with
    {
        DeltaX = SaturatingAdd(current.Pointer.DeltaX, update.Pointer.DeltaX),
        DeltaY = SaturatingAdd(current.Pointer.DeltaY, update.Pointer.DeltaY),
        Wheel = SaturatingAdd(current.Pointer.Wheel, update.Pointer.Wheel),
        HorizontalWheel = SaturatingAdd(current.Pointer.HorizontalWheel, update.Pointer.HorizontalWheel)
    }, update.Controllers.ToArray());

    internal static EmulationInputSnapshot ConsumeRelativePointer(EmulationInputSnapshot snapshot) =>
        snapshot with
        {
            Pointer = snapshot.Pointer with
            {
                DeltaX = InputConstants.ConsumedRelativeValue,
                DeltaY = InputConstants.ConsumedRelativeValue,
                Wheel = InputConstants.ConsumedRelativeValue,
                HorizontalWheel = InputConstants.ConsumedRelativeValue
            }
        };

    private static short MouseState(EmulationPointerState pointer, uint port, uint index, uint id)
    {
        if (port != InputConstants.PrimaryPort || index != InputConstants.LeftAnalogIndex)
            return InputConstants.InactiveState;
        return id switch
        {
            InputConstants.MouseXId => Clamp(pointer.DeltaX),
            InputConstants.MouseYId => Clamp(pointer.DeltaY),
            InputConstants.MouseLeftId => Boolean(pointer.Left),
            InputConstants.MouseRightId => Boolean(pointer.Right),
            InputConstants.MouseWheelUpId => Boolean(pointer.Wheel > InputConstants.ConsumedRelativeValue),
            InputConstants.MouseWheelDownId => Boolean(pointer.Wheel < InputConstants.ConsumedRelativeValue),
            InputConstants.MouseMiddleId => Boolean(pointer.Middle),
            _ => InputConstants.InactiveState
        };
    }

    private static short Boolean(bool value) => value ? InputConstants.ActiveState : InputConstants.InactiveState;
    private static short Clamp(int value) => (short)Math.Clamp(value, short.MinValue, short.MaxValue);
    private static int SaturatingAdd(int left, int right) =>
        (int)Math.Clamp((long)left + right, int.MinValue, int.MaxValue);
}
