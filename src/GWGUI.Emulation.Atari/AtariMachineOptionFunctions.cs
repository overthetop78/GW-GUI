using System.Globalization;

namespace GWGUI.Emulation.Atari;

internal static class AtariMachineOptionFunctions
{
    private const string MachineType = "hatari_machinetype";
    private const string RamSize = "hatari_ramsize";
    private const string CpuFrequency = "hatari_cpu_freq";
    private const string HighResolution = "hatari_video_hires";
    private const string RefreshRate = "hatari_forcerefresh";
    private const string CropOverscan = "hatari_video_crop_overscan";
    private const string FrameSkip = "hatari_frameskips";
    private const string MouseSpeed = "hatari_emulated_mouse_speed";
    private const string FastFloppy = "hatari_fastfdc";
    private const string FloppyWriteProtection = "hatari_writeprotect_floppy";

    private const string MainMemory = "gwgui_atari_main_memory";
    private const string Frequency = "gwgui_atari_cpu_frequency";
    private const string VideoStandard = "gwgui_atari_video_standard";
    private const string Crop = "gwgui_atari_video_crop";
    private const string Frames = "gwgui_atari_video_frameskip";
    private const string PointerSpeed = "gwgui_atari_mouse_speed";
    private const string FloppySpeedPrefix = "storage.speed.";
    private const string FloppyWriteProtectionPrefix = "storage.writeProtected.";

    internal static IReadOnlyDictionary<string, string> Apply(AtariMachineConfiguration configuration)
    {
        var showDriveActivity = configuration.Options.GetValueOrDefault(
            "hatari_led_status_display", "false");
        var result = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal)
        {
            [MachineType] = MachineTypeFor(configuration.Model),
            ["hatari_nomouse"] = "false",
            // Hatari calls its joypad-driven pointer mode "mouse mode". GW GUI supplies a real
            // relative mouse through RETRO_DEVICE_MOUSE, which Hatari reads in the opposite mode.
            ["hatari_start_in_mouse_mode"] = "false",
            ["hatari_nokeys"] = "false",
            ["hatari_twojoy"] = SecondJoystick(configuration) ? "true" : "false",
            ["hatari_led_status_display"] = showDriveActivity,
            // Older Hatari builds only paint drive LEDs inside their status line. Newer builds
            // also draw the compact OSD bars, so setting both keeps this option effective.
            ["hatari_joymousestatus_display"] = string.Equals(showDriveActivity, "true",
                StringComparison.OrdinalIgnoreCase) ? "1" : "0",
            ["hatari_autoload_config"] = "false"
        };
        Copy(result, MainMemory, RamSize, RamValue);
        Copy(result, Frequency, CpuFrequency, CpuFrequencyValue);
        Copy(result, VideoStandard, HighResolution,
            value => string.Equals(value, "Monochrome", StringComparison.OrdinalIgnoreCase) ? "true" : "false");
        Copy(result, VideoStandard, RefreshRate, RefreshRateValue);
        Copy(result, Crop, CropOverscan,
            value => string.Equals(value, "enabled", StringComparison.OrdinalIgnoreCase) ? "true" : "false");
        Copy(result, Frames, FrameSkip, value => value);
        Copy(result, PointerSpeed, MouseSpeed, MouseSpeedValue);
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
        || controllers.Any(binding => binding.Port == 1 && binding.Peripheral != AtariPeripheralKind.None);

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
        var slot = configuration.Media.FirstOrDefault(media => media.Kind == AtariMediaKind.Floppy
            && media.IsInserted)?.Slot ?? Emulation.EmulationMediaSlot.Floppy0;
        if (configuration.Options.TryGetValue(FloppySpeedPrefix + slot, out var speed))
            result[FastFloppy] = speed == "100" ? "false" : "true";
        if (configuration.Options.TryGetValue(FloppyWriteProtectionPrefix + slot, out var protection)
            && bool.TryParse(protection, out var protectedMedia))
            result[FloppyWriteProtection] = protectedMedia ? "on" : "off";
    }
}
