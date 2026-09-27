namespace GWGUI.Emulation.Amiga.Common.Machines.Common.Dictionaries;

internal static class SettingsHelpDictionary
{
    internal static readonly IReadOnlyDictionary<string, string> Resources =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SettingsConstants.OptionCpuModel] = "Emulation.Help.Cpu.Model",
            [SettingsConstants.OptionFpuModel] = "Emulation.Help.Cpu.FpuModel",
            [SettingsConstants.OptionCpuCompatibility] = "Emulation.Help.Cpu.Precision",
            [SettingsConstants.CpuOriginalSpeed] = "Emulation.Help.Cpu.OriginalSpeed",
            [SettingsConstants.CpuSpeed] = "Emulation.Help.Cpu.Speed",
            [SettingsConstants.OptionChipmemSize] = "Emulation.Help.Memory.Main",
            [SettingsConstants.OptionBogomemSize] = "Emulation.Help.Memory.Slow",
            [SettingsConstants.OptionFastmemSize] = "Emulation.Help.Memory.Fast",
            [SettingsConstants.OptionZ3memSize] = "Emulation.Help.Memory.Z3",
            [SettingsConstants.KickstartPath] = "Emulation.Help.Firmware.System",
            [SettingsConstants.ExtendedRomPath] = "Emulation.Help.Firmware.ExtendedRom",
            [SettingsConstants.RomKeyPath] = "Emulation.Help.Firmware.RomKey",
            [SettingsConstants.OptionVideoStandard] = "Emulation.Help.Video.Standard",
            [SettingsConstants.OptionVideoResolution] = "Emulation.Help.Video.Resolution",
            [SettingsConstants.OptionVideoAspect] = "Emulation.Help.Video.AspectRatio",
            [SettingsConstants.OptionCrop] = "Emulation.Help.Video.Crop",
            [SettingsConstants.OptionVideoVresolution] = "Emulation.Help.Video.LineMode",
            [SettingsConstants.OptionVideoAllowHzChange] = "Emulation.Help.Video.HzChange",
            [SettingsConstants.OptionGfxFramerate] = "Emulation.Help.Video.FrameSkip",
            [SettingsConstants.OptionGfxColors] = "Emulation.Help.Video.Colors",
            [SettingsConstants.OptionGfxGamma] = "Emulation.Help.Video.Gamma",
            [SettingsConstants.OptionImmediateBlits] = "Emulation.Help.Video.ImmediateBlits",
            [SettingsConstants.OptionCollisionLevel] = "Emulation.Help.Video.CollisionLevel",
            [SettingsConstants.OptionGfxFlickerfixer] = "Emulation.Help.Video.FlickerFixer",
            [SettingsConstants.AudioLatency] = "Emulation.Help.Audio.Latency",
            [SettingsConstants.AudioEnabled] = "Emulation.Help.Audio.Enabled",
            [SettingsConstants.AudioOutput] = "Emulation.Help.Audio.Output",
            [SettingsConstants.OptionFloppySound] = "Emulation.Help.Audio.Floppy.Enabled",
            [SettingsConstants.OptionSoundVolumeCd] = "Emulation.Help.Audio.CdVolume",
            [SettingsConstants.OptionSoundInterpol] = "Emulation.Help.Audio.Interpolation",
            [SettingsConstants.OptionSoundFilter] = "Emulation.Help.Audio.Filter",
            [SettingsConstants.OptionSoundFilterType] = "Emulation.Help.Audio.FilterType",
            [SettingsConstants.AudioStereoSeparation] = "Emulation.Help.Audio.StereoSeparation",
            [SettingsConstants.OptionFloppySoundType] = "Emulation.Help.Audio.Floppy.SoundType",
            [SettingsConstants.OptionFloppySoundEmptyMute] = "Emulation.Help.Audio.Floppy.MuteEmpty",
            [SettingsConstants.OptionAnalogmouse] = "Emulation.Help.Mouse.Analog",
            [SettingsConstants.OptionAnalogmouseDeadzone] = "Emulation.Help.Mouse.AnalogDeadzone",
            [SettingsConstants.OptionAnalogmouseSpeed] = "Emulation.Help.Mouse.AnalogSpeed",
            [SettingsConstants.OptionAnalogmouseSpeedRight] = "Emulation.Help.Mouse.AnalogSpeed",
            [SettingsConstants.OptionMouseSpeed] = "Emulation.Help.Mouse.Speed",
            [SettingsConstants.OptionTurboPulse] = "Emulation.Help.Controller.TurboPulse",
            [SettingsConstants.ParallelJoystickAdapter] = "Emulation.Help.Controller.ParallelAdapter"
        };
}
