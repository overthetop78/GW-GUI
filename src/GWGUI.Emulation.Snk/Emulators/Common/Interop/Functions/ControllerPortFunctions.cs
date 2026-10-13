using GWGUI.Emulation.Snk.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Snk.Emulators.Common.Dictionaries;
namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;

internal static class ControllerPortFunctions
{
    internal static void Configure(ExternalCoreExports exports, MachineConfiguration configuration)
    {
        var factory = (LibretroMachineFactory)CoreCatalog.CreateAdapter(configuration.Core);
        var count = factory.ControllerPortCount(configuration);
        for (var port = ControllerConstants.FirstPort; port < count; port++)
        {
            var peripheral = configuration.Input.Controllers?.FirstOrDefault(controller => controller.Port == port)?.Peripheral ?? InputSettingsFunctions.DefaultPeripheral(configuration.Model);
            exports.SetControllerPortDevice(checked((uint)port), factory.ControllerDeviceId(configuration, port, peripheral));
        }
    }
}
