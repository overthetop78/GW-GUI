using GWGUI.App.Constants.Input.Controllers;
using GWGUI.Emulation;

namespace GWGUI.App.Functions.Input.Controllers;

public static class ControllerInputMap
{
    public static bool IsModernSourcePressed(string source, EmulationControllerState controller) => EmulationInputMappingFunctions.IsControllerSourcePressed(source, controller);

    public static EmulationControllerState ControllerForSource(string source, IReadOnlyList<EmulationControllerState> controllers, EmulationControllerState fallback)
    => GWGUI.Emulation.Functions.EmulationInputMappingFunctions.ResolveSourceController(source, controllers, fallback, fallbackWhenMissing: true);
}
