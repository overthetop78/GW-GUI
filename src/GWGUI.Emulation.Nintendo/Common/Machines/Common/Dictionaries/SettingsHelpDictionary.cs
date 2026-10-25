namespace GWGUI.Emulation.Nintendo.Common.Machines.Common.Dictionaries;

internal static class SettingsHelpDictionary
{
    internal static readonly IReadOnlyDictionary<string, string> Resources =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SettingsConstants.Model] = "Emulation.Nintendo.Help.General.Model",
            [SettingsConstants.Emulator] = "Emulation.Nintendo.Help.General.Emulator",
            [SettingsConstants.Model + ".cpu"] = "Emulation.Nintendo.Help.Cpu.Model",
            [SettingsConstants.Model + ".frequency"] = "Emulation.Nintendo.Help.Cpu.Frequency",
            [SettingsConstants.Ram] = "Emulation.Nintendo.Help.Memory.Ram",
            [SettingsConstants.FirmwareIntegrated] = "Emulation.Nintendo.Help.Firmware.Integrated",
            [SettingsConstants.AudioEnabled] = "Emulation.Nintendo.Help.Audio.Enabled",
            [SettingsConstants.AudioOutput] = "Emulation.Nintendo.Help.Audio.Output",
            [SettingsConstants.AudioLatency] = "Emulation.Help.Audio.Latency"
        };
}
