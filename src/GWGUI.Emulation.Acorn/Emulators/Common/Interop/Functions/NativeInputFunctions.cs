using CommonControllerFunctions = GWGUI.Emulation.Acorn.Common.Functions.ControllerFunctions;
using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Constants;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Acorn.Emulators.Common.Interop.Functions;

internal static class NativeInputFunctions
{
    private static readonly IReadOnlyDictionary<uint, IReadOnlySet<EmulationKey>> KeyboardMap =
        KeyboardFunctions.CreateKeyMap().GroupBy(pair => pair.Value).ToDictionary(group => group.Key, group => (IReadOnlySet<EmulationKey>)group.Select(pair => pair.Key).ToHashSet());

    internal static short State(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id)
    {
        if (device == InputConstants.KeyboardDevice)
            return KeyboardMap.TryGetValue(id, out var keys) && keys.Any(snapshot.Keys.Contains) ? InputConstants.ActiveState : InputConstants.InactiveState;
        if (device == InputConstants.MouseDevice)
            return CommonControllerFunctions.NativeMouseState(snapshot.Pointer, port, index, id);
        if (port >= snapshot.Controllers.Count) return InputConstants.InactiveState;
        var controller = snapshot.Controllers[checked((int)port)];
        if (device == InputConstants.JoypadDevice)
        {
            if (index != InputConstants.LeftAnalogIndex) return InputConstants.InactiveState;
            return CommonControllerFunctions.NativeJoypadState(controller.Buttons, id, InputConstants.MaximumJoypadButtonCount);
        }
        if (device != InputConstants.AnalogDevice) return InputConstants.InactiveState;
        return (index, id) switch
        {
            (InputConstants.TriggerAnalogIndex, InputConstants.LeftTriggerId) => controller.LeftTrigger,
            (InputConstants.TriggerAnalogIndex, InputConstants.RightTriggerId) => controller.RightTrigger,
            _ => CommonControllerFunctions.NativeAnalogState(controller, index, id)
        };
    }



}

