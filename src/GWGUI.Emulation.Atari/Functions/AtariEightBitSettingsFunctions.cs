namespace GWGUI.Emulation.Atari.Functions;

public static class AtariEightBitSettingsFunctions
{
    public static IReadOnlyDictionary<string, string> Normalize(
        AtariMachineConfiguration configuration)
    {
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
        if (AtariEightBitSettingsCatalog.SupportsComputerOptions(configuration.Model))
        {
            Validate(options, AtariConfigurationOptionConstants.VideoResolution,
                AtariEightBitSettingsCatalog.OriginalComputerResolutions,
                AtariEightBitSettingsCatalog.OriginalComputerResolutions[0]);
            SetDefault(options, AtariConfigurationOptionConstants.VideoResolution,
                AtariEightBitSettingsCatalog.OriginalComputerResolutions[0]);
        }

        if (configuration.Model != AtariMachineModel.Atari400) return options;

        foreach (var setting in AtariEightBitSettingsCatalog.NativeSettings.Where(setting =>
                     setting.Atari400Disposition is AtariEightBitSettingDisposition.DifferentModel
                         or AtariEightBitSettingDisposition.HiddenInternal))
            options.Remove(setting.Key);

        Validate(options, AtariConfigurationOptionConstants.VideoStandard,
            Enum.GetNames<AtariClassicRegion>(), AtariClassicRegion.Ntsc.ToString());
        Validate(options, AtariEightBitSettingsConstants.ArtifactingModeOptionKey,
            AtariEightBitSettingsCatalog.ArtifactingModes, AtariEightBitSettingsConstants.None);

        Validate(options, AtariEightBitSettingsConstants.ColorHueOptionKey,
            AtariEightBitSettingsCatalog.ColorAdjustments, AtariEightBitSettingsConstants.DefaultColorAdjustment);
        Validate(options, AtariEightBitSettingsConstants.ColorSaturationOptionKey,
            AtariEightBitSettingsCatalog.ColorAdjustments, AtariEightBitSettingsConstants.DefaultColorAdjustment);
        Validate(options, AtariEightBitSettingsConstants.ColorContrastOptionKey,
            AtariEightBitSettingsCatalog.ContrastAndBrightness, AtariEightBitSettingsConstants.DefaultColorAdjustment);
        Validate(options, AtariEightBitSettingsConstants.ColorBrightnessOptionKey,
            AtariEightBitSettingsCatalog.ContrastAndBrightness, AtariEightBitSettingsConstants.DefaultColorAdjustment);
        Validate(options, AtariEightBitSettingsConstants.ColorGammaOptionKey,
            AtariEightBitSettingsCatalog.GammaValues, AtariEightBitSettingsConstants.DefaultGamma);
        Validate(options, AtariEightBitSettingsConstants.ColorDelayOptionKey,
            AtariEightBitSettingsCatalog.ColorDelayValues, AtariEightBitSettingsConstants.DefaultColorDelay);
        Validate(options, AtariEightBitSettingsConstants.ExternalPaletteOptionKey,
            AtariEightBitSettingsCatalog.ExternalPalettes, AtariEightBitSettingsConstants.None);
        Validate(options, AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey,
            AtariEightBitSettingsCatalog.ControllerCompatibilityModes, AtariEightBitSettingsConstants.None);
        Validate(options, AtariEightBitSettingsConstants.PaddleMovementSpeedOptionKey,
            AtariEightBitSettingsCatalog.PaddleMovementSpeeds,
            AtariEightBitSettingsConstants.DefaultPaddleMovementSpeed);
        Validate(options, AtariEightBitSettingsConstants.DigitalSensitivityOptionKey,
            AtariEightBitSettingsCatalog.Sensitivities, AtariEightBitSettingsConstants.DefaultSensitivity);
        Validate(options, AtariEightBitSettingsConstants.AnalogSensitivityOptionKey,
            AtariEightBitSettingsCatalog.Sensitivities, AtariEightBitSettingsConstants.DefaultSensitivity);
        Validate(options, AtariEightBitSettingsConstants.AutofireOptionKey,
            AtariEightBitSettingsCatalog.AutofireModes, AtariEightBitSettingsConstants.Disabled);
        options[AtariEightBitSettingsConstants.Os400800OptionKey] = AtariEightBitSettingsFunctionsConstants.Auto;
        options[AtariEightBitSettingsConstants.BasicVersionOptionKey] = AtariEightBitSettingsFunctionsConstants.Auto;
        Validate(options, AtariEightBitSettingsConstants.MosaicMemoryOptionKey,
            AtariEightBitSettingsCatalog.Mosaic(configuration.Model).Select(choice => choice.Value).ToArray(),
            AtariEightBitSettingsConstants.Disabled);
        Validate(options, AtariEightBitSettingsConstants.AxlonMemoryOptionKey,
            AtariEightBitSettingsCatalog.Axlon(configuration.Model).Select(choice => choice.Value).ToArray(),
            AtariEightBitSettingsConstants.Disabled);
        foreach (var key in new[]
                 {
                     AtariEightBitSettingsConstants.PaddleActiveOptionKey,
                     AtariEightBitSettingsConstants.AxlonShadowOptionKey,
                     AtariEightBitSettingsConstants.BasicEnabledOptionKey,
                     AtariEightBitSettingsConstants.ShowSpeedOptionKey,
                     AtariEightBitSettingsConstants.ShowSectorOptionKey,
                     AtariEightBitSettingsConstants.RealTimeClockOptionKey,
                     AtariEightBitSettingsConstants.PrinterDeviceOptionKey,
                     AtariEightBitSettingsConstants.SerialDeviceOptionKey,
                     AtariEightBitSettingsConstants.CassetteBootOptionKey,
                     AtariEightBitSettingsConstants.PokeyStereoOptionKey
                 })
            Validate(options, key, AtariEightBitSettingsCatalog.ToggleModes,
                AtariEightBitSettingsConstants.Disabled);
        Validate(options, AtariEightBitSettingsConstants.ShowActivityOptionKey,
            AtariEightBitSettingsCatalog.ToggleModes, AtariEightBitSettingsConstants.Enabled);
        Validate(options, AtariEightBitSettingsConstants.SioAccelerationOptionKey,
            AtariEightBitSettingsCatalog.ToggleModes, AtariEightBitSettingsConstants.Enabled);

        SetDefault(options, AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey,
            AtariEightBitSettingsConstants.None);
        SetDefault(options, AtariConfigurationOptionConstants.VideoStandard,
            AtariClassicModelCatalog.Get(configuration.Model).DefaultRegion.ToString());
        SetDefault(options, AtariEightBitSettingsConstants.ArtifactingModeOptionKey,
            AtariEightBitSettingsConstants.None);
        SetDefault(options, AtariEightBitSettingsConstants.PaddleActiveOptionKey,
            AtariEightBitSettingsConstants.Disabled);
        SetDefault(options, AtariEightBitSettingsConstants.PaddleMovementSpeedOptionKey,
            AtariEightBitSettingsConstants.DefaultPaddleMovementSpeed);
        SetDefault(options, AtariEightBitSettingsConstants.DigitalSensitivityOptionKey,
            AtariEightBitSettingsConstants.DefaultSensitivity);
        SetDefault(options, AtariEightBitSettingsConstants.AnalogSensitivityOptionKey,
            AtariEightBitSettingsConstants.DefaultSensitivity);
        SetDefault(options, AtariEightBitSettingsConstants.AutofireOptionKey,
            AtariEightBitSettingsConstants.Disabled);
        SetDefault(options, AtariEightBitSettingsConstants.MosaicMemoryOptionKey,
            AtariEightBitSettingsConstants.Disabled);
        SetDefault(options, AtariEightBitSettingsConstants.AxlonMemoryOptionKey,
            AtariEightBitSettingsConstants.Disabled);
        SetDefault(options, AtariEightBitSettingsConstants.AxlonShadowOptionKey,
            AtariEightBitSettingsConstants.Disabled);
        SetDefault(options, AtariEightBitSettingsConstants.BasicEnabledOptionKey,
            AtariEightBitSettingsConstants.Disabled);
        SetDefault(options, AtariEightBitSettingsConstants.ShowSpeedOptionKey,
            AtariEightBitSettingsConstants.Disabled);
        SetDefault(options, AtariEightBitSettingsConstants.ShowActivityOptionKey,
            AtariEightBitSettingsConstants.Enabled);
        SetDefault(options, AtariEightBitSettingsConstants.ShowSectorOptionKey,
            AtariEightBitSettingsConstants.Disabled);
        SetDefault(options, AtariEightBitSettingsConstants.RealTimeClockOptionKey,
            AtariEightBitSettingsConstants.Disabled);
        SetDefault(options, AtariEightBitSettingsConstants.PrinterDeviceOptionKey,
            AtariEightBitSettingsConstants.Disabled);
        SetDefault(options, AtariEightBitSettingsConstants.SerialDeviceOptionKey,
            AtariEightBitSettingsConstants.Disabled);
        SetDefault(options, AtariEightBitSettingsConstants.SioAccelerationOptionKey,
            AtariEightBitSettingsConstants.Enabled);
        SetDefault(options, AtariEightBitSettingsConstants.CassetteBootOptionKey,
            AtariEightBitSettingsConstants.Disabled);
        SetDefault(options, AtariEightBitSettingsConstants.PokeyStereoOptionKey,
            AtariEightBitSettingsConstants.Disabled);

        var mosaicEnabled = options[AtariEightBitSettingsConstants.MosaicMemoryOptionKey]
            != AtariEightBitSettingsConstants.Disabled;
        var axlonEnabled = options[AtariEightBitSettingsConstants.AxlonMemoryOptionKey]
            != AtariEightBitSettingsConstants.Disabled;
        if (mosaicEnabled && axlonEnabled)
            options[AtariEightBitSettingsConstants.AxlonMemoryOptionKey] = AtariEightBitSettingsConstants.Disabled;
        if (options[AtariEightBitSettingsConstants.AxlonMemoryOptionKey]
            == AtariEightBitSettingsConstants.Disabled)
            options[AtariEightBitSettingsConstants.AxlonShadowOptionKey] = AtariEightBitSettingsConstants.Disabled;

        if (options.GetValueOrDefault(AtariEightBitSettingsConstants.PaddleActiveOptionKey)
            == AtariEightBitSettingsConstants.Enabled)
            options[AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey] =
                AtariEightBitSettingsConstants.None;
        options[AtariEightBitSettingsConstants.AnalogDeadZoneOptionKey] =
            AtariEightBitSettingsConstants.NeutralAnalogDeadZone;
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
