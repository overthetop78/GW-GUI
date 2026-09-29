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
            [SettingsConstants.AudioEnabled] = "Emulation.Microsoft.Help.Audio.Enabled",
            [SettingsConstants.AudioOutput] = "Emulation.Microsoft.Help.Audio.Output",
            [SettingsConstants.AudioLatency] = "Emulation.Help.Audio.Latency",
            [SettingsConstants.Model + ".video"] = "Emulation.Microsoft.Help.Video.Model",
            [SettingsConstants.Model + ".audio"] = "Emulation.Microsoft.Help.Audio.Model"
        };
}
