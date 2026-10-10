namespace GWGUI.Emulation.Mattel.Emulators.Common.Interop.Functions;

internal static class ControllerPortFunctions
{
    internal static void Configure(ExternalCoreExports exports, MachineConfiguration configuration)
    {
        for (var port = ControllerConstants.FirstPort;
             port < ConfigurationFunctions.ControllerPortCount(configuration.Model); port++)
            exports.SetControllerPortDevice((uint)port, InputConstants.JoypadDevice);
    }
}
