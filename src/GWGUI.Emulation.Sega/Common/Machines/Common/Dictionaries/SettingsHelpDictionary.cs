namespace GWGUI.Emulation.Sega.Common.Machines.Common.Dictionaries;

internal static class SettingsHelpDictionary
{
    internal static readonly IReadOnlyDictionary<string, string> Resources =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SettingsConstants.Model] = "Emulation.Sega.Help.General.Model",
            [SettingsConstants.Emulator] = "Emulation.Sega.Help.General.Emulator",
            [SettingsConstants.MasterSystemVariant] = "Emulation.Sega.Help.MasterSystem.Variant",
            [SettingsConstants.MasterSystemThreeDGlasses] = "Emulation.Sega.Help.MasterSystem.ThreeDGlasses",
            [SettingsConstants.MegaDriveModel] = "Emulation.Sega.Help.MegaDrive.Model",
            [SettingsConstants.MegaCdModel] = "Emulation.Sega.Help.MegaDrive.MegaCd",
            [SettingsConstants.MegaCdEnabled] = "Emulation.Sega.Help.MegaDrive.MegaCd.Enabled",
            [SettingsConstants.MegaDriveThirtyTwoX] = "Emulation.Sega.Help.MegaDrive.32X",
            [SettingsConstants.MegaDriveRegion] = "Emulation.Sega.Help.MegaDrive.Region",
            [SettingsConstants.MegaDriveVideoStandard] = "Emulation.Sega.Help.MegaDrive.VideoStandard",
            [SettingsConstants.Model + ".cpu"] = "Emulation.Sega.Help.Cpu.Model",
            [SettingsConstants.Model + ".frequency"] = "Emulation.Sega.Help.Cpu.Frequency",
            [SettingsConstants.Ram] = "Emulation.Sega.Help.Memory.Ram",
            [SettingsConstants.FirmwareIntegrated] = "Emulation.Sega.Help.Firmware.Integrated",
            [SettingsConstants.VideoResolution] = "Emulation.Sega.Help.Video.Resolution",
            [SettingsConstants.VideoMonitor] = "Emulation.Sega.Help.Video.Monitor",
            [SettingsConstants.VideoIntensity] = "Emulation.Sega.Help.Video.Intensity",
            [SettingsConstants.VideoCrop] = "Emulation.Sega.Help.Video.Crop",
            [SettingsConstants.AudioEnabled] = "Emulation.Sega.Help.Audio.Enabled",
            [SettingsConstants.AudioOutput] = "Emulation.Sega.Help.Audio.Output",
            [SettingsConstants.AudioLatency] = "Emulation.Help.Audio.Latency",
            [SettingsConstants.FloppySound] = "Emulation.Sega.Help.Audio.FloppySound"
        };
}
