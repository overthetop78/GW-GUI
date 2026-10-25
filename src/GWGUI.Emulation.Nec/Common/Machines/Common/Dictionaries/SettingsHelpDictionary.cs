namespace GWGUI.Emulation.Nec.Common.Machines.Common.Dictionaries;

internal static class SettingsHelpDictionary
{
    internal static readonly IReadOnlyDictionary<string, string> Resources =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SettingsConstants.Model] = SettingsHelpResourceConstants.Emulation_Nec_Help_General_Model,
            [SettingsConstants.Emulator] = SettingsHelpResourceConstants.Emulation_Nec_Help_General_Emulator,
            [SettingsConstants.Model + SettingsDescriptionFunctionsConstants.ModelCpuSuffix] = SettingsHelpResourceConstants.Emulation_Nec_Help_Cpu_Model,
            [SettingsConstants.Model + SettingsDescriptionFunctionsConstants.ModelFrequencySuffix] = SettingsHelpResourceConstants.Emulation_Nec_Help_Cpu_Frequency,
            [SettingsConstants.Ram] = SettingsHelpResourceConstants.Emulation_Nec_Help_Memory_Fixed,
            [SettingsConstants.FirmwareIntegrated] = SettingsHelpResourceConstants.Emulation_Nec_Help_Firmware_Integrated,
            [SettingsConstants.FirmwarePath] = SettingsHelpResourceConstants.Emulation_Nec_Help_Firmware_Path,
            [SettingsConstants.PcePalette] = SettingsHelpResourceConstants.Emulation_Nec_Help_Video_Palette,
            [SettingsConstants.PceHorizontalOverscan] = SettingsHelpResourceConstants.Emulation_Nec_Help_Video_HorizontalWidth,
            [SettingsConstants.PceFrameSkip] = SettingsHelpResourceConstants.Emulation_Nec_Help_Video_FrameSkip,
            [SettingsConstants.PceFrameSkipThreshold] = SettingsHelpResourceConstants.Emulation_Nec_Help_Video_FrameSkipThreshold,
            [SettingsConstants.PceInitialScanline] = SettingsHelpResourceConstants.Emulation_Nec_Help_Video_InitialScanline,
            [SettingsConstants.PceLastScanline] = SettingsHelpResourceConstants.Emulation_Nec_Help_Video_LastScanline,
            [SettingsConstants.PceNoSpriteLimit] = SettingsHelpResourceConstants.Emulation_Nec_Help_Video_NoSpriteLimit,
            [SettingsConstants.PcfxWidth] = SettingsHelpResourceConstants.Emulation_Nec_Help_Video_DotClockWidth,
            [SettingsConstants.PcfxChromaInterpolation] = SettingsHelpResourceConstants.Emulation_Nec_Help_Video_ChromaInterpolation,
            [SettingsConstants.PcfxNoSpriteLimit] = SettingsHelpResourceConstants.Emulation_Nec_Help_Video_NoSpriteLimit,
            [SettingsConstants.PcfxInitialScanline] = SettingsHelpResourceConstants.Emulation_Nec_Help_Video_InitialScanline,
            [SettingsConstants.PcfxLastScanline] = SettingsHelpResourceConstants.Emulation_Nec_Help_Video_LastScanline,
            [SettingsConstants.PceCdVolume] = SettingsHelpResourceConstants.Emulation_Nec_Help_Audio_CdVolume,
            [SettingsConstants.PceAdpcmVolume] = SettingsHelpResourceConstants.Emulation_Nec_Help_Audio_AdpcmVolume,
            [SettingsConstants.PceCdPsgVolume] = SettingsHelpResourceConstants.Emulation_Nec_Help_Audio_CdPsgVolume,
            [SettingsConstants.PceAdpcmLowPass] = SettingsHelpResourceConstants.Emulation_Nec_Help_Audio_AdpcmLowPass,
            [SettingsConstants.PcfxSoundQuality] = SettingsHelpResourceConstants.Emulation_Nec_Help_Audio_SoundQuality,
            [SettingsConstants.PcfxSuppressClicks] = SettingsHelpResourceConstants.Emulation_Nec_Help_Audio_SuppressClicks,
            [SettingsConstants.PcfxBuggyCodec] = SettingsHelpResourceConstants.Emulation_Nec_Help_Audio_BuggyCodec,
            [SettingsConstants.PcfxMouseSensitivity] = SettingsHelpResourceConstants.Emulation_Nec_Help_Mouse_Sensitivity,
            [SettingsConstants.PceMouseSensitivity] = SettingsHelpResourceConstants.Emulation_Nec_Help_Mouse_Sensitivity,
            [SettingsConstants.VideoResolution] = SettingsHelpResourceConstants.Emulation_Nec_Help_Video_Resolution,
            [SettingsConstants.VideoMonitor] = SettingsHelpResourceConstants.Emulation_Nec_Help_Video_Monitor,
            [SettingsConstants.VideoIntensity] = SettingsHelpResourceConstants.Emulation_Nec_Help_Video_Intensity,
            [SettingsConstants.VideoCrop] = SettingsHelpResourceConstants.Emulation_Nec_Help_Video_Crop,
            [SettingsConstants.AudioEnabled] = SettingsHelpResourceConstants.Emulation_Nec_Help_Audio_Enabled,
            [SettingsConstants.AudioOutput] = SettingsHelpResourceConstants.Emulation_Nec_Help_Audio_Output,
            [SettingsConstants.AudioLatency] = SettingsHelpResourceConstants.Emulation_Help_Audio_Latency,
            [SettingsConstants.FloppySound] = SettingsHelpResourceConstants.Emulation_Nec_Help_Audio_FloppySound
        };
}
