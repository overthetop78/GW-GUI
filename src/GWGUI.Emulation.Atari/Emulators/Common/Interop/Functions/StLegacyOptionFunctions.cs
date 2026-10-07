using System.Globalization;


namespace GWGUI.Emulation.Atari.Emulators.Common.Interop.Functions;

internal static class StLegacyOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> ApplyWriteProtection(
        IReadOnlyDictionary<string, string> options, StStorage? storage)
    {
        var result = new Dictionary<string, string>(options, StringComparer.Ordinal);
        if (storage is not null)
            result[StLegacyOptionConstants.HardDriveWriteProtectionOption] = storage.Configuration.IsReadOnly
                ? StLegacyOptionConstants.WriteProtectionEnabled : StLegacyOptionConstants.WriteProtectionDisabled;
        return result;
    }
    private static readonly IReadOnlyDictionary<string, string> GenericToNative =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [MachineOptionConstants.DriveActivity] = StLegacyOptionConstants.DriveActivity,
            [SettingsDescriptionFunctionsConstants.FastBoot] = StLegacyOptionConstants.FastBoot,
            [VideoAudioSettingsConstants.FloppySoundOption] = StLegacyOptionConstants.FloppySound,
            [VideoAudioSettingsConstants.FloppySoundVolumeOption] = StLegacyOptionConstants.FloppySoundVolume,
            [VideoAudioSettingsConstants.PolarizedFilterOption] = StLegacyOptionConstants.PolarizedFilter,
            [MachineValues.ResetType] = StLegacyOptionConstants.ResetType
        };

    internal static IReadOnlyDictionary<string, string> ToNativeOptions(
        IReadOnlyDictionary<string, string> options)
    {
        var translated = new Dictionary<string, string>(options, StringComparer.Ordinal);
        foreach (var mapping in GenericToNative)
        {
            if (translated.Remove(mapping.Key, out var value)) translated[mapping.Value] = value;
        }
        return translated;
    }

    internal static IReadOnlyDictionary<string, string> Apply(MachineConfiguration configuration)
    {
        var showDriveActivity = configuration.Options.GetValueOrDefault(
            MachineOptionConstants.DriveActivity, MachineOptionFunctionsConstants.False);
        var result = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal)
        {
            [StLegacyOptionConstants.MachineType] = MachineTypeFor(configuration.Model),
            [StLegacyOptionConstants.DisableMouse] = MachineOptionFunctionsConstants.False,
            [StLegacyOptionConstants.StartInMouseMode] = MachineOptionFunctionsConstants.False,
            [StLegacyOptionConstants.DisableKeyboard] = MachineOptionFunctionsConstants.False,
            [StLegacyOptionConstants.TwoJoysticks] = SecondJoystick(configuration)
                ? MachineOptionFunctionsConstants.True : MachineOptionFunctionsConstants.False,
            [StLegacyOptionConstants.DriveActivity] = showDriveActivity,
            [StLegacyOptionConstants.InputStatusDisplay] = string.Equals(showDriveActivity,
                MachineOptionFunctionsConstants.True, StringComparison.OrdinalIgnoreCase)
                ? MachineOptionFunctionsConstants.Value1 : MachineOptionFunctionsConstants.Value0,
            [StLegacyOptionConstants.AutoloadConfiguration] = MachineOptionFunctionsConstants.False
        };
        Copy(result, MachineOptionConstants.MainMemory, StLegacyOptionConstants.RamSize, RamValue);
        Copy(result, MachineOptionConstants.Frequency, StLegacyOptionConstants.CpuFrequency, CpuFrequencyValue);
        Copy(result, VideoAudioSettingsConstants.StandardOption, StLegacyOptionConstants.HighResolution,
            value => string.Equals(value, MachineOptionFunctionsConstants.Monochrome,
                StringComparison.OrdinalIgnoreCase) ? MachineOptionFunctionsConstants.True : MachineOptionFunctionsConstants.False);
        Copy(result, VideoAudioSettingsConstants.StandardOption, StLegacyOptionConstants.RefreshRate, RefreshRateValue);
        Copy(result, MachineOptionConstants.Crop, StLegacyOptionConstants.CropOverscan,
            value => string.Equals(value, MachineOptionFunctionsConstants.Enabled,
                StringComparison.OrdinalIgnoreCase) ? MachineOptionFunctionsConstants.True : MachineOptionFunctionsConstants.False);
        Copy(result, MachineOptionConstants.Frames, StLegacyOptionConstants.FrameSkip, value => value);
        Copy(result, MachineOptionConstants.PointerSpeed, StLegacyOptionConstants.MouseSpeed, MouseSpeedValue);
        ApplyFloppySettings(configuration, result);
        return ToNativeOptions(result);
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

    private static string? CpuFrequencyValue(string value) =>
        value is MachineOptionFunctionsConstants.Value8 or MachineOptionFunctionsConstants.Value16 ? value : null;

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
            result[StLegacyOptionConstants.FastFloppy] = speed == MachineOptionFunctionsConstants.Value100
                ? MachineOptionFunctionsConstants.False : MachineOptionFunctionsConstants.True;
        if (configuration.Options.TryGetValue(MachineOptionConstants.FloppyWriteProtectionPrefix + slot, out var protection)
            && bool.TryParse(protection, out var protectedMedia))
            result[StLegacyOptionConstants.FloppyWriteProtection] = protectedMedia
                ? MachineOptionFunctionsConstants.On : MachineOptionFunctionsConstants.Off;
    }
}
