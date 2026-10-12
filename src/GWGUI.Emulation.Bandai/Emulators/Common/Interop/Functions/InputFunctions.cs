using GWGUI.Emulation.Bandai.Emulators.Common.Interop.Constants;
namespace GWGUI.Emulation.Bandai.Emulators.Common.Interop.Functions;

internal static class NativeInputFunctions
{
    internal static short State(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id)
    {
        if (device != InputConstants.JoypadDevice || index != InputConstants.LeftAnalogIndex
            || port >= snapshot.Controllers.Count) return InputConstants.InactiveState;
        var buttons = snapshot.Controllers[checked((int)port)].Buttons;
        if (id == InputConstants.JoypadMaskId) return unchecked((short)(buttons & ushort.MaxValue));
        return id < InputConstants.MaximumJoypadButtonCount && (buttons & (1u << checked((int)id))) != 0 ? InputConstants.ActiveState : InputConstants.InactiveState;
    }
}
