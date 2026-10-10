namespace GWGUI.Emulation.Philips.Emulators.Common.Interop.Functions;

internal static class ControllerPortFunctions
{
    internal static void Configure(ExternalCoreExports exports, MachineConfiguration configuration)
    {
        for (var port = ControllerConstants.FirstPort;
             port < ConfigurationFunctions.ControllerPortCount(configuration.Model); port++)
        {
            var peripheral = configuration.Input.Controllers?.FirstOrDefault(item => item.Port == port)?.Peripheral
                ?? (configuration.Family == MachineFamily.Cdi ? PeripheralCategory.CdiGamepad : PeripheralCategory.Joystick);
            var device = peripheral switch
            {
                PeripheralCategory.None => InputConstants.NoDevice,
                PeripheralCategory.Joystick or PeripheralCategory.CdiGamepad => InputConstants.JoypadDevice,
                PeripheralCategory.Mouse => InputConstants.MouseDevice,
                PeripheralCategory.LightGun => InputConstants.LightgunDevice,
                _ => throw new ArgumentOutOfRangeException(nameof(configuration))
            };
            exports.SetControllerPortDevice((uint)port, device);
        }
    }
}
