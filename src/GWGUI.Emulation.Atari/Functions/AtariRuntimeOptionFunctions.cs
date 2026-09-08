namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariRuntimeOptionFunctions
{
    private static readonly IReadOnlySet<string> RestartRequired = new HashSet<string>(StringComparer.Ordinal)
    {
        Atari800MediaConstants.SystemOptionKey,
        AtariEightBitSettingsConstants.BasicEnabledOptionKey,
        AtariEightBitSettingsConstants.Os400800OptionKey,
        AtariEightBitSettingsConstants.XlOsOptionKey,
        AtariEightBitSettingsConstants.ConsoleOsOptionKey,
        AtariEightBitSettingsConstants.BasicVersionOptionKey,
        AtariEightBitSettingsConstants.MosaicMemoryOptionKey,
        AtariEightBitSettingsConstants.AxlonMemoryOptionKey,
        AtariEightBitSettingsConstants.AxlonShadowOptionKey,
        AtariEightBitSettingsConstants.MapRamOptionKey,
        AtariEightBitSettingsConstants.Xep80OptionKey,
        AtariEightBitSettingsConstants.RealTimeClockOptionKey,
        AtariEightBitSettingsConstants.PrinterDeviceOptionKey,
        AtariEightBitSettingsConstants.SerialDeviceOptionKey,
        AtariEightBitSettingsConstants.CassetteBootOptionKey,
        AtariEightBitSettingsConstants.PokeyStereoOptionKey
    };

    internal static bool RequiresRestart(AtariEmulator emulator, string key) =>
        emulator == AtariEmulator.Atari800 && RestartRequired.Contains(key);
}
