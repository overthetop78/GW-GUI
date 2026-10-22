using CommonControllerFunctions = GWGUI.Emulation.Acorn.Common.Functions.ControllerFunctions;
using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Constants;
namespace GWGUI.Emulation.Acorn.Emulators.Common.Interop.Functions;

internal static class ControllerPortFunctions
{
    internal static void Configure(ExternalCoreExports exports, ExternalHostCallbacks callbacks, MachineConfiguration configuration)
    {
        for (var port = ControllerPortConstants.MinimumControllerPort;
             port < ConfigurationFunctions.ControllerPortCount(configuration.Model); port++)
        {
            var binding = configuration.Input.Controllers?.FirstOrDefault(item => item.Port == port);
            ConfigurePort(exports, callbacks, configuration, port, binding?.Peripheral ?? PeripheralCategory.Automatic);
        }
        callbacks.ConfigureInput(configuration.Input);
    }

    internal static void ConfigurePort(ExternalCoreExports exports, ExternalHostCallbacks callbacks, MachineConfiguration configuration, int port, PeripheralCategory peripheral)
    {
        if (port < ControllerPortConstants.MinimumControllerPort
            || port >= ConfigurationFunctions.ControllerPortCount(configuration.Model))
            throw new ArgumentOutOfRangeException(nameof(port), ErrorMessages.InvalidControllerPort);
        if (!CommonControllerFunctions.Peripherals(configuration.Model).Contains(peripheral))
            throw new ArgumentException(ErrorMessages.UnsupportedControllerDevice, nameof(peripheral));
        // b2 polls analog axes and joypad buttons directly rather than publishing controller descriptors.
        exports.SetControllerPortDevice(checked((uint)port), peripheral == PeripheralCategory.None ? CoreLifecycleConstants.NoDevice : CoreLifecycleConstants.DefaultJoypadDevice);
    }
}
