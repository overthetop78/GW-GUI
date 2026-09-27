using GWGUI.Emulation.Atari.Emulators.Atari800.Constants;

namespace GWGUI.Emulation.Atari.Emulators.Atari800.Functions;

internal static class Atari800OptionFunctions
{
    private static readonly (string Generic, string Native)[] Mappings =
    [
        (EightBitSettingsConstants.SystemOptionKey, Atari800OptionConstants.System),
        (EightBitSettingsConstants.VideoStandardOptionKey, Atari800OptionConstants.VideoStandard),
        (EightBitSettingsConstants.ResolutionOptionKey, Atari800OptionConstants.Resolution),
        (EightBitSettingsConstants.PokeyStereoOptionKey, Atari800OptionConstants.PokeyStereo),
        (EightBitSettingsConstants.ArtifactingModeOptionKey, Atari800OptionConstants.ArtifactingMode),
        (EightBitSettingsConstants.ColorHueOptionKey, Atari800OptionConstants.ColorHue),
        (EightBitSettingsConstants.ColorSaturationOptionKey, Atari800OptionConstants.ColorSaturation),
        (EightBitSettingsConstants.ColorContrastOptionKey, Atari800OptionConstants.ColorContrast),
        (EightBitSettingsConstants.ColorBrightnessOptionKey, Atari800OptionConstants.ColorBrightness),
        (EightBitSettingsConstants.ColorGammaOptionKey, Atari800OptionConstants.ColorGamma),
        (EightBitSettingsConstants.ColorDelayOptionKey, Atari800OptionConstants.ColorDelay),
        (EightBitSettingsConstants.ExternalPaletteOptionKey, Atari800OptionConstants.ExternalPalette),
        (EightBitSettingsConstants.ControllerCompatibilityOptionKey, Atari800OptionConstants.ControllerCompatibility),
        (EightBitSettingsConstants.DigitalSensitivityOptionKey, Atari800OptionConstants.DigitalSensitivity),
        (EightBitSettingsConstants.AnalogSensitivityOptionKey, Atari800OptionConstants.AnalogSensitivity),
        (EightBitSettingsConstants.AnalogDeadZoneOptionKey, Atari800OptionConstants.AnalogDeadZone),
        (EightBitSettingsConstants.KeyboardModeOptionKey, Atari800OptionConstants.KeyboardMode),
        (EightBitSettingsConstants.VirtualKeyboardOptionKey, Atari800OptionConstants.VirtualKeyboard),
        (EightBitSettingsConstants.XegsKeyboardOptionKey, Atari800OptionConstants.XegsKeyboard),
        (EightBitSettingsConstants.ShowActivityOptionKey, Atari800OptionConstants.ShowActivity),
        (EightBitSettingsConstants.ShowSpeedOptionKey, Atari800OptionConstants.ShowSpeed),
        (EightBitSettingsConstants.ShowSectorOptionKey, Atari800OptionConstants.ShowSector),
        (EightBitSettingsConstants.Show1200XlLedsOptionKey, Atari800OptionConstants.Show1200XlLeds),
        (EightBitSettingsConstants.Xep80OptionKey, Atari800OptionConstants.Xep80),
        (EightBitSettingsConstants.RealTimeClockOptionKey, Atari800OptionConstants.RealTimeClock),
        (EightBitSettingsConstants.PrinterDeviceOptionKey, Atari800OptionConstants.PrinterDevice),
        (EightBitSettingsConstants.SerialDeviceOptionKey, Atari800OptionConstants.SerialDevice),
        (EightBitSettingsConstants.SlowExecutableLoadingOptionKey, Atari800OptionConstants.SlowExecutableLoading),
        (EightBitSettingsConstants.SioAccelerationOptionKey, Atari800OptionConstants.SioAcceleration),
        (EightBitSettingsConstants.CassetteBootOptionKey, Atari800OptionConstants.CassetteBoot),
        (EightBitSettingsConstants.MosaicMemoryOptionKey, Atari800OptionConstants.MosaicMemory),
        (EightBitSettingsConstants.AxlonMemoryOptionKey, Atari800OptionConstants.AxlonMemory),
        (EightBitSettingsConstants.AxlonShadowOptionKey, Atari800OptionConstants.AxlonShadow),
        (EightBitSettingsConstants.MapRamOptionKey, Atari800OptionConstants.MapRam),
        (EightBitSettingsConstants.Os400800OptionKey, Atari800OptionConstants.Os400800),
        (EightBitSettingsConstants.BasicEnabledOptionKey, Atari800OptionConstants.BasicEnabled),
        (EightBitSettingsConstants.BasicVersionOptionKey, Atari800OptionConstants.BasicVersion),
        (EightBitSettingsConstants.XlOsOptionKey, Atari800OptionConstants.XlOs),
        (EightBitSettingsConstants.ConsoleOsOptionKey, Atari800OptionConstants.ConsoleOs),
        (EightBitSettingsConstants.LegacyConfigurationOptionKey, Atari800OptionConstants.LegacyConfiguration),
        (EightBitSettingsConstants.PaddleActiveOptionKey, Atari800OptionConstants.PaddleActive),
        (EightBitSettingsConstants.PaddleMovementSpeedOptionKey, Atari800OptionConstants.PaddleMovementSpeed),
        (EightBitSettingsConstants.AutofireOptionKey, Atari800OptionConstants.Autofire)
    ];

    internal static IReadOnlyDictionary<string, string> ToNative(
        IReadOnlyDictionary<string, string> options) => Translate(options);

    private static IReadOnlyDictionary<string, string> Translate(
        IReadOnlyDictionary<string, string> options)
    {
        var translated = new Dictionary<string, string>(options, StringComparer.Ordinal);
        foreach (var mapping in Mappings)
        {
            if (translated.Remove(mapping.Generic, out var value))
                translated[mapping.Native] = value;
        }
        return translated;
    }
}
