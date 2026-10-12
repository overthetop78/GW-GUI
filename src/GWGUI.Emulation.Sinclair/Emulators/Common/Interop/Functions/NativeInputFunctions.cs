namespace GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Functions;
internal static class NativeInputFunctions
{
    private static readonly IReadOnlyDictionary<ControllerCommand, uint> Carriers = new Dictionary<ControllerCommand, uint>
    {
        [ControllerCommand.Up] = InputConstants.JoypadUpId,
        [ControllerCommand.Down] = InputConstants.JoypadDownId,
        [ControllerCommand.Left] = InputConstants.JoypadLeftId,
        [ControllerCommand.Right] = InputConstants.JoypadRightId,
        [ControllerCommand.Fire1] = InputConstants.JoypadFireId
    };
    internal static short State(
        EmulationInputSnapshot snapshot,
        IReadOnlySet<uint> keyboard,
        uint port,
        uint device,
        uint index,
        uint id,
        Emulator emulator
    )
    {
        if (index != InputConstants.LeftAnalogIndex)
            return InputConstants.InactiveState;
        var keyboardPort = emulator == Emulator.EightyOne ? InputConstants.EightyOneKeyboardPort : InputConstants.FuseKeyboardPort;
        var baseDevice = device & InputConstants.DeviceMask;
        if (baseDevice == InputConstants.KeyboardDevice)
            return Active(port == keyboardPort && keyboard.Contains(id));
        if (port >= keyboardPort)
            return InputConstants.InactiveState;
        if (baseDevice == InputConstants.MouseDevice)
        {
            if (emulator != Emulator.Fuse)
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

        if (baseDevice != InputConstants.JoypadDevice || port >= snapshot.Controllers.Count)
            return InputConstants.InactiveState;
        var controller = snapshot.Controllers[checked((int)port)];
        uint buttons = 0;
        foreach (var pair in Carriers)
        {
            if ((controller.Buttons & (1u << (int)pair.Key)) != 0)
                buttons |= 1u << checked((int)pair.Value);
        }

        return id == InputConstants.JoypadMaskId ? unchecked((short)buttons) : Active(id < InputConstants.MaximumJoypadButtonCount && (buttons & (1u << checked((int)id))) != 0);
    }

    private static short Active(bool pressed) => pressed ? InputConstants.ActiveState : InputConstants.InactiveState;
}