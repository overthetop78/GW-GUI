namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;

internal static class NativeInputFunctions
{
    internal static short State(EmulationInputSnapshot snapshot, IReadOnlyDictionary<ControllerCommand, uint> buttonIds, ulong capabilities, uint port, uint device, uint index, uint id)
    {
        var baseDevice = device & InputConstants.DeviceMask;
        if (baseDevice >= sizeof(ulong) * 8 || (capabilities & (1UL << checked((int)baseDevice))) == 0)
            return InputConstants.InactiveState;
        if (baseDevice == InputConstants.MouseDevice)
        {
            if (port != InputConstants.PrimaryPort || index != InputConstants.LeftAnalogIndex)
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
    
        if (port >= snapshot.Controllers.Count)
            return InputConstants.InactiveState;
        var controller = snapshot.Controllers[checked((int)port)];
        if (baseDevice == InputConstants.AnalogDevice)
            return (index, id) switch
            {
                (InputConstants.LeftAnalogIndex, InputConstants.AnalogXId) => controller.LeftX,
                (InputConstants.LeftAnalogIndex, InputConstants.AnalogYId) => controller.LeftY,
                (InputConstants.RightAnalogIndex, InputConstants.AnalogXId) => controller.RightX,
                (InputConstants.RightAnalogIndex, InputConstants.AnalogYId) => controller.RightY,
                _ => InputConstants.InactiveState
            };
        if (baseDevice != InputConstants.JoypadDevice || index != InputConstants.LeftAnalogIndex)
            return InputConstants.InactiveState;
        uint buttons = 0;
        foreach (var pair in buttonIds)
        {
            if (pair.Value < InputConstants.MaximumJoypadButtonCount && (controller.Buttons & (1u << (int)pair.Key)) != 0)
                buttons |= 1u << checked((int)pair.Value);
        }
    
        return id == InputConstants.JoypadMaskId ? unchecked((short)buttons) : Active(id < InputConstants.MaximumJoypadButtonCount && (buttons & (1u << checked((int)id))) != 0);
    }

    private static short Active(bool pressed) => pressed ? InputConstants.ActiveState : InputConstants.InactiveState;
}
