using GWGUI.Emulation.Philips.Emulators.Common.Interop.Constants;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Philips.Emulators.Common.Interop.Functions;

internal static class NativeInputFunctions
{
    private static readonly IReadOnlyDictionary<uint, IReadOnlySet<EmulationKey>> KeyboardMap =
        KeyboardFunctions.CreateKeyMap().GroupBy(pair => pair.Value).ToDictionary(group => group.Key, group => (IReadOnlySet<EmulationKey>)group.Select(pair => pair.Key).ToHashSet());
    private static readonly IReadOnlyDictionary<uint, IReadOnlySet<EmulationKey>> P2000KeyboardMap =
        KeyboardFunctions.CreateKeyMap(p2000: true).GroupBy(pair => pair.Value).ToDictionary(group => group.Key, group => (IReadOnlySet<EmulationKey>)group.Select(pair => pair.Key).ToHashSet());

    internal static short State(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id, Emulator emulator)
    {
        if (device == InputConstants.KeyboardDevice)
            return (emulator == Emulator.M2000 ? P2000KeyboardMap : KeyboardMap).TryGetValue(id, out var keys) && keys.Any(snapshot.Keys.Contains) ? InputConstants.ActiveState : InputConstants.InactiveState;
        if (device == InputConstants.MouseDevice)
            return EmulationInputMappingFunctions.NativeMouseState(snapshot.Pointer, port, index, id);
        if (port >= snapshot.Controllers.Count) return InputConstants.InactiveState;
        var controller = snapshot.Controllers[checked((int)port)];
        if (device == InputConstants.LightgunDevice && index == InputConstants.LeftAnalogIndex)
            return id switch
            {
                InputConstants.LightgunScreenXId => controller.LeftX,
                InputConstants.LightgunScreenYId => controller.LeftY,
                InputConstants.LightgunTriggerId => Button(controller, InputConstants.NativeTriggerButton),
                InputConstants.LightgunAuxiliaryId => Button(controller, InputConstants.NativeAuxiliaryButton),
                InputConstants.LightgunReloadId or InputConstants.LightgunOffscreenId =>
                    Button(controller, InputConstants.NativeReloadButton),
                _ => InputConstants.InactiveState
            };
        if (device == InputConstants.JoypadDevice)
        {
            if (index != InputConstants.LeftAnalogIndex) return InputConstants.InactiveState;
            return EmulationInputMappingFunctions.NativeJoypadState(controller.Buttons, id, InputConstants.MaximumJoypadButtonCount);
        }
        if (device != InputConstants.AnalogDevice) return InputConstants.InactiveState;
        return EmulationInputMappingFunctions.NativeAnalogState(controller, index, id);
    }


    private static short Boolean(bool value) => value ? InputConstants.ActiveState : InputConstants.InactiveState;
    private static short Button(EmulationControllerState controller, int index) => Boolean((controller.Buttons & (1u << index)) != 0);
}
