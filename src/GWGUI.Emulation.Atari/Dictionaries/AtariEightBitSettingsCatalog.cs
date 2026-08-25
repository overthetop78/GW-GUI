namespace GWGUI.Emulation.Atari.Dictionaries;




public static class AtariEightBitSettingsCatalog
{
    public static readonly IReadOnlyList<AtariEightBitNativeSetting> NativeSettings =
    [
        Visible(AtariEightBitSettingsConstants.VideoStandardOptionKey),
        Visible(AtariEightBitSettingsConstants.ArtifactingModeOptionKey),
        Visible(AtariEightBitSettingsConstants.ResolutionOptionKey),
        Visible(AtariEightBitSettingsConstants.ColorHueOptionKey),
        Visible(AtariEightBitSettingsConstants.ColorSaturationOptionKey),
        Visible(AtariEightBitSettingsConstants.ColorContrastOptionKey),
        Visible(AtariEightBitSettingsConstants.ColorBrightnessOptionKey),
        Visible(AtariEightBitSettingsConstants.ColorGammaOptionKey),
        Visible(AtariEightBitSettingsConstants.ColorDelayOptionKey),
        Visible(AtariEightBitSettingsConstants.ExternalPaletteOptionKey),
        Visible(AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey),
        Visible(AtariEightBitSettingsConstants.PaddleActiveOptionKey),
        Visible(AtariEightBitSettingsConstants.PaddleMovementSpeedOptionKey),
        Visible(AtariEightBitSettingsConstants.DigitalSensitivityOptionKey),
        Visible(AtariEightBitSettingsConstants.AnalogSensitivityOptionKey),
        Managed(AtariEightBitSettingsConstants.AnalogDeadZoneOptionKey),
        Hidden(AtariEightBitSettingsConstants.KeyboardModeOptionKey),
        Hidden(AtariEightBitSettingsConstants.VirtualKeyboardOptionKey),
        Different(AtariEightBitSettingsConstants.XegsKeyboardOptionKey),
        Managed(Atari800MediaConstants.SystemOptionKey),
        Visible(AtariEightBitSettingsConstants.BasicEnabledOptionKey),
        Visible(AtariEightBitSettingsConstants.Os400800OptionKey),
        Different(AtariEightBitSettingsConstants.XlOsOptionKey),
        Different(AtariEightBitSettingsConstants.ConsoleOsOptionKey),
        Visible(AtariEightBitSettingsConstants.BasicVersionOptionKey),
        Visible(AtariEightBitSettingsConstants.MosaicMemoryOptionKey),
        Visible(AtariEightBitSettingsConstants.AxlonMemoryOptionKey),
        Visible(AtariEightBitSettingsConstants.AxlonShadowOptionKey),
        Different(AtariEightBitSettingsConstants.MapRamOptionKey),
        Visible(AtariEightBitSettingsConstants.AutofireOptionKey),
        Visible(AtariEightBitSettingsConstants.ShowSpeedOptionKey),
        Visible(AtariEightBitSettingsConstants.ShowActivityOptionKey),
        Visible(AtariEightBitSettingsConstants.ShowSectorOptionKey),
        Different(AtariEightBitSettingsConstants.Show1200XlLedsOptionKey),
        Hidden(AtariEightBitSettingsConstants.Xep80OptionKey),
        Visible(AtariEightBitSettingsConstants.RealTimeClockOptionKey),
        Visible(AtariEightBitSettingsConstants.PrinterDeviceOptionKey),
        Visible(AtariEightBitSettingsConstants.SerialDeviceOptionKey),
        Hidden(AtariEightBitSettingsConstants.SlowExecutableLoadingOptionKey),
        Visible(AtariEightBitSettingsConstants.SioAccelerationOptionKey),
        Visible(AtariEightBitSettingsConstants.CassetteBootOptionKey),
        Visible(AtariEightBitSettingsConstants.PokeyStereoOptionKey),
        Hidden(AtariEightBitSettingsConstants.LegacyConfigurationOptionKey)
    ];
    public static readonly IReadOnlyList<string> PaddleMovementSpeeds =
        Enumerable.Range(1, 9).Select(value => value.ToString()).ToArray();

    public static readonly IReadOnlyList<string> AutofireModes =
    [
        AtariEightBitSettingsConstants.Disabled,
        AtariEightBitSettingsConstants.AutofireOnButton,
        AtariEightBitSettingsConstants.AutofireAlways
    ];
    public static readonly IReadOnlyList<string> ToggleModes =
        [AtariEightBitSettingsConstants.Disabled, AtariEightBitSettingsConstants.Enabled];

    public static readonly IReadOnlyList<string> ControllerCompatibilityModes =
        [AtariEightBitSettingsConstants.None, AtariEightBitSettingsConstants.DualStick,
            AtariEightBitSettingsConstants.SwapPorts, AtariEightBitSettingsConstants.Joy2BPlus];

    public static readonly IReadOnlyList<string> Sensitivities =
        Enumerable.Range(1, 20).Select(value => (value * 5).ToString()).ToArray();

    public static readonly IReadOnlyList<string> ColorAdjustments = DecimalValues(-1.0m, 1.0m, 0.05m);
    public static readonly IReadOnlyList<string> ContrastAndBrightness = DecimalValues(-2.0m, 2.0m, 0.05m);
    public static readonly IReadOnlyList<string> GammaValues = DecimalValues(1.0m, 3.5m, 0.05m);
    public static readonly IReadOnlyList<string> ColorDelayValues =
        [AtariEightBitSettingsConstants.DefaultColorDelay, ..DecimalValues(10.0m, 50.0m, 0.5m)];
    public static readonly IReadOnlyList<string> ExternalPalettes =
        [AtariEightBitSettingsConstants.None, AtariEightBitSettingsCatalogConstants.Default, AtariEightBitSettingsCatalogConstants.Gray, AtariEightBitSettingsCatalogConstants.Jakub, AtariEightBitSettingsCatalogConstants.Real, AtariEightBitSettingsCatalogConstants.Xformer];
    public static readonly IReadOnlyList<string> OriginalComputerResolutions =
        [AtariEightBitSettingsCatalogConstants.Value336x240, AtariEightBitSettingsCatalogConstants.Value320x240, AtariEightBitSettingsCatalogConstants.Value384x240, AtariEightBitSettingsCatalogConstants.Value384x272, AtariEightBitSettingsCatalogConstants.Value384x288, AtariEightBitSettingsCatalogConstants.Value400x300];
    public static readonly IReadOnlyList<string> ArtifactingModes =
        [AtariEightBitSettingsConstants.None, AtariEightBitSettingsCatalogConstants.BlueBrown1, AtariEightBitSettingsCatalogConstants.BlueBrown2, AtariEightBitSettingsCatalogConstants.GTIA, AtariEightBitSettingsCatalogConstants.CTIA];
    public static readonly IReadOnlyList<string> BasicRevisions =
        [AtariEightBitSettingsCatalogConstants.Auto, AtariEightBitSettingsCatalogConstants.RevA, AtariEightBitSettingsCatalogConstants.RevB, AtariEightBitSettingsCatalogConstants.RevC, AtariEightBitSettingsCatalogConstants.AltirraBASIC];

    private static readonly IReadOnlyList<AtariMemoryExpansionChoice> MosaicChoices =
    [
        new(AtariEightBitSettingsConstants.Disabled, 0),
        new(AtariEightBitSettingsCatalogConstants.Value16KB, 16 * 1024),
        new(AtariEightBitSettingsCatalogConstants.Value80KB, 80 * 1024),
        new(AtariEightBitSettingsCatalogConstants.Value144KB, 144 * 1024)
    ];

    private static readonly IReadOnlyList<AtariMemoryExpansionChoice> AxlonChoices =
    [
        new(AtariEightBitSettingsConstants.Disabled, 0),
        new(AtariEightBitSettingsCatalogConstants.Value128KB, 128 * 1024),
        new(AtariEightBitSettingsCatalogConstants.Value256KB, 256 * 1024),
        new(AtariEightBitSettingsCatalogConstants.Value512KB, 512 * 1024),
        new(AtariEightBitSettingsCatalogConstants.Value1MB, 1024 * 1024),
        new(AtariEightBitSettingsCatalogConstants.Value2MB, 2 * 1024 * 1024),
        new(AtariEightBitSettingsCatalogConstants.Value4MB, 4 * 1024 * 1024)
    ];

    public static bool SupportsOriginalComputerOptions(AtariMachineModel model) =>
        model is AtariMachineModel.Atari400 or AtariMachineModel.Atari800;

    public static bool SupportsComputerOptions(AtariMachineModel model) => model is
        AtariMachineModel.Atari400 or AtariMachineModel.Atari800 or AtariMachineModel.Atari800Xl or
        AtariMachineModel.Atari130Xe or AtariMachineModel.Xegs or AtariMachineModel.XlXe;

    public static bool SupportsMapRam(AtariMachineModel model) => model is
        AtariMachineModel.Atari800Xl or AtariMachineModel.Atari130Xe or
        AtariMachineModel.Xegs or AtariMachineModel.XlXe;

    public static IReadOnlyList<AtariMemoryExpansionChoice> Mosaic(AtariMachineModel model) =>
        SupportsOriginalComputerOptions(model) ? MosaicChoices : [];

    public static IReadOnlyList<AtariMemoryExpansionChoice> Axlon(AtariMachineModel model) =>
        SupportsOriginalComputerOptions(model) ? AxlonChoices : [];

    public static long CpuFrequency(AtariClassicRegion region) => region switch
    {
        AtariClassicRegion.Pal => AtariEightBitSettingsConstants.PalCpuFrequencyHz,
        AtariClassicRegion.Ntsc => AtariEightBitSettingsConstants.NtscCpuFrequencyHz,
        _ => throw new ArgumentOutOfRangeException(nameof(region), region, null)
    };

    public static IReadOnlyList<string> OriginalOsRevisions(AtariClassicRegion region) => region switch
    {
        AtariClassicRegion.Pal => [AtariEightBitSettingsCatalogConstants.Auto, AtariEightBitSettingsCatalogConstants.RevAPAL, AtariEightBitSettingsCatalogConstants.RevBNTSC, AtariEightBitSettingsCatalogConstants.AltirraOS],
        AtariClassicRegion.Ntsc => [AtariEightBitSettingsCatalogConstants.Auto, AtariEightBitSettingsCatalogConstants.RevANTSC, AtariEightBitSettingsCatalogConstants.RevBNTSC, AtariEightBitSettingsCatalogConstants.AltirraOS],
        _ => [AtariEightBitSettingsCatalogConstants.Auto, AtariEightBitSettingsCatalogConstants.AltirraOS]
    };

    public static bool IsOriginalOsCompatible(AtariFirmwareDefinition definition, AtariClassicRegion region) =>
        definition.Category switch
        {
            AtariFirmwareCategory.AtariOsA when definition.Version == AtariEightBitSettingsCatalogConstants.RevAPAL =>
                region == AtariClassicRegion.Pal,
            AtariFirmwareCategory.AtariOsA when definition.Version == AtariEightBitSettingsCatalogConstants.RevANTSC =>
                region == AtariClassicRegion.Ntsc,
            AtariFirmwareCategory.AtariOsB => true,
            _ => true
        };

    private static AtariEightBitNativeSetting Visible(string key) =>
        new(key, AtariEightBitSettingDisposition.UserVisible);
    private static AtariEightBitNativeSetting Managed(string key) =>
        new(key, AtariEightBitSettingDisposition.ManagedByApplication);
    private static AtariEightBitNativeSetting Hidden(string key) =>
        new(key, AtariEightBitSettingDisposition.HiddenInternal);
    private static AtariEightBitNativeSetting Different(string key) =>
        new(key, AtariEightBitSettingDisposition.DifferentModel);

    private static IReadOnlyList<string> DecimalValues(decimal minimum, decimal maximum, decimal step)
    {
        var count = decimal.ToInt32((maximum - minimum) / step) + 1;
        return Enumerable.Range(0, count).Select(index => (minimum + index * step).ToString(AtariEightBitSettingsCatalogConstants.Value000,
            System.Globalization.CultureInfo.InvariantCulture)).ToArray();
    }
}
