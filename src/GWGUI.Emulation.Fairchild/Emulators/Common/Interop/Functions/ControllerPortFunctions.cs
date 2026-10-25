using GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Contracts;
namespace GWGUI.Emulation.Fairchild.Emulators.Common.Interop.Functions;

internal static class ControllerPortFunctions
{
    internal static void Configure(ExternalCoreExports exports)
    {
        for (var port = ControllerConstants.FirstPort; port < ControllerConstants.PortCount; port++)
            exports.SetControllerPortDevice((uint)port, CoreLifecycleConstants.DefaultJoypadDevice);
    }
}
