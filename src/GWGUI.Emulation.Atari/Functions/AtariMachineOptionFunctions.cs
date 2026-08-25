using System.Globalization;

namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariMachineOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Apply(AtariMachineConfiguration configuration)
    {
        var showDriveActivity = configuration.Options.GetValueOrDefault(
            AtariMachineOptionConstants.DriveActivity, AtariMachineOptionFunctionsConstants.False);
        var result = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal)
        {
            [AtariMachineOptionConstants.MachineType] = MachineTypeFor(configuration.Model),
            [AtariMachineOptionConstants.DisableMouse] = AtariMachineOptionFunctionsConstants.False,
            // Hatari calls its joypad-driven pointer mode "mouse mode". GW GUI supplies a real
            // relative mouse through RETRO_DEVICE_MOUSE, which Hatari reads in the opposite mode.
            [AtariMachineOptionConstants.StartInMouseMode] = AtariMachineOptionFunctionsConstants.False,
            [AtariMachineOptionConstants.DisableKeyboard] = AtariMachineOptionFunctionsConstants.False,
            [AtariMachineOptionConstants.TwoJoysticks] = SecondJoystick(configuration) ? AtariMachineOptionFunctionsConstants.True : AtariMachineOptionFunctionsConstants.False,
            [AtariMachineOptionConstants.DriveActivity] = showDriveActivity,
            // Older Hatari builds only paint drive LEDs inside their status line. Newer builds
            // also draw the compact OSD bars, so setting both keeps this option effective.
            [AtariMachineOptionConstants.InputStatusDisplay] = string.Equals(showDriveActivity, AtariMachineOptionFunctionsConstants.True,
                StringComparison.OrdinalIgnoreCase) ? AtariMachineOptionFunctionsConstants.Value1 : AtariMachineOptionFunctionsConstants.Value0,
            [AtariMachineOptionConstants.AutoloadConfiguration] = AtariMachineOptionFunctionsConstants.False
        };
        Copy(result, AtariMachineOptionConstants.MainMemory, AtariMachineOptionConstants.RamSize, RamValue);
        Copy(result, AtariMachineOptionConstants.Frequency, AtariMachineOptionConstants.CpuFrequency, CpuFrequencyValue);
        Copy(result, AtariConfigurationOptionConstants.VideoStandard, AtariMachineOptionConstants.HighResolution,
            value => string.Equals(value, AtariMachineOptionFunctionsConstants.Monochrome, StringComparison.OrdinalIgnoreCase) ? AtariMachineOptionFunctionsConstants.True : AtariMachineOptionFunctionsConstants.False);
        Copy(result, AtariConfigurationOptionConstants.VideoStandard, AtariMachineOptionConstants.RefreshRate, RefreshRateValue);
        Copy(result, AtariMachineOptionConstants.Crop, AtariMachineOptionConstants.CropOverscan,
            value => string.Equals(value, AtariMachineOptionFunctionsConstants.Enabled, StringComparison.OrdinalIgnoreCase) ? AtariMachineOptionFunctionsConstants.True : AtariMachineOptionFunctionsConstants.False);
        Copy(result, AtariMachineOptionConstants.Frames, AtariMachineOptionConstants.FrameSkip, value => value);
        Copy(result, AtariMachineOptionConstants.PointerSpeed, AtariMachineOptionConstants.MouseSpeed, MouseSpeedValue);
        ApplyFloppySettings(configuration, result);
        return result;
    }

    private static void Copy(IDictionary<string, string> values, string source, string target,
        Func<string, string?> convert)
    {
        if (values.TryGetValue(source, out var value) && convert(value) is { } converted)
            values[target] = converted;
    }

    private static string MachineTypeFor(AtariMachineModel model) => model switch
    {
        AtariMachineModel.St or AtariMachineModel.Stf or AtariMachineModel.Stfm or AtariMachineModel.MegaSt => AtariMachineOptionFunctionsConstants.St,
        AtariMachineModel.Ste or AtariMachineModel.MegaSte => AtariMachineOptionFunctionsConstants.Ste,
        AtariMachineModel.Tt => AtariMachineOptionFunctionsConstants.Tt,
        AtariMachineModel.Falcon => AtariMachineOptionFunctionsConstants.Falcon,
        _ => throw new ArgumentOutOfRangeException(nameof(model), model, AtariErrorMessages.UnknownStModel)
    };

    private static string? RamValue(string value)
    {
        if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var bytes)) return null;
        return bytes switch
        {
            524288 => AtariMachineOptionFunctionsConstants.Value0,
            1048576 => AtariMachineOptionFunctionsConstants.Value1,
            2097152 => AtariMachineOptionFunctionsConstants.Value2,
            4194304 => AtariMachineOptionFunctionsConstants.Value4,
            8388608 => AtariMachineOptionFunctionsConstants.Value8,
            14680064 => AtariMachineOptionFunctionsConstants.Value14,
            _ => null
        };
    }

    private static string? CpuFrequencyValue(string value) => value is AtariMachineOptionFunctionsConstants.Value8 or AtariMachineOptionFunctionsConstants.Value16 ? value : null;

    private static string RefreshRateValue(string value) => value.ToUpperInvariant() switch
    {
        AtariMachineOptionFunctionsConstants.NTSC => AtariMachineOptionFunctionsConstants.Value1,
        AtariMachineOptionFunctionsConstants.PAL => AtariMachineOptionFunctionsConstants.Value2,
        _ => AtariMachineOptionFunctionsConstants.Auto
    };

    private static bool SecondJoystick(AtariMachineConfiguration configuration) =>
        configuration.Input.Controllers is not { } controllers
        || controllers.Any(binding => binding.Port == 1 && binding.Peripheral != AtariPeripheralCategory.None);

    private static string? MouseSpeedValue(string value)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var percent)) return null;
        return percent switch
        {
            <= 50 => AtariMachineOptionFunctionsConstants.Value1,
            <= 100 => AtariMachineOptionFunctionsConstants.Value2,
            <= 125 => AtariMachineOptionFunctionsConstants.Value3,
            <= 150 => AtariMachineOptionFunctionsConstants.Value4,
            <= 175 => AtariMachineOptionFunctionsConstants.Value5,
            _ => AtariMachineOptionFunctionsConstants.Value6
        };
    }

    private static void ApplyFloppySettings(AtariMachineConfiguration configuration,
        IDictionary<string, string> result)
    {
        var slot = configuration.Media.FirstOrDefault(media => media.Category == AtariMediaCategory.Floppy
            && media.IsInserted)?.Slot ?? EmulationMediaSlot.Floppy0;
        if (configuration.Options.TryGetValue(AtariMachineOptionConstants.FloppySpeedPrefix + slot, out var speed))
            result[AtariMachineOptionConstants.FastFloppy] = speed == AtariMachineOptionFunctionsConstants.Value100 ? AtariMachineOptionFunctionsConstants.False : AtariMachineOptionFunctionsConstants.True;
        if (configuration.Options.TryGetValue(AtariMachineOptionConstants.FloppyWriteProtectionPrefix + slot, out var protection)
            && bool.TryParse(protection, out var protectedMedia))
            result[AtariMachineOptionConstants.FloppyWriteProtection] = protectedMedia ? AtariMachineOptionFunctionsConstants.On : AtariMachineOptionFunctionsConstants.Off;
    }
}
