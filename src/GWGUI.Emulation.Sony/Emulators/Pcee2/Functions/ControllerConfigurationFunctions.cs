using System.Globalization;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcee2.Constants;
using Ports = GWGUI.Emulation.Sony.Common.Constants.ControllerPortConstants;
using Controllers = GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.ControllerConstants;

namespace GWGUI.Emulation.Sony.Emulators.Pcee2.Functions;

internal static class ControllerConfigurationFunctions
{
    internal static int GamepadCount(MachineConfiguration configuration) => configuration.Options?.GetValueOrDefault(OptionConstants.MultitapKey) switch
    {
        OptionConstants.MultitapPort1 or OptionConstants.MultitapPort2 => Ports.SingleMultitapControllerCount,
        OptionConstants.MultitapBoth => Ports.MaximumGamepadPortCount,
        _ => Ports.TwoControllerPorts
    };

    internal static string? DisplayName(int port) => port < Controllers.UsbPortCount
        ? Controllers.UsbPortPrefix + (port + 1).ToString(CultureInfo.InvariantCulture)
        : null;

    internal static PeripheralCategory UsbType(MachineConfiguration configuration, int port)
    {
        var binding = configuration.Input?.ControllerBindings?.FirstOrDefault(item => item.Port == port);
        var type = binding?.Type ?? configuration.Controllers?.ElementAtOrDefault(port) ?? PeripheralCategory.Automatic;
        if (type != PeripheralCategory.Automatic) return type;
        var native = configuration.Options?.GetValueOrDefault(OptionConstants.LightgunKey);
        var enabled = native == OptionConstants.LightgunBoth || native == (port == Controllers.FirstUsbPort ? OptionConstants.LightgunUsb1 : OptionConstants.LightgunUsb2);
        return enabled ? PeripheralCategory.GunCon2 : PeripheralCategory.None;
    }

    internal static MachineConfiguration Configure(MachineConfiguration configuration)
    {
        var first = UsbType(configuration, Controllers.FirstUsbPort) == PeripheralCategory.GunCon2;
        var second = UsbType(configuration, Controllers.SecondUsbPort) == PeripheralCategory.GunCon2;
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            [OptionConstants.LightgunKey] = (first, second) switch
            {
                (true, true) => OptionConstants.LightgunBoth,
                (true, false) => OptionConstants.LightgunUsb1,
                (false, true) => OptionConstants.LightgunUsb2,
                _ => OptionConstants.LightgunDisabled
            }
        };
        return configuration with { Options = options };
    }

    internal static IReadOnlyList<CoreControllerPort> CreatePorts(MachineConfiguration configuration) =>
    [
        .. Enumerable.Range(0, Controllers.UsbPortCount).Select(port => new CoreControllerPort(port, (uint)port, ExternalHostCallbacksConstants.LightGunDevice, CanChangeWhileRunning: false)),
        .. Enumerable.Range(0, GamepadCount(configuration)).Select(port => new CoreControllerPort(port + Controllers.UsbPortCount, (uint)port, ExternalHostCallbacksConstants.JoypadDevice))
    ];

    internal static IReadOnlyList<InputBindingDefinition> GunCommands { get; } =
    [
        new(EmulationControllerCommandIds.LeftStickLeft, string.Empty, string.Empty, Controllers.AimLeft) { OppositeDirectionId = EmulationControllerCommandIds.LeftStickRight },
        new(EmulationControllerCommandIds.LeftStickRight, string.Empty, string.Empty, Controllers.AimRight) { OppositeDirectionId = EmulationControllerCommandIds.LeftStickLeft },
        new(EmulationControllerCommandIds.LeftStickUp, string.Empty, string.Empty, Controllers.AimUp) { OppositeDirectionId = EmulationControllerCommandIds.LeftStickDown },
        new(EmulationControllerCommandIds.LeftStickDown, string.Empty, string.Empty, Controllers.AimDown) { OppositeDirectionId = EmulationControllerCommandIds.LeftStickUp },
        new(EmulationControllerCommandIds.B, InputSettingsFunctionsConstants.ResourceControllerActionFire1, string.Empty),
        new(EmulationControllerCommandIds.Reload, InputSettingsFunctionsConstants.ResourceControllerActionReload, string.Empty),
        new(EmulationControllerCommandIds.A, string.Empty, string.Empty, Controllers.ButtonA),
        new(EmulationControllerCommandIds.X, string.Empty, string.Empty, Controllers.ButtonB),
        new(EmulationControllerCommandIds.Y, string.Empty, string.Empty, Controllers.ButtonC),
        new(EmulationControllerCommandIds.Start, string.Empty, string.Empty, InputSettingsFunctionsConstants.ControllerStart),
        new(EmulationControllerCommandIds.Select, string.Empty, string.Empty, InputSettingsFunctionsConstants.ControllerSelect),
        new(EmulationControllerCommandIds.Up, InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
        new(EmulationControllerCommandIds.Down, InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
        new(EmulationControllerCommandIds.Left, InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
        new(EmulationControllerCommandIds.Right, InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty)
    ];
}
