namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Dictionaries;

internal static class AmigaSettingsHelpDictionary
{
    internal static readonly IReadOnlyDictionary<string, string> Resources =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SettingsConstants.OptionCpuModel] = SettingsHelpResourceKeys.CpuModel,
            [SettingsConstants.OptionFpuModel] = SettingsHelpResourceKeys.CpuFpuModel,
            [SettingsConstants.OptionCpuCompatibility] = SettingsHelpResourceKeys.CpuPrecision,
            [SettingsConstants.CpuOriginalSpeed] = SettingsHelpResourceKeys.CpuOriginalSpeed,
            [SettingsConstants.CpuSpeed] = SettingsHelpResourceKeys.CpuSpeed,
            [SettingsConstants.OptionChipmemSize] = SettingsHelpResourceKeys.MemoryMain,
            [SettingsConstants.OptionBogomemSize] = SettingsHelpResourceKeys.MemorySlow,
            [SettingsConstants.OptionFastmemSize] = SettingsHelpResourceKeys.MemoryFast,
            [SettingsConstants.OptionZ3memSize] = SettingsHelpResourceKeys.MemoryZ3,
            [SettingsConstants.KickstartPath] = SettingsHelpResourceKeys.FirmwareSystem,
            [SettingsConstants.ExtendedRomPath] = SettingsHelpResourceKeys.FirmwareExtendedRom,
            [SettingsConstants.RomKeyPath] = SettingsHelpResourceKeys.FirmwareRomKey,
            [SettingsConstants.OptionVideoStandard] = SettingsHelpResourceKeys.VideoStandard,
            [SettingsConstants.OptionVideoResolution] = SettingsHelpResourceKeys.VideoResolution,
            [SettingsConstants.OptionVideoAspect] = SettingsHelpResourceKeys.VideoAspectRatio,
            [SettingsConstants.OptionCrop] = SettingsHelpResourceKeys.VideoCrop,
            [SettingsConstants.OptionVideoVresolution] = SettingsHelpResourceKeys.VideoLineMode,
            [SettingsConstants.OptionVideoAllowHzChange] = SettingsHelpResourceKeys.VideoHzChange,
            [SettingsConstants.OptionGfxFramerate] = SettingsHelpResourceKeys.VideoFrameSkip,
            [SettingsConstants.OptionGfxColors] = SettingsHelpResourceKeys.VideoColors,
            [SettingsConstants.OptionGfxGamma] = SettingsHelpResourceKeys.VideoGamma,
            [SettingsConstants.OptionImmediateBlits] = SettingsHelpResourceKeys.VideoImmediateBlits,
            [SettingsConstants.OptionCollisionLevel] = SettingsHelpResourceKeys.VideoCollisionLevel,
            [SettingsConstants.OptionGfxFlickerfixer] = SettingsHelpResourceKeys.VideoFlickerFixer,
            [MachineSettingsConstants.AudioLatency] = SettingsHelpResourceKeys.AudioLatency,
            [MachineSettingsConstants.AudioEnabled] = SettingsHelpResourceKeys.AudioEnabled,
            [MachineSettingsConstants.AudioOutput] = SettingsHelpResourceKeys.AudioOutput,
            [SettingsConstants.OptionFloppySound] = SettingsHelpResourceKeys.AudioFloppyEnabled,
            [SettingsConstants.OptionSoundVolumeCd] = SettingsHelpResourceKeys.AudioCdVolume,
            [SettingsConstants.OptionSoundInterpol] = SettingsHelpResourceKeys.AudioInterpolation,
            [SettingsConstants.OptionSoundFilter] = SettingsHelpResourceKeys.AudioFilter,
            [SettingsConstants.OptionSoundFilterType] = SettingsHelpResourceKeys.AudioFilterType,
            [MachineSettingsConstants.AudioStereoSeparation] = SettingsHelpResourceKeys.AudioStereoSeparation,
            [SettingsConstants.OptionFloppySoundType] = SettingsHelpResourceKeys.AudioFloppySoundType,
            [SettingsConstants.OptionFloppySoundEmptyMute] = SettingsHelpResourceKeys.AudioFloppyMuteEmpty,
            [SettingsConstants.OptionAnalogmouse] = SettingsHelpResourceKeys.MouseAnalog,
            [SettingsConstants.OptionAnalogmouseDeadzone] = SettingsHelpResourceKeys.MouseAnalogDeadzone,
            [SettingsConstants.OptionAnalogmouseSpeed] = SettingsHelpResourceKeys.MouseAnalogSpeed,
            [SettingsConstants.OptionAnalogmouseSpeedRight] = SettingsHelpResourceKeys.MouseAnalogSpeed,
            [SettingsConstants.OptionMouseSpeed] = SettingsHelpResourceKeys.MouseSpeed,
            [SettingsConstants.OptionTurboPulse] = SettingsHelpResourceKeys.ControllerTurboPulse,
            [SettingsConstants.ParallelJoystickAdapter] = SettingsHelpResourceKeys.ControllerParallelAdapter
        };
}
