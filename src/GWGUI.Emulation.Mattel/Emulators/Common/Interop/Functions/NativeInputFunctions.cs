namespace GWGUI.Emulation.Mattel.Emulators.Common.Interop.Functions;

internal static class NativeInputFunctions
{
    // Libretro carrier controls, not default associations with host controls.
    private enum Joypad { B = 0, Y = 1, A = 8, LeftThumb = 14, RightThumb = 15, LeftTrigger = 12, RightTrigger = 13 }

    private static readonly IReadOnlyDictionary<ControllerCommand, (short X, short Y)> KeypadAxes =
        new Dictionary<ControllerCommand, (short, short)>
        {
            [ControllerCommand.Key1] = (-24576, -24576),
            [ControllerCommand.Key2] = (-16384, -24576),
            [ControllerCommand.Key3] = (8192, -24576),
            [ControllerCommand.Key4] = (-24576, 0),
            [ControllerCommand.Key6] = (8192, 0),
            [ControllerCommand.Key7] = (-24576, 8192),
            [ControllerCommand.Key8] = (-16384, 24576),
            [ControllerCommand.Key9] = (8192, 8192)
        };

    internal static short State(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id,
        Emulator emulator)
    {
        if (emulator != Emulator.FreeIntv || port >= snapshot.Controllers.Count) return InputConstants.InactiveState;
        var controller = snapshot.Controllers[checked((int)port)];
        if (device == InputConstants.AnalogDevice)
        {
            if (index == InputConstants.LeftAnalogIndex)
                return id switch { InputConstants.AnalogXId => controller.LeftX,
                    InputConstants.AnalogYId => controller.LeftY, _ => InputConstants.InactiveState };
            if (index != InputConstants.RightAnalogIndex) return InputConstants.InactiveState;
            foreach (var pair in KeypadAxes)
                if (Pressed(controller, pair.Key))
                    return id switch { InputConstants.AnalogXId => pair.Value.X,
                        InputConstants.AnalogYId => pair.Value.Y, _ => InputConstants.InactiveState };
            return InputConstants.InactiveState;
        }
        if (device != InputConstants.JoypadDevice || index != InputConstants.LeftAnalogIndex)
            return InputConstants.InactiveState;
        uint buttons = 0;
        Carry(ControllerCommand.Top, Joypad.Y);
        Carry(ControllerCommand.BottomLeft, Joypad.A);
        Carry(ControllerCommand.BottomRight, Joypad.B);
        Carry(ControllerCommand.Key0, Joypad.LeftThumb);
        Carry(ControllerCommand.Key5, Joypad.RightThumb);
        Carry(ControllerCommand.Clear, Joypad.LeftTrigger);
        Carry(ControllerCommand.Enter, Joypad.RightTrigger);
        if (id == InputConstants.JoypadMaskId) return unchecked((short)buttons);
        return id < InputConstants.MaximumJoypadButtonCount && (buttons & (1u << (int)id)) != 0
            ? InputConstants.ActiveState : InputConstants.InactiveState;

        void Carry(ControllerCommand command, Joypad carrier)
        {
            if (Pressed(controller, command)) buttons |= 1u << (int)carrier;
        }
    }

    private static bool Pressed(EmulationControllerState controller, ControllerCommand command) =>
        (controller.Buttons & (1u << (int)command)) != 0;
}
