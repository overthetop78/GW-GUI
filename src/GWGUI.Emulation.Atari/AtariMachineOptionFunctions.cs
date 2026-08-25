using System.Globalization;

namespace GWGUI.Emulation.Atari;

internal static class AtariMachineOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Apply(AtariMachineConfiguration configuration)
    {
        var showDriveActivity = configuration.Options.GetValueOrDefault(
            AtariMachineOptionConstants.DriveActivity, "false");
        var result = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal)
        {
            [AtariMachineOptionConstants.MachineType] = MachineTypeFor(configuration.Model),
            [AtariMachineOptionConstants.DisableMouse] = "false",
            // Hatari calls its joypad-driven pointer mode "mouse mode". GW GUI supplies a real
            // relative mouse through RETRO_DEVICE_MOUSE, which Hatari reads in the opposite mode.
            [AtariMachineOptionConstants.StartInMouseMode] = "false",
            [AtariMachineOptionConstants.DisableKeyboard] = "false",
            [AtariMachineOptionConstants.TwoJoysticks] = SecondJoystick(configuration) ? "true" : "false",
            [AtariMachineOptionConstants.DriveActivity] = showDriveActivity,
            // Older Hatari builds only paint drive LEDs inside their status line. Newer builds
            // also draw the compact OSD bars, so setting both keeps this option effective.
            [AtariMachineOptionConstants.InputStatusDisplay] = string.Equals(showDriveActivity, "true",
                StringComparison.OrdinalIgnoreCase) ? "1" : "0",
            [AtariMachineOptionConstants.AutoloadConfiguration] = "false"
        };
        Copy(result, AtariMachineOptionConstants.MainMemory, AtariMachineOptionConstants.RamSize, RamValue);
        Copy(result, AtariMachineOptionConstants.Frequency, AtariMachineOptionConstants.CpuFrequency, CpuFrequencyValue);
        Copy(result, AtariConfigurationOptionConstants.VideoStandard, AtariMachineOptionConstants.HighResolution,
            value => string.Equals(value, "Monochrome", StringComparison.OrdinalIgnoreCase) ? "true" : "false");
        Copy(result, AtariConfigurationOptionConstants.VideoStandard, AtariMachineOptionConstants.RefreshRate, RefreshRateValue);
        Copy(result, AtariMachineOptionConstants.Crop, AtariMachineOptionConstants.CropOverscan,
            value => string.Equals(value, "enabled", StringComparison.OrdinalIgnoreCase) ? "true" : "false");
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
        AtariMachineModel.St or AtariMachineModel.Stf or AtariMachineModel.Stfm or AtariMachineModel.MegaSt => "st",
        AtariMachineModel.Ste or AtariMachineModel.MegaSte => "ste",
        AtariMachineModel.Tt => "tt",
        AtariMachineModel.Falcon => "falcon",
        _ => throw new ArgumentOutOfRangeException(nameof(model), model, AtariErrorMessages.UnknownStModel)
    };

    private static string? RamValue(string value)
    {
        if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var bytes)) return null;
        return bytes switch
        {
            524288 => "0",
            1048576 => "1",
            2097152 => "2",
            4194304 => "4",
            8388608 => "8",
            14680064 => "14",
            _ => null
        };
    }

    private static string? CpuFrequencyValue(string value) => value is "8" or "16" ? value : null;

    private static string RefreshRateValue(string value) => value.ToUpperInvariant() switch
    {
        "NTSC" => "1",
        "PAL" => "2",
        _ => "auto"
    };

    private static bool SecondJoystick(AtariMachineConfiguration configuration) =>
        configuration.Input.Controllers is not { } controllers
        || controllers.Any(binding => binding.Port == 1 && binding.Peripheral != AtariPeripheralCategory.None);

    private static string? MouseSpeedValue(string value)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var percent)) return null;
        return percent switch
        {
            <= 50 => "1",
            <= 100 => "2",
            <= 125 => "3",
            <= 150 => "4",
            <= 175 => "5",
            _ => "6"
        };
    }

    private static void ApplyFloppySettings(AtariMachineConfiguration configuration,
        IDictionary<string, string> result)
    {
        var slot = configuration.Media.FirstOrDefault(media => media.Category == AtariMediaCategory.Floppy
            && media.IsInserted)?.Slot ?? EmulationMediaSlot.Floppy0;
        if (configuration.Options.TryGetValue(AtariMachineOptionConstants.FloppySpeedPrefix + slot, out var speed))
            result[AtariMachineOptionConstants.FastFloppy] = speed == "100" ? "false" : "true";
        if (configuration.Options.TryGetValue(AtariMachineOptionConstants.FloppyWriteProtectionPrefix + slot, out var protection)
            && bool.TryParse(protection, out var protectedMedia))
            result[AtariMachineOptionConstants.FloppyWriteProtection] = protectedMedia ? "on" : "off";
    }
}
