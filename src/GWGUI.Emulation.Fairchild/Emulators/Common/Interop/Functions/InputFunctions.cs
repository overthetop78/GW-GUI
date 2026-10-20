using GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Constants;
namespace GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Functions;

internal static class NativeInputFunctions
{
    internal static short State(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id)
    {
        if (device != InputConstants.JoypadDevice || index != InputConstants.PrimaryIndex || port >= snapshot.Controllers.Count)
            return InputConstants.InactiveState;
        var controller = snapshot.Controllers[checked((int)port)];
        return EmulationInputMappingFunctions.NativeJoypadState(controller.Buttons, id, InputConstants.MaximumJoypadButtonCount);
    }
}
