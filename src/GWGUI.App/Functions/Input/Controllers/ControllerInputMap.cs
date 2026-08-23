using GWGUI.App.Constants.Input.Controllers;
using GWGUI.Emulation;

namespace GWGUI.App.Functions.Input.Controllers;

public static class ControllerInputMap
{
    public static bool IsModernSourcePressed(string source, EmulationControllerState controller) =>
        EmulationInputMappingFunctions.IsControllerSourcePressed(source, controller);

    public static EmulationControllerState ControllerForSource(string source,
        IReadOnlyList<EmulationControllerState> controllers, EmulationControllerState fallback)
    {
        var deviceId = EmulationInputMappingFunctions.ParseControllerDeviceId(source);
        return deviceId is null ? fallback : controllers.FirstOrDefault(controller =>
            string.Equals(controller.DeviceId, deviceId, StringComparison.OrdinalIgnoreCase)) ?? fallback;
    }
}
