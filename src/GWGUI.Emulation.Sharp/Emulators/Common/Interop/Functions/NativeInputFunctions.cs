namespace GWGUI.Emulation.Sharp.Emulators.Common.Interop.Functions;
internal static class NativeInputFunctions
{
    // Protocol carriers only: physical controls are resolved by the explicit user mappings.
    private enum Joypad : uint
    {
        B = 0,
        Y = 1,
        Select = 2,
        Start = 3,
        Up = 4,
        Down = 5,
        Left = 6,
        Right = 7,
        A = 8,
        X = 9,
        L = 10,
        R = 11
    }

    private static readonly IReadOnlyDictionary<ControllerCommand, Joypad> Carriers = new Dictionary<ControllerCommand, Joypad>
    {
        [ControllerCommand.Up] = Joypad.Up,
        [ControllerCommand.Down] = Joypad.Down,
        [ControllerCommand.Left] = Joypad.Left,
        [ControllerCommand.Right] = Joypad.Right,
        [ControllerCommand.Fire1] = Joypad.A,
        [ControllerCommand.Fire2] = Joypad.B,
        [ControllerCommand.Fire3] = Joypad.X,
        [ControllerCommand.Fire4] = Joypad.Y,
        [ControllerCommand.Fire5] = Joypad.L,
        [ControllerCommand.Fire6] = Joypad.R,
        [ControllerCommand.Fire7] = Joypad.Start,
        [ControllerCommand.Fire8] = Joypad.Select
    };
    internal static short State(EmulationInputSnapshot snapshot, IReadOnlySet<uint> keyboard, uint port, uint device, uint index, uint id, Emulator emulator)
    {
        if (device == InputConstants.KeyboardDevice)
            return port == InputConstants.PrimaryPort && index == InputConstants.LeftAnalogIndex && keyboard.Contains(id) ? InputConstants.ActiveState : InputConstants.InactiveState;
        if (device == InputConstants.MouseDevice)
        {
            if (emulator != Emulator.Px68k || port != InputConstants.PrimaryPort || index != InputConstants.LeftAnalogIndex)
                return InputConstants.InactiveState;
            return id switch
            {
                InputConstants.MouseXId => (short)Math.Clamp(snapshot.Pointer.DeltaX, short.MinValue, short.MaxValue),
                InputConstants.MouseYId => (short)Math.Clamp(snapshot.Pointer.DeltaY, short.MinValue, short.MaxValue),
                InputConstants.MouseLeftId => Active(snapshot.Pointer.Left),
                InputConstants.MouseRightId => Active(snapshot.Pointer.Right),
                _ => InputConstants.InactiveState
            };
        }

        if (port >= snapshot.Controllers.Count || emulator == Emulator.X1 && port != InputConstants.PrimaryPort)
            return InputConstants.InactiveState;
        var controller = snapshot.Controllers[checked((int)port)];
        if (device == InputConstants.AnalogDevice)
        {
            if (emulator != Emulator.Px68k)
                return InputConstants.InactiveState;
            return (index, id) switch
            {
                (InputConstants.LeftAnalogIndex, InputConstants.AnalogXId) => controller.LeftX,
                (InputConstants.LeftAnalogIndex, InputConstants.AnalogYId) => controller.LeftY,
                (InputConstants.RightAnalogIndex, InputConstants.AnalogYId) => controller.RightY,
                _ => InputConstants.InactiveState
            };
        }

        if (device != InputConstants.JoypadDevice || index != InputConstants.LeftAnalogIndex)
            return InputConstants.InactiveState;
        uint buttons = 0;
        foreach (var pair in Carriers)
        {
            if (emulator == Emulator.X1 && pair.Key > ControllerCommand.Fire4)
                continue;
            if ((controller.Buttons & (1u << (int)pair.Key)) != 0)
                buttons |= 1u << (int)pair.Value;
        }

        return id == InputConstants.JoypadMaskId ? unchecked((short)buttons) : Active(id < InputConstants.MaximumJoypadButtonCount && (buttons & (1u << (int)id)) != 0);
    }

    private static short Active(bool pressed) => pressed ? InputConstants.ActiveState : InputConstants.InactiveState;
}