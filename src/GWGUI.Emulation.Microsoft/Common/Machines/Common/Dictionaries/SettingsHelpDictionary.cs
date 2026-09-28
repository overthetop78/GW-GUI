namespace GWGUI.Emulation.Microsoft.Common.Machines.Common.Dictionaries;

internal static class SettingsHelpDictionary
{
    internal static readonly IReadOnlyDictionary<string, string> Resources =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SettingsConstants.Model] = "Emulation.Microsoft.Help.General.Model",
            [SettingsConstants.Emulator] = "Emulation.Microsoft.Help.General.Emulator",
            [SettingsConstants.Model + ".cpu"] = "Emulation.Microsoft.Help.Cpu.Model",
            [SettingsConstants.Model + ".frequency"] = "Emulation.Microsoft.Help.Cpu.Frequency",
            [SettingsConstants.Ram] = "Emulation.Microsoft.Help.Memory.Ram",
            [SettingsConstants.FirmwareIntegrated] = "Emulation.Microsoft.Help.Firmware.Integrated",
            [SettingsConstants.VideoResolution] = "Emulation.Microsoft.Help.Video.Resolution",
            [SettingsConstants.VideoMonitor] = "Emulation.Microsoft.Help.Video.Monitor",
            [SettingsConstants.VideoIntensity] = "Emulation.Microsoft.Help.Video.Intensity",
            [SettingsConstants.VideoCrop] = "Emulation.Microsoft.Help.Video.Crop",
            [SettingsConstants.AudioEnabled] = "Emulation.Microsoft.Help.Audio.Enabled",
            [SettingsConstants.AudioOutput] = "Emulation.Microsoft.Help.Audio.Output",
            [SettingsConstants.AudioLatency] = "Emulation.Help.Audio.Latency",
            [SettingsConstants.FloppySound] = "Emulation.Microsoft.Help.Audio.FloppySound"
        };
}
