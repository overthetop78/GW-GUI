using GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Constants;
namespace GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Functions;

internal static class NativeInputFunctions
{
    internal static short State(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id)
    {
        if (device != InputConstants.JoypadDevice || index != InputConstants.PrimaryIndex || port >= snapshot.Controllers.Count)
            return InputConstants.InactiveState;
        var controller = snapshot.Controllers[checked((int)port)];
        if (id == InputConstants.JoypadMaskId) return unchecked((short)(controller.Buttons & ushort.MaxValue));
        return id < InputConstants.MaximumJoypadButtonCount && (controller.Buttons & (1u << checked((int)id))) != 0 ? InputConstants.ActiveState : InputConstants.InactiveState;
    }
}
