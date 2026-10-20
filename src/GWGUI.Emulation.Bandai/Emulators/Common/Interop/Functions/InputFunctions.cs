using GWGUI.Emulation.Bandai.Emulators.Common.Interop.Constants;
namespace GWGUI.Emulation.Bandai.Emulators.Common.Interop.Functions;

internal static class NativeInputFunctions
{
    internal static short State(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id)
    {
        if (device != InputConstants.JoypadDevice || index != InputConstants.LeftAnalogIndex
            || port >= snapshot.Controllers.Count) return InputConstants.InactiveState;
        var buttons = snapshot.Controllers[checked((int)port)].Buttons;
        return EmulationInputMappingFunctions.NativeJoypadState(buttons, id, InputConstants.MaximumJoypadButtonCount);
    }
}
