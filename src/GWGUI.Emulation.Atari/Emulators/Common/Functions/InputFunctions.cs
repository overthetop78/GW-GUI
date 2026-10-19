using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Emulators.Common.Functions;

internal static class InputFunctions
{
    private static readonly IReadOnlyDictionary<uint, EmulationKey> KeyboardMap = KeyboardFunctions.CreateKeyMap().ToDictionary(pair => pair.Value, pair => pair.Key);

    internal static short State(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id)
    {
        if (device == InputConstants.KeyboardDevice)
            return KeyboardMap.TryGetValue(id, out var key) && snapshot.Keys.Contains(key) ? InputConstants.ActiveState : InputConstants.InactiveState;
        if (device == InputConstants.MouseDevice)
            return MouseState(snapshot.Pointer, port, index, id);
        if (port >= snapshot.Controllers.Count) return InputConstants.InactiveState;
        var controller = snapshot.Controllers[checked((int)port)];
        if (device == InputConstants.JoypadDevice)
        {
            if (index != InputConstants.LeftAnalogIndex) return InputConstants.InactiveState;
            if (id == InputConstants.JoypadMaskId)
                return unchecked((short)(controller.Buttons & ushort.MaxValue));
            return id < InputConstants.MaximumJoypadButtonCount && ((controller.Buttons & (1u << checked((int)id))) != ExternalCoreInteropConstants.NoInputState || KeyboardFunctions.IsConsoleKeyActive(snapshot.Keys, id)) ? InputConstants.ActiveState : InputConstants.InactiveState;
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
}
