namespace GWGUI.Emulation.Nec.Common.Machines.Common.Dictionaries;

internal static class SettingsHelpDictionary
{
    internal static readonly IReadOnlyDictionary<string, string> Resources =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SettingsConstants.Model] = "Emulation.Nec.Help.General.Model",
            [SettingsConstants.Emulator] = "Emulation.Nec.Help.General.Emulator",
            [SettingsConstants.Model + ".cpu"] = "Emulation.Nec.Help.Cpu.Model",
            [SettingsConstants.Model + ".frequency"] = "Emulation.Nec.Help.Cpu.Frequency",
            [SettingsConstants.Ram] = "Emulation.Nec.Help.Memory.Ram",
            [SettingsConstants.FirmwareIntegrated] = "Emulation.Nec.Help.Firmware.Integrated",
            [SettingsConstants.VideoResolution] = "Emulation.Nec.Help.Video.Resolution",
            [SettingsConstants.VideoMonitor] = "Emulation.Nec.Help.Video.Monitor",
            [SettingsConstants.VideoIntensity] = "Emulation.Nec.Help.Video.Intensity",
            [SettingsConstants.VideoCrop] = "Emulation.Nec.Help.Video.Crop",
            [SettingsConstants.AudioEnabled] = "Emulation.Nec.Help.Audio.Enabled",
            [SettingsConstants.AudioOutput] = "Emulation.Nec.Help.Audio.Output",
            [SettingsConstants.AudioLatency] = "Emulation.Help.Audio.Latency",
            [SettingsConstants.FloppySound] = "Emulation.Nec.Help.Audio.FloppySound"
        };
}
