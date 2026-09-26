namespace GWGUI.Emulation.Atari.Common.Machines.Atari8Bit.Dictionaries;




public static class EightBitSettingsCatalog
{
    public static readonly IReadOnlyList<EightBitNativeSetting> NativeSettings =
    [
        Visible(EightBitSettingsConstants.VideoStandardOptionKey),
        Visible(EightBitSettingsConstants.ArtifactingModeOptionKey),
        Visible(EightBitSettingsConstants.ResolutionOptionKey),
        Visible(EightBitSettingsConstants.ColorHueOptionKey),
        Visible(EightBitSettingsConstants.ColorSaturationOptionKey),
        Visible(EightBitSettingsConstants.ColorContrastOptionKey),
        Visible(EightBitSettingsConstants.ColorBrightnessOptionKey),
        Visible(EightBitSettingsConstants.ColorGammaOptionKey),
        Visible(EightBitSettingsConstants.ColorDelayOptionKey),
        Visible(EightBitSettingsConstants.ExternalPaletteOptionKey),
        Visible(EightBitSettingsConstants.ControllerCompatibilityOptionKey),
        Visible(EightBitSettingsConstants.PaddleActiveOptionKey),
        Visible(EightBitSettingsConstants.PaddleMovementSpeedOptionKey),
        Visible(EightBitSettingsConstants.DigitalSensitivityOptionKey),
        Visible(EightBitSettingsConstants.AnalogSensitivityOptionKey),
        Managed(EightBitSettingsConstants.AnalogDeadZoneOptionKey),
        Hidden(EightBitSettingsConstants.KeyboardModeOptionKey),
        Hidden(EightBitSettingsConstants.VirtualKeyboardOptionKey),
        Different(EightBitSettingsConstants.XegsKeyboardOptionKey),
        Managed(EightBitSettingsConstants.SystemOptionKey),
        Visible(EightBitSettingsConstants.BasicEnabledOptionKey),
        Visible(EightBitSettingsConstants.Os400800OptionKey),
        Different(EightBitSettingsConstants.XlOsOptionKey),
        Different(EightBitSettingsConstants.ConsoleOsOptionKey),
        Visible(EightBitSettingsConstants.BasicVersionOptionKey),
        Visible(EightBitSettingsConstants.MosaicMemoryOptionKey),
        Visible(EightBitSettingsConstants.AxlonMemoryOptionKey),
        Visible(EightBitSettingsConstants.AxlonShadowOptionKey),
        Different(EightBitSettingsConstants.MapRamOptionKey),
        Visible(EightBitSettingsConstants.AutofireOptionKey),
        Visible(EightBitSettingsConstants.ShowSpeedOptionKey),
        Visible(EightBitSettingsConstants.ShowActivityOptionKey),
        Visible(EightBitSettingsConstants.ShowSectorOptionKey),
        Different(EightBitSettingsConstants.Show1200XlLedsOptionKey),
        Hidden(EightBitSettingsConstants.Xep80OptionKey),
        Visible(EightBitSettingsConstants.RealTimeClockOptionKey),
        Visible(EightBitSettingsConstants.PrinterDeviceOptionKey),
        Visible(EightBitSettingsConstants.SerialDeviceOptionKey),
        Hidden(EightBitSettingsConstants.SlowExecutableLoadingOptionKey),
        Managed(EightBitSettingsConstants.SioAccelerationOptionKey),
        Visible(EightBitSettingsConstants.CassetteBootOptionKey),
        Visible(EightBitSettingsConstants.PokeyStereoOptionKey),
        Hidden(EightBitSettingsConstants.LegacyConfigurationOptionKey)
    ];
    public static readonly IReadOnlyList<string> PaddleMovementSpeeds =
        Enumerable.Range(1, 9).Select(value => value.ToString()).ToArray();

    public static readonly IReadOnlyList<string> AutofireModes =
    [
        EightBitSettingsConstants.Disabled,
        EightBitSettingsConstants.AutofireOnButton,
        EightBitSettingsConstants.AutofireAlways
    ];
    public static readonly IReadOnlyList<string> ToggleModes =
        [EightBitSettingsConstants.Disabled, EightBitSettingsConstants.Enabled];

    public static readonly IReadOnlyList<string> ControllerCompatibilityModes =
        [EightBitSettingsConstants.None, EightBitSettingsConstants.DualStick,
            EightBitSettingsConstants.SwapPorts, EightBitSettingsConstants.Joy2BPlus];

    public static readonly IReadOnlyList<string> Sensitivities =
        Enumerable.Range(1, 20).Select(value => (value * 5).ToString()).ToArray();

    public static readonly IReadOnlyList<string> ColorAdjustments = DecimalValues(-1.0m, 1.0m, 0.05m);
    public static readonly IReadOnlyList<string> ContrastAndBrightness = DecimalValues(-2.0m, 2.0m, 0.05m);
    public static readonly IReadOnlyList<string> GammaValues = DecimalValues(1.0m, 3.5m, 0.05m);
    public static readonly IReadOnlyList<string> ColorDelayValues =
        [EightBitSettingsConstants.DefaultColorDelay, ..DecimalValues(10.0m, 50.0m, 0.5m)];
    public static readonly IReadOnlyList<string> ExternalPalettes =
        [EightBitSettingsConstants.None, EightBitSettingsCatalogConstants.Default, EightBitSettingsCatalogConstants.Gray, EightBitSettingsCatalogConstants.Jakub, EightBitSettingsCatalogConstants.Real, EightBitSettingsCatalogConstants.Xformer];
    public static readonly IReadOnlyList<string> OriginalComputerResolutions =
        [EightBitSettingsCatalogConstants.Value336x240, EightBitSettingsCatalogConstants.Value320x240, EightBitSettingsCatalogConstants.Value384x240, EightBitSettingsCatalogConstants.Value384x272, EightBitSettingsCatalogConstants.Value384x288, EightBitSettingsCatalogConstants.Value400x300];
    public static readonly IReadOnlyList<string> ArtifactingModes =
        [EightBitSettingsConstants.None, EightBitSettingsCatalogConstants.BlueBrown1, EightBitSettingsCatalogConstants.BlueBrown2, EightBitSettingsCatalogConstants.GTIA, EightBitSettingsCatalogConstants.CTIA];
    public static readonly IReadOnlyList<string> BasicRevisions =
        [EightBitSettingsCatalogConstants.Auto, EightBitSettingsCatalogConstants.RevA, EightBitSettingsCatalogConstants.RevB, EightBitSettingsCatalogConstants.RevC, EightBitSettingsCatalogConstants.AltirraBASIC];

    private static readonly IReadOnlyList<MemoryExpansionChoice> MosaicChoices =
    [
        new(EightBitSettingsConstants.Disabled, 0),
        new(EightBitSettingsCatalogConstants.Value16KB, 16 * 1024),
        new(EightBitSettingsCatalogConstants.Value80KB, 80 * 1024),
        new(EightBitSettingsCatalogConstants.Value144KB, 144 * 1024)
    ];

    private static readonly IReadOnlyList<MemoryExpansionChoice> AxlonChoices =
    [
        new(EightBitSettingsConstants.Disabled, 0),
        new(EightBitSettingsCatalogConstants.Value128KB, 128 * 1024),
        new(EightBitSettingsCatalogConstants.Value256KB, 256 * 1024),
        new(EightBitSettingsCatalogConstants.Value512KB, 512 * 1024),
        new(EightBitSettingsCatalogConstants.Value1MB, 1024 * 1024),
        new(EightBitSettingsCatalogConstants.Value2MB, 2 * 1024 * 1024),
        new(EightBitSettingsCatalogConstants.Value4MB, 4 * 1024 * 1024)
    ];

    public static bool SupportsOriginalComputerOptions(MachineModel model) =>
        model is MachineModel.Atari400 or MachineModel.Atari800;

    public static bool SupportsComputerOptions(MachineModel model) => model is
        MachineModel.Atari400 or MachineModel.Atari800 or MachineModel.Atari800Xl or
        MachineModel.Atari130Xe or MachineModel.Xegs or MachineModel.XlXe;

    public static bool SupportsMapRam(MachineModel model) => model is
        MachineModel.Atari800Xl or MachineModel.Atari130Xe or
        MachineModel.Xegs or MachineModel.XlXe;

    public static IReadOnlyList<MemoryExpansionChoice> Mosaic(MachineModel model) =>
        SupportsOriginalComputerOptions(model) ? MosaicChoices : [];

    public static IReadOnlyList<MemoryExpansionChoice> Axlon(MachineModel model) =>
        SupportsOriginalComputerOptions(model) ? AxlonChoices : [];

    public static long CpuFrequency(HardwareRegion region) => region switch
    {
        HardwareRegion.Pal => EightBitSettingsConstants.PalCpuFrequencyHz,
        HardwareRegion.Ntsc => EightBitSettingsConstants.NtscCpuFrequencyHz,
        _ => throw new ArgumentOutOfRangeException(nameof(region), region, null)
    };

    public static IReadOnlyList<string> OriginalOsRevisions(HardwareRegion region) => region switch
    {
        HardwareRegion.Pal => [EightBitSettingsCatalogConstants.Auto, EightBitSettingsCatalogConstants.RevAPAL, EightBitSettingsCatalogConstants.RevBNTSC, EightBitSettingsCatalogConstants.AltirraOS],
        HardwareRegion.Ntsc => [EightBitSettingsCatalogConstants.Auto, EightBitSettingsCatalogConstants.RevANTSC, EightBitSettingsCatalogConstants.RevBNTSC, EightBitSettingsCatalogConstants.AltirraOS],
        _ => [EightBitSettingsCatalogConstants.Auto, EightBitSettingsCatalogConstants.AltirraOS]
    };

    public static bool IsOriginalOsCompatible(FirmwareDefinition definition, HardwareRegion region) =>
        definition.Category switch
        {
            FirmwareCategory.AtariOsA when definition.Version == EightBitSettingsCatalogConstants.RevAPAL =>
                region == HardwareRegion.Pal,
            FirmwareCategory.AtariOsA when definition.Version == EightBitSettingsCatalogConstants.RevANTSC =>
                region == HardwareRegion.Ntsc,
            FirmwareCategory.AtariOsB => true,
            _ => true
        };

    private static EightBitNativeSetting Visible(string key) =>
        new(key, EightBitSettingDisposition.UserVisible);
    private static EightBitNativeSetting Managed(string key) =>
        new(key, EightBitSettingDisposition.ManagedByApplication);
    private static EightBitNativeSetting Hidden(string key) =>
        new(key, EightBitSettingDisposition.HiddenInternal);
    private static EightBitNativeSetting Different(string key) =>
        new(key, EightBitSettingDisposition.DifferentModel);

    private static IReadOnlyList<string> DecimalValues(decimal minimum, decimal maximum, decimal step)
    {
        var count = decimal.ToInt32((maximum - minimum) / step) + 1;
        return Enumerable.Range(0, count).Select(index => (minimum + index * step).ToString(EightBitSettingsCatalogConstants.Value000,
            System.Globalization.CultureInfo.InvariantCulture)).ToArray();
    }
}
