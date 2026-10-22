using GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums;
using CommonControllerFunctions = GWGUI.Emulation.Sony.Common.Functions.ControllerFunctions;
using static GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants.ExternalHostCallbacksConstants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class ControllerInputFunctions
{
    internal static bool LightGunButton(EmulationControllerState controller, LightGunInput input) => input switch
    {
        LightGunInput.Trigger => PortButton(controller, EmulationControllerCommandIds.B),
        LightGunInput.AuxiliaryA => PortButton(controller, EmulationControllerCommandIds.A),
        LightGunInput.AuxiliaryB => PortButton(controller, EmulationControllerCommandIds.X),
        LightGunInput.AuxiliaryC => PortButton(controller, EmulationControllerCommandIds.Y),
        LightGunInput.Start or LightGunInput.Pause => PortButton(controller, EmulationControllerCommandIds.Start),
        LightGunInput.Select => PortButton(controller, EmulationControllerCommandIds.Select),
        LightGunInput.Up => PortButton(controller, EmulationControllerCommandIds.Up),
        LightGunInput.Down => PortButton(controller, EmulationControllerCommandIds.Down),
        LightGunInput.Left => PortButton(controller, EmulationControllerCommandIds.Left),
        LightGunInput.Right => PortButton(controller, EmulationControllerCommandIds.Right),
        LightGunInput.Reload => PortButton(controller, EmulationControllerCommandIds.Reload),
        _ => false
    };

    internal static bool PortButton(EmulationControllerState controller,
        string command,
        bool fallback = false) => controller.Controls.TryGetValue(command,
        out var pressure) ? pressure > PressedPressureThreshold : (InputSnapshotDictionary.ButtonIndexes.TryGetValue(command,
        out var index) && (controller.Buttons & (1u << index)) != EmptyButtonMask) || fallback;

    /// <summary>Lit la pression par le Common, après contrôle de l’identité de bouton du protocole Sony.</summary>
    internal static short AnalogButton(EmulationControllerState controller, JoypadInput button) => Enum.IsDefined(button)
        ? CommonControllerFunctions.NativeAnalogState(controller, (uint)AnalogInputIndex.Button, (uint)button) : (short)0;

}
