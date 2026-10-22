using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using NativeControllerConstants = GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants.ControllerPortConstants;
using SemanticPortConstants = GWGUI.Emulation.Sony.Common.Constants.ControllerPortConstants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class ControllerPortFunctions
{
    internal static uint Resolve(PeripheralCategory type, int port, IReadOnlyList<IReadOnlyList<ControllerDevice>> ports)
    {
        if (port < SemanticPortConstants.MinimumControllerPort
            || port >= SemanticPortConstants.MaximumGamepadPortCount)
            throw new ArgumentOutOfRangeException(nameof(port), port, null);
        if (type == PeripheralCategory.None) return NativeControllerConstants.NoneDevice;
        if (type is PeripheralCategory.Joystick or PeripheralCategory.DanceMat or PeripheralCategory.ArcadeStick)
        {
            if (!ports.Any()) return NativeControllerConstants.JoypadDevice;
            if (port >= ports.Count)
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
            if (ports[port].Any(device => device.Id == NativeControllerConstants.JoypadDevice))
                return NativeControllerConstants.JoypadDevice;
            var joypads = ports[port].Where(device => (device.Id & NativeControllerConstants.DeviceBaseMask) == NativeControllerConstants.JoypadDevice).ToArray();
            if (joypads.Length == SemanticPortConstants.OneControllerPort) return joypads.Single().Id;
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        if (port < ports.Count && NativeControllerConstants.NativeNames.TryGetValue(type, out var names))
        {
            var device = ports[port].FirstOrDefault(device => names.Contains(device.Name, StringComparer.OrdinalIgnoreCase));
            if (device is not null) return device.Id;
        }
        throw new ArgumentOutOfRangeException(nameof(type), type, null);
    }
}
