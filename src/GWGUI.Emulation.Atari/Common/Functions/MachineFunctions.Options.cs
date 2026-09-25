using System.Globalization;

namespace GWGUI.Emulation.Atari.Common.Functions;

internal static class MachineOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Apply(MachineConfiguration configuration)
    {
        var showDriveActivity = configuration.Options.GetValueOrDefault(
            MachineOptionConstants.DriveActivity, MachineOptionFunctionsConstants.False);
        var result = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal)
        {
            [MachineOptionConstants.MachineType] = MachineTypeFor(configuration.Model),
            [MachineOptionConstants.DisableMouse] = MachineOptionFunctionsConstants.False,
            // Hatari calls its joypad-driven pointer mode "mouse mode". GW GUI supplies a real
            // relative mouse through RETRO_DEVICE_MOUSE, which Hatari reads in the opposite mode.
            [MachineOptionConstants.StartInMouseMode] = MachineOptionFunctionsConstants.False,
            [MachineOptionConstants.DisableKeyboard] = MachineOptionFunctionsConstants.False,
            [MachineOptionConstants.TwoJoysticks] = SecondJoystick(configuration) ? MachineOptionFunctionsConstants.True : MachineOptionFunctionsConstants.False,
            [MachineOptionConstants.DriveActivity] = showDriveActivity,
            // Older Hatari builds only paint drive LEDs inside their status line. Newer builds
            // also draw the compact OSD bars, so setting both keeps this option effective.
            [MachineOptionConstants.InputStatusDisplay] = string.Equals(showDriveActivity, MachineOptionFunctionsConstants.True,
                StringComparison.OrdinalIgnoreCase) ? MachineOptionFunctionsConstants.Value1 : MachineOptionFunctionsConstants.Value0,
            [MachineOptionConstants.AutoloadConfiguration] = MachineOptionFunctionsConstants.False
        };
        Copy(result, MachineOptionConstants.MainMemory, MachineOptionConstants.RamSize, RamValue);
        Copy(result, MachineOptionConstants.Frequency, MachineOptionConstants.CpuFrequency, CpuFrequencyValue);
        Copy(result, ConfigurationOptionConstants.VideoStandard, MachineOptionConstants.HighResolution,
            value => string.Equals(value, MachineOptionFunctionsConstants.Monochrome, StringComparison.OrdinalIgnoreCase) ? MachineOptionFunctionsConstants.True : MachineOptionFunctionsConstants.False);
        Copy(result, ConfigurationOptionConstants.VideoStandard, MachineOptionConstants.RefreshRate, RefreshRateValue);
        Copy(result, MachineOptionConstants.Crop, MachineOptionConstants.CropOverscan,
            value => string.Equals(value, MachineOptionFunctionsConstants.Enabled, StringComparison.OrdinalIgnoreCase) ? MachineOptionFunctionsConstants.True : MachineOptionFunctionsConstants.False);
        Copy(result, MachineOptionConstants.Frames, MachineOptionConstants.FrameSkip, value => value);
        Copy(result, MachineOptionConstants.PointerSpeed, MachineOptionConstants.MouseSpeed, MouseSpeedValue);
        ApplyFloppySettings(configuration, result);
        return result;
    }

    private static void Copy(IDictionary<string, string> values, string source, string target,
        Func<string, string?> convert)
    {
        if (values.TryGetValue(source, out var value) && convert(value) is { } converted)
            values[target] = converted;
    }

    private static string MachineTypeFor(MachineModel model) => model switch
    {
        MachineModel.St or MachineModel.Stf or MachineModel.Stfm or MachineModel.MegaSt => MachineOptionFunctionsConstants.St,
        MachineModel.Ste or MachineModel.MegaSte => MachineOptionFunctionsConstants.Ste,
        MachineModel.Tt => MachineOptionFunctionsConstants.Tt,
        MachineModel.Falcon => MachineOptionFunctionsConstants.Falcon,
        _ => throw new ArgumentOutOfRangeException(nameof(model), model, ErrorMessages.UnknownStModel)
    };

    private static string? RamValue(string value)
    {
        if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var bytes)) return null;
        return bytes switch
        {
            524288 => MachineOptionFunctionsConstants.Value0,
            1048576 => MachineOptionFunctionsConstants.Value1,
            2097152 => MachineOptionFunctionsConstants.Value2,
            4194304 => MachineOptionFunctionsConstants.Value4,
            8388608 => MachineOptionFunctionsConstants.Value8,
            14680064 => MachineOptionFunctionsConstants.Value14,
            _ => null
        };
    }

    private static string? CpuFrequencyValue(string value) => value is MachineOptionFunctionsConstants.Value8 or MachineOptionFunctionsConstants.Value16 ? value : null;

    private static string RefreshRateValue(string value) => value.ToUpperInvariant() switch
    {
        MachineOptionFunctionsConstants.NTSC => MachineOptionFunctionsConstants.Value1,
        MachineOptionFunctionsConstants.PAL => MachineOptionFunctionsConstants.Value2,
        _ => MachineOptionFunctionsConstants.Auto
    };

    private static bool SecondJoystick(MachineConfiguration configuration) =>
        configuration.Input.Controllers is not { } controllers
        || controllers.Any(binding => binding.Port == 1 && binding.Peripheral != PeripheralCategory.None);

    private static string? MouseSpeedValue(string value)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var percent)) return null;
        return percent switch
        {
            <= 50 => MachineOptionFunctionsConstants.Value1,
            <= 100 => MachineOptionFunctionsConstants.Value2,
            <= 125 => MachineOptionFunctionsConstants.Value3,
            <= 150 => MachineOptionFunctionsConstants.Value4,
            <= 175 => MachineOptionFunctionsConstants.Value5,
            _ => MachineOptionFunctionsConstants.Value6
        };
    }

    private static void ApplyFloppySettings(MachineConfiguration configuration,
        IDictionary<string, string> result)
    {
        var slot = configuration.Media.FirstOrDefault(media => media.Category == MediaCategory.Floppy
            && media.IsInserted)?.Slot ?? EmulationMediaSlot.Floppy0;
        if (configuration.Options.TryGetValue(MachineOptionConstants.FloppySpeedPrefix + slot, out var speed))
            result[MachineOptionConstants.FastFloppy] = speed == MachineOptionFunctionsConstants.Value100 ? MachineOptionFunctionsConstants.False : MachineOptionFunctionsConstants.True;
        if (configuration.Options.TryGetValue(MachineOptionConstants.FloppyWriteProtectionPrefix + slot, out var protection)
            && bool.TryParse(protection, out var protectedMedia))
            result[MachineOptionConstants.FloppyWriteProtection] = protectedMedia ? MachineOptionFunctionsConstants.On : MachineOptionFunctionsConstants.Off;
    }
}
