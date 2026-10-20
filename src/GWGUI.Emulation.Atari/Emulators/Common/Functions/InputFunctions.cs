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
            return EmulationInputMappingFunctions.NativeMouseState(snapshot.Pointer, port, index, id);
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
            (ControllerConstants.TriggerAnalogIndex, ControllerConstants.LeftTriggerId) => controller.LeftTrigger,
            (ControllerConstants.TriggerAnalogIndex, ControllerConstants.RightTriggerId) => controller.RightTrigger,
            _ => EmulationInputMappingFunctions.NativeAnalogState(controller, index, id)
        };
    }


}
