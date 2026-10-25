using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class ControllerOptionFunctions
{
    internal static IReadOnlyList<string>? Types(CoreOption option)
    {
        if (option.Tab != EmulationMachineTab.Controllers) return null;
        if (option.Key.StartsWith("pcsx2_", StringComparison.Ordinal)
            || option.Key.StartsWith("ppsspp_", StringComparison.Ordinal))
            return [PeripheralCategory.Joystick.ToString(), PeripheralCategory.DualShock.ToString()];
        if (Contains(ControllerOptionConstants.Multitap)) return null;
        PeripheralCategory[] types = ControllerOptionConstants.IsAnalogCalibration(option.Key)
            ? [PeripheralCategory.DualShock, PeripheralCategory.AnalogController, PeripheralCategory.AnalogJoystick,
                PeripheralCategory.NeGcon, PeripheralCategory.NeGconRumble]
            : Contains(ControllerOptionConstants.NeGcon)
            || option.Key.EndsWith(ControllerOptionConstants.SteeringDeadzoneSuffix, StringComparison.Ordinal)
            || option.Key.EndsWith(ControllerOptionConstants.TwistResponseSuffix, StringComparison.Ordinal)
                ? [PeripheralCategory.NeGcon, PeripheralCategory.NeGconRumble]
            : Contains(ControllerOptionConstants.KonamiGun) ? [PeripheralCategory.Justifier]
            : Contains(ControllerOptionConstants.GunCon)
                || option.Key.EndsWith(ControllerOptionConstants.XScaleSuffix, StringComparison.Ordinal)
                || option.Key.EndsWith(ControllerOptionConstants.YScaleSuffix, StringComparison.Ordinal)
                ? [PeripheralCategory.GunCon]
            : Contains(ControllerOptionConstants.Gun) || Contains(ControllerOptionConstants.Crosshair)
                ? [PeripheralCategory.GunCon, PeripheralCategory.Justifier]
            : Contains(ControllerOptionConstants.Mouse) ? [PeripheralCategory.Mouse]
            : Contains(ControllerOptionConstants.Rumble) || Contains(ControllerOptionConstants.Vibration)
                ? [PeripheralCategory.DualShock, PeripheralCategory.NeGconRumble]
            : Contains(ControllerOptionConstants.Analog) || Contains(ControllerOptionConstants.Axis)
                || Contains(ControllerOptionConstants.Deadzone)
                ? [PeripheralCategory.DualShock, PeripheralCategory.AnalogController, PeripheralCategory.AnalogJoystick]
            : Enum.GetValues<PeripheralCategory>().Where(type => type != PeripheralCategory.None).ToArray();
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
