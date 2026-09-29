namespace GWGUI.Emulation.Sony.Common.Machines.Common.Dictionaries;

internal static class SettingsHelpDictionary
{
    internal static readonly IReadOnlyDictionary<string, string> Resources =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SettingsConstants.Model] = "Emulation.Sony.Help.General.Model",
            [SettingsConstants.Emulator] = "Emulation.Sony.Help.General.Emulator",
            [SettingsConstants.Model + ".cpu"] = "Emulation.Sony.Help.Cpu.Model",
            [SettingsConstants.Model + ".frequency"] = "Emulation.Sony.Help.Cpu.Frequency",
            [SettingsConstants.Ram] = "Emulation.Sony.Help.Memory.Ram",
            [SettingsConstants.FirmwareIntegrated] = "Emulation.Sony.Help.Firmware.Integrated",
            [SettingsConstants.AudioEnabled] = "Emulation.Sony.Help.Audio.Enabled",
            [SettingsConstants.AudioOutput] = "Emulation.Sony.Help.Audio.Output",
            [SettingsConstants.AudioLatency] = "Emulation.Help.Audio.Latency"
        };
}
