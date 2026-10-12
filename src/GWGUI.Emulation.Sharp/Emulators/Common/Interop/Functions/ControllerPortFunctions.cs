using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Contracts;
namespace GWGUI.Emulation.Sharp.Emulators.Common.Interop.Functions;

internal static class ControllerPortFunctions
{
    internal static void Configure(ExternalCoreExports exports, MachineConfiguration configuration)
    {
        for (var port = ControllerConstants.FirstPort; port < ConfigurationFunctions.ControllerPortCount(configuration.Model); port++) exports.SetControllerPortDevice(checked((uint)port), InputConstants.JoypadDevice);
    }
}
