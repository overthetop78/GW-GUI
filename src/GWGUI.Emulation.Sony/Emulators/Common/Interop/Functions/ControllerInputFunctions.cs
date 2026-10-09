using GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums;
using static GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants.ExternalHostCallbacksConstants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class ControllerInputFunctions
{
    internal static bool PortButton(EmulationControllerState controller, string command,
        bool fallback = false) => controller.Controls.TryGetValue(command, out var pressure)
        ? pressure > PressedPressureThreshold
        : (InputSnapshotDictionary.ButtonIndexes.TryGetValue(command, out var index)
            && (controller.Buttons & (1u << index)) != EmptyButtonMask) || fallback;

    internal static short AnalogButton(EmulationControllerState controller, JoypadInput button)
    {
        if (!Enum.IsDefined(button)) return default;
        if (controller.Controls.TryGetValue(button.ToString(), out var pressure))
            return (short)Math.Round(Math.Clamp(pressure, ReleasedPressure, FullPressure) * short.MaxValue);
        var trigger = button switch
        {
            JoypadInput.L2 => controller.LeftTrigger,
            JoypadInput.R2 => controller.RightTrigger,
            _ => default(short)
        };
        if (trigger > ReleasedPressure) return trigger;
        return PortButton(controller, button.ToString()) ? short.MaxValue : default;
    }

}
