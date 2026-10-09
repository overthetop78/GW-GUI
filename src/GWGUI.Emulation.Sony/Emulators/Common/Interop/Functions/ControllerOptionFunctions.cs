using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class ControllerOptionFunctions
{
    internal static IReadOnlyList<string>? Types(CoreOption option)
    {
        if (option.Tab != EmulationMachineTab.Controllers) return null;
        if (option.Key.StartsWith("pcsx2_", StringComparison.Ordinal)
            || option.Key.StartsWith("ppsspp_", StringComparison.Ordinal))
            return [ControllerType.Joystick.ToString(), ControllerType.DualShock.ToString()];
        if (Contains(ControllerOptionConstants.Multitap)) return null;
        ControllerType[] types = ControllerOptionConstants.IsAnalogCalibration(option.Key)
            ? [ControllerType.DualShock, ControllerType.AnalogController, ControllerType.AnalogJoystick,
                ControllerType.NeGcon, ControllerType.NeGconRumble]
            : Contains(ControllerOptionConstants.NeGcon)
            || option.Key.EndsWith(ControllerOptionConstants.SteeringDeadzoneSuffix, StringComparison.Ordinal)
            || option.Key.EndsWith(ControllerOptionConstants.TwistResponseSuffix, StringComparison.Ordinal)
                ? [ControllerType.NeGcon, ControllerType.NeGconRumble]
            : Contains(ControllerOptionConstants.KonamiGun) ? [ControllerType.Justifier]
            : Contains(ControllerOptionConstants.GunCon)
                || option.Key.EndsWith(ControllerOptionConstants.XScaleSuffix, StringComparison.Ordinal)
                || option.Key.EndsWith(ControllerOptionConstants.YScaleSuffix, StringComparison.Ordinal)
                ? [ControllerType.GunCon]
            : Contains(ControllerOptionConstants.Gun) || Contains(ControllerOptionConstants.Crosshair)
                ? [ControllerType.GunCon, ControllerType.Justifier]
            : Contains(ControllerOptionConstants.Mouse) ? [ControllerType.Mouse]
            : Contains(ControllerOptionConstants.Rumble) || Contains(ControllerOptionConstants.Vibration)
                ? [ControllerType.DualShock, ControllerType.NeGconRumble]
            : Contains(ControllerOptionConstants.Analog) || Contains(ControllerOptionConstants.Axis)
                || Contains(ControllerOptionConstants.Deadzone)
                ? [ControllerType.DualShock, ControllerType.AnalogController, ControllerType.AnalogJoystick]
            : Enum.GetValues<ControllerType>().Where(type => type != ControllerType.None).ToArray();
        return types.Select(type => type.ToString()).ToArray();

        bool Contains(string value) => option.Key.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    internal static int? PortNumber(CoreOption option)
    {
        var key = option.Key;
        if (option.Tab == EmulationMachineTab.Controllers
            && key.StartsWith("pcsx2_", StringComparison.Ordinal) && key[^1] is '1' or '2')
            return key[^1] - '0';
        if (key.StartsWith(ControllerOptionConstants.SwanPortPrefix, StringComparison.Ordinal)
            && key.Length > ControllerOptionConstants.SwanPortPrefix.Length
            && char.IsAsciiDigit(key[ControllerOptionConstants.SwanPortPrefix.Length]))
            return key[ControllerOptionConstants.SwanPortPrefix.Length] - '0';
        if (key.Contains(ControllerOptionConstants.PlayerOne, StringComparison.OrdinalIgnoreCase)) return 1;
        if (key.Contains(ControllerOptionConstants.PlayerTwo, StringComparison.OrdinalIgnoreCase)) return 2;
        if (key.EndsWith(ControllerOptionConstants.PortOneSuffix, StringComparison.OrdinalIgnoreCase)) return 1;
        if (key.EndsWith(ControllerOptionConstants.PortTwoSuffix, StringComparison.OrdinalIgnoreCase)) return 2;
        return null;
    }
}
