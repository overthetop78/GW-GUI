using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Constants;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Epoch.Emulators.Common.Interop.Functions;

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
        if (device == InputConstants.LightGunDevice)
            return GunState(controller, port, index, id);
        if (device == InputConstants.JoypadDevice)
        {
            if (index != InputConstants.LeftAnalogIndex) return InputConstants.InactiveState;
            return EmulationInputMappingFunctions.NativeJoypadState(controller.Buttons, id, InputConstants.MaximumJoypadButtonCount);
        }
        if (device != InputConstants.AnalogDevice) return InputConstants.InactiveState;
        return EmulationInputMappingFunctions.NativeAnalogState(controller, index, id);
    }


    private static short Boolean(bool value) => value ? InputConstants.ActiveState : InputConstants.InactiveState;
    private static short GunState(EmulationControllerState controller, uint port, uint index, uint id)
    {
        if (port != InputConstants.PrimaryPort || index != InputConstants.LeftAnalogIndex)
            return InputConstants.InactiveState;
        var enabled = (controller.Buttons & InputConstants.GunEnabledMask) != 0;
        return id switch
        {
            InputConstants.LightGunOffscreen => Boolean(!enabled),
            InputConstants.LightGunScreenX => enabled ? controller.RightX : InputConstants.InactiveState,
            InputConstants.LightGunScreenY => enabled ? controller.RightY : InputConstants.InactiveState,
            InputConstants.LightGunTrigger => Boolean(enabled && (controller.Buttons & InputConstants.GunTriggerMask) != 0),
            InputConstants.LightGunReload => Boolean(enabled && (controller.Buttons & InputConstants.GunReloadMask) != 0),
            _ => InputConstants.InactiveState
        };
    }
}
