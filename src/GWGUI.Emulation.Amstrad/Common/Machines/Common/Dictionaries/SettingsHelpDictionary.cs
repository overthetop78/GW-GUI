namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Dictionaries;

internal static class SettingsHelpDictionary
{
    internal static readonly IReadOnlyDictionary<string, string> Resources =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SettingsConstants.Model] = "Emulation.Amstrad.Help.General.Model",
            [SettingsConstants.Emulator] = "Emulation.Amstrad.Help.General.Emulator",
            [SettingsConstants.Model + ".cpu"] = "Emulation.Amstrad.Help.Cpu.Model",
            [SettingsConstants.Model + ".frequency"] = "Emulation.Amstrad.Help.Cpu.Frequency",
            [SettingsConstants.Ram] = "Emulation.Amstrad.Help.Memory.Ram",
            [SettingsConstants.FirmwareIntegrated] = "Emulation.Amstrad.Help.Firmware.Integrated",
            [SettingsConstants.VideoResolution] = "Emulation.Amstrad.Help.Video.Resolution",
            [SettingsConstants.VideoMonitor] = "Emulation.Amstrad.Help.Video.Monitor",
            [SettingsConstants.VideoIntensity] = "Emulation.Amstrad.Help.Video.Intensity",
            [SettingsConstants.VideoCrop] = "Emulation.Amstrad.Help.Video.Crop",
            [SettingsConstants.AudioEnabled] = "Emulation.Amstrad.Help.Audio.Enabled",
            [SettingsConstants.AudioOutput] = "Emulation.Amstrad.Help.Audio.Output",
            [SettingsConstants.AudioLatency] = "Emulation.Help.Audio.Latency",
            [SettingsConstants.FloppySound] = "Emulation.Amstrad.Help.Audio.FloppySound"
        };
}
