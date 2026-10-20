using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Constants;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions;

internal static class NativeInputFunctions
{
    private static readonly IReadOnlyDictionary<uint, EmulationKey> KeyboardMap = KeyboardFunctions.CreateKeyMap().ToDictionary(pair => pair.Value, pair => pair.Key);

    internal static short State(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id)
    {
        if (device == InputConstants.KeyboardDevice)
            return KeyboardMap.TryGetValue(id, out var key) && snapshot.Keys.Contains(key) ? InputConstants.ActiveState : InputConstants.InactiveState;
        if (device == InputConstants.MouseDevice)
            return EmulationInputMappingFunctions.NativeMouseState(snapshot.Pointer, port, index, id);
        if (port >= snapshot.Controllers.Count) return InputConstants.InactiveState;
        var controller = snapshot.Controllers[checked((int)port)];
        if (device == InputConstants.JoypadDevice)
        {
            if (index != InputConstants.LeftAnalogIndex) return InputConstants.InactiveState;
            return EmulationInputMappingFunctions.NativeJoypadState(controller.Buttons, id, InputConstants.MaximumJoypadButtonCount);
        }
        if (device != InputConstants.AnalogDevice) return InputConstants.InactiveState;
        return EmulationInputMappingFunctions.NativeAnalogState(controller, index, id);
    }


}
