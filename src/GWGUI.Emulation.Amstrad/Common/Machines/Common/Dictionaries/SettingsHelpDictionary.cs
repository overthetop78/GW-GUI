namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Dictionaries;

internal static class SettingsHelpDictionary
{
    internal static readonly IReadOnlyDictionary<string, string> Resources =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SettingsConstants.Model] = SettingsHelpDictionaryConstants.GeneralModelHelpResourceKey,
            [SettingsConstants.Emulator] = SettingsHelpDictionaryConstants.GeneralEmulatorHelpResourceKey,
            [SettingsConstants.Model + SettingsHelpDictionaryConstants.CpuSettingSuffix] = SettingsHelpDictionaryConstants.CpuModelHelpResourceKey,
            [SettingsConstants.Model + SettingsHelpDictionaryConstants.CpuFrequencySettingSuffix] = SettingsHelpDictionaryConstants.CpuFrequencyHelpResourceKey,
            [SettingsConstants.Ram] = SettingsHelpDictionaryConstants.MemoryRamHelpResourceKey,
            [SettingsConstants.FirmwareIntegrated] = SettingsHelpDictionaryConstants.FirmwareIntegratedHelpResourceKey,
            [SettingsConstants.VideoResolution] = SettingsHelpDictionaryConstants.VideoResolutionHelpResourceKey,
            [SettingsConstants.VideoMonitor] = SettingsHelpDictionaryConstants.VideoMonitorHelpResourceKey,
            [SettingsConstants.VideoIntensity] = SettingsHelpDictionaryConstants.VideoIntensityHelpResourceKey,
            [SettingsConstants.VideoCrop] = SettingsHelpDictionaryConstants.VideoCropHelpResourceKey,
            [SettingsConstants.AudioEnabled] = SettingsHelpDictionaryConstants.AudioEnabledHelpResourceKey,
            [SettingsConstants.AudioOutput] = SettingsHelpDictionaryConstants.AudioOutputHelpResourceKey,
            [SettingsConstants.AudioLatency] = SettingsHelpDictionaryConstants.AudioLatencyHelpResourceKey,
            [SettingsConstants.FloppySound] = SettingsHelpDictionaryConstants.AudioFloppySoundHelpResourceKey
        };
}
