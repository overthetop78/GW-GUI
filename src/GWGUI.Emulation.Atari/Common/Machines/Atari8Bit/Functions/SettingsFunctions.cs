using GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

namespace GWGUI.Emulation.Atari.Common.Machines.Atari8Bit.Functions;

public static class EightBitSettingsFunctions
{
    public static IReadOnlyDictionary<string, string> Normalize(
        MachineConfiguration configuration)
    {
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
        if (EightBitSettingsCatalog.SupportsComputerOptions(configuration.Model))
        {
            Validate(options, VideoAudioSettingsConstants.ResolutionOption,
                EightBitSettingsCatalog.OriginalComputerResolutions,
                EightBitSettingsCatalog.OriginalComputerResolutions[0]);
            SetDefault(options, VideoAudioSettingsConstants.ResolutionOption,
                EightBitSettingsCatalog.OriginalComputerResolutions[0]);
        }

        if (configuration.Model == MachineModel.Atari400)
            foreach (var setting in EightBitSettingsCatalog.NativeSettings.Where(setting =>
                         setting.Atari400Disposition is EightBitSettingDisposition.DifferentModel
                             or EightBitSettingDisposition.HiddenInternal))
                options.Remove(setting.Key);

        Validate(options, VideoAudioSettingsConstants.StandardOption,
            Enum.GetNames<HardwareRegion>(), HardwareRegion.Ntsc.ToString());
        Validate(options, EightBitSettingsConstants.ArtifactingModeOptionKey,
            EightBitSettingsCatalog.ArtifactingModes, EightBitSettingsConstants.None);

        Validate(options, EightBitSettingsConstants.ColorHueOptionKey,
            EightBitSettingsCatalog.ColorAdjustments, EightBitSettingsConstants.DefaultColorAdjustment);
        Validate(options, EightBitSettingsConstants.ColorSaturationOptionKey,
            EightBitSettingsCatalog.ColorAdjustments, EightBitSettingsConstants.DefaultColorAdjustment);
        Validate(options, EightBitSettingsConstants.ColorContrastOptionKey,
            EightBitSettingsCatalog.ContrastAndBrightness, EightBitSettingsConstants.DefaultColorAdjustment);
        Validate(options, EightBitSettingsConstants.ColorBrightnessOptionKey,
            EightBitSettingsCatalog.ContrastAndBrightness, EightBitSettingsConstants.DefaultColorAdjustment);
        Validate(options, EightBitSettingsConstants.ColorGammaOptionKey,
            EightBitSettingsCatalog.GammaValues, EightBitSettingsConstants.DefaultGamma);
        Validate(options, EightBitSettingsConstants.ColorDelayOptionKey,
            EightBitSettingsCatalog.ColorDelayValues, EightBitSettingsConstants.DefaultColorDelay);
        Validate(options, EightBitSettingsConstants.ExternalPaletteOptionKey,
            EightBitSettingsCatalog.ExternalPalettes, EightBitSettingsConstants.None);
        Validate(options, EightBitSettingsConstants.ControllerCompatibilityOptionKey,
            EightBitSettingsCatalog.ControllerCompatibilityModes, EightBitSettingsConstants.None);
        Validate(options, EightBitSettingsConstants.PaddleMovementSpeedOptionKey,
            EightBitSettingsCatalog.PaddleMovementSpeeds,
            EightBitSettingsConstants.DefaultPaddleMovementSpeed);
        Validate(options, EightBitSettingsConstants.DigitalSensitivityOptionKey,
            EightBitSettingsCatalog.Sensitivities, EightBitSettingsConstants.DefaultSensitivity);
        Validate(options, EightBitSettingsConstants.AnalogSensitivityOptionKey,
            EightBitSettingsCatalog.Sensitivities, EightBitSettingsConstants.DefaultSensitivity);
        Validate(options, EightBitSettingsConstants.AutofireOptionKey,
            EightBitSettingsCatalog.AutofireModes, EightBitSettingsConstants.Disabled);
        if (configuration.Model == MachineModel.Atari400)
        {
            options[EightBitSettingsConstants.Os400800OptionKey] = EightBitSettingsFunctionsConstants.Auto;
            options[EightBitSettingsConstants.BasicVersionOptionKey] = EightBitSettingsFunctionsConstants.Auto;
        }
        Validate(options, EightBitSettingsConstants.MosaicMemoryOptionKey,
            EightBitSettingsCatalog.Mosaic(configuration.Model).Select(choice => choice.Value).ToArray(),
            EightBitSettingsConstants.Disabled);
        Validate(options, EightBitSettingsConstants.AxlonMemoryOptionKey,
            EightBitSettingsCatalog.Axlon(configuration.Model).Select(choice => choice.Value).ToArray(),
            EightBitSettingsConstants.Disabled);
        foreach (var key in new[]
                 {
                     EightBitSettingsConstants.PaddleActiveOptionKey,
                     EightBitSettingsConstants.AxlonShadowOptionKey,
                     EightBitSettingsConstants.BasicEnabledOptionKey,
                     EightBitSettingsConstants.ShowSpeedOptionKey,
                     EightBitSettingsConstants.ShowSectorOptionKey,
                     EightBitSettingsConstants.RealTimeClockOptionKey,
                     EightBitSettingsConstants.PrinterDeviceOptionKey,
                     EightBitSettingsConstants.SerialDeviceOptionKey,
                     EightBitSettingsConstants.CassetteBootOptionKey,
                     EightBitSettingsConstants.PokeyStereoOptionKey
                 })
            Validate(options, key, EightBitSettingsCatalog.ToggleModes,
                EightBitSettingsConstants.Disabled);
        Validate(options, EightBitSettingsConstants.ShowActivityOptionKey,
            EightBitSettingsCatalog.ToggleModes, EightBitSettingsConstants.Enabled);
        Validate(options, EightBitSettingsConstants.SioAccelerationOptionKey,
            EightBitSettingsCatalog.ToggleModes, EightBitSettingsConstants.Enabled);

        SetDefault(options, EightBitSettingsConstants.ControllerCompatibilityOptionKey,
            EightBitSettingsConstants.None);
        SetDefault(options, VideoAudioSettingsConstants.StandardOption,
            HardwareModelCatalog.Get(configuration.Model).DefaultRegion.ToString());
        SetDefault(options, EightBitSettingsConstants.ArtifactingModeOptionKey,
            EightBitSettingsConstants.None);
        SetDefault(options, EightBitSettingsConstants.PaddleActiveOptionKey,
            EightBitSettingsConstants.Disabled);
        SetDefault(options, EightBitSettingsConstants.PaddleMovementSpeedOptionKey,
            EightBitSettingsConstants.DefaultPaddleMovementSpeed);
        SetDefault(options, EightBitSettingsConstants.DigitalSensitivityOptionKey,
            EightBitSettingsConstants.DefaultSensitivity);
        SetDefault(options, EightBitSettingsConstants.AnalogSensitivityOptionKey,
            EightBitSettingsConstants.DefaultSensitivity);
        SetDefault(options, EightBitSettingsConstants.AutofireOptionKey,
            EightBitSettingsConstants.Disabled);
        SetDefault(options, EightBitSettingsConstants.MosaicMemoryOptionKey,
            EightBitSettingsConstants.Disabled);
        SetDefault(options, EightBitSettingsConstants.AxlonMemoryOptionKey,
            EightBitSettingsConstants.Disabled);
        SetDefault(options, EightBitSettingsConstants.AxlonShadowOptionKey,
            EightBitSettingsConstants.Disabled);
        SetDefault(options, EightBitSettingsConstants.BasicEnabledOptionKey,
            EightBitSettingsConstants.Disabled);
        SetDefault(options, EightBitSettingsConstants.ShowSpeedOptionKey,
            EightBitSettingsConstants.Disabled);
        SetDefault(options, EightBitSettingsConstants.ShowActivityOptionKey,
            EightBitSettingsConstants.Enabled);
        SetDefault(options, EightBitSettingsConstants.ShowSectorOptionKey,
            EightBitSettingsConstants.Disabled);
        SetDefault(options, EightBitSettingsConstants.RealTimeClockOptionKey,
            EightBitSettingsConstants.Disabled);
        SetDefault(options, EightBitSettingsConstants.PrinterDeviceOptionKey,
            EightBitSettingsConstants.Disabled);
        SetDefault(options, EightBitSettingsConstants.SerialDeviceOptionKey,
            EightBitSettingsConstants.Disabled);
        options[EightBitSettingsConstants.SioAccelerationOptionKey] =
            EightBitSettingsConstants.Enabled;
        SetDefault(options, EightBitSettingsConstants.CassetteBootOptionKey,
            EightBitSettingsConstants.Disabled);
        SetDefault(options, EightBitSettingsConstants.PokeyStereoOptionKey,
            EightBitSettingsConstants.Disabled);

        var mosaicEnabled = options[EightBitSettingsConstants.MosaicMemoryOptionKey]
            != EightBitSettingsConstants.Disabled;
        var axlonEnabled = options[EightBitSettingsConstants.AxlonMemoryOptionKey]
            != EightBitSettingsConstants.Disabled;
        if (mosaicEnabled && axlonEnabled)
            options[EightBitSettingsConstants.AxlonMemoryOptionKey] = EightBitSettingsConstants.Disabled;
        if (options[EightBitSettingsConstants.AxlonMemoryOptionKey]
            == EightBitSettingsConstants.Disabled)
            options[EightBitSettingsConstants.AxlonShadowOptionKey] = EightBitSettingsConstants.Disabled;

        if (options.GetValueOrDefault(EightBitSettingsConstants.PaddleActiveOptionKey)
            == EightBitSettingsConstants.Enabled)
            options[EightBitSettingsConstants.ControllerCompatibilityOptionKey] =
                EightBitSettingsConstants.None;
        options[EightBitSettingsConstants.AnalogDeadZoneOptionKey] =
            EightBitSettingsConstants.NeutralAnalogDeadZone;
        return options;
    }

    private static void Validate(IDictionary<string, string> options, string key,
        IReadOnlyList<string> allowed, string fallback)
    {
        if (options.TryGetValue(key, out var value) && !allowed.Contains(value)) options[key] = fallback;
    }

    private static void SetDefault(IDictionary<string, string> options, string key, string value)
    {
        if (!options.ContainsKey(key)) options[key] = value;
    }
}
