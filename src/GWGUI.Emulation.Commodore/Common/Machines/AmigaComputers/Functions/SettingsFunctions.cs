using System.IO;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Functions;

internal static partial class AmigaSettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(AmigaModel model,
        MachineConfiguration configuration)
    {
        var options = configuration.Options ?? new Dictionary<string, string>();
        var cpu = Value(options, SettingsConstants.OptionCpuModel, CpuValue(model.DefaultCpu));
        var selectedCpu = model.CpuModels.FirstOrDefault(candidate => CpuValue(candidate) == cpu, model.DefaultCpu);
        var compatibility = SettingsValueFunctions.Parse(SettingsChoiceConstants.CpuCompatibilityChoices,
            Value(options, SettingsConstants.OptionCpuCompatibility, SettingsChoiceConstants.CpuCompatibilityChoices[CpuCompatibility.Exact].Id),
            CpuCompatibility.Exact);
        var standard = SettingsValueFunctions.Parse(SettingsChoiceConstants.VideoStandardChoices,
            Value(options, SettingsConstants.OptionVideoStandard, SettingsChoiceConstants.VideoStandardChoices[VideoStandard.PAL].Id),
            VideoStandard.PAL);
        var ntsc = standard is VideoStandard.NTSC or VideoStandard.NTSCAuto;
        var frequencies = CpuFrequencyChoices(model, compatibility, ntsc);
        var frequency = CpuFrequencyValue(options, compatibility, frequencies);
        return
        [
            Block(SettingsBlocks.Cpu, EmulationMachineTab.Cpu, SettingsResourceKeys.CpuProcessor, SettingsGlyphs.Processor, MachineSettingsConstants.TwoSettingColumns,
                Select(SettingsConstants.OptionCpuModel, EmulationMachineTab.Cpu, SettingsBlocks.Cpu, SettingsResourceKeys.CpuModel,
                    cpu, model.CpuModels.Select(CpuChoice), model.CpuModels.Count > MachineSettingsConstants.InformationColumnCount,
                    refreshSettingsOnChange: true),
                Select(SettingsConstants.OptionFpuModel, EmulationMachineTab.Cpu, SettingsBlocks.Cpu, SettingsResourceKeys.FpuModel,
                    Value(options, SettingsConstants.OptionFpuModel, FpuValue(DefaultFpu(selectedCpu))), FpuValues(selectedCpu).Select(FpuChoice)),
                Select(SettingsConstants.OptionCpuCompatibility, EmulationMachineTab.Cpu, SettingsBlocks.Cpu, SettingsResourceKeys.CpuPrecision,
                    SettingsChoiceConstants.CpuCompatibilityChoices[compatibility].Id, CompatibilityChoices(), refreshSettingsOnChange: true),
                Information(SettingsConstants.CpuOriginalSpeed, EmulationMachineTab.Cpu, SettingsBlocks.Cpu,
                    SettingsResourceKeys.CpuSpeedOriginal, FormatMhz(NominalCpuFrequencyMhz(model, ntsc))),
                Select(SettingsConstants.CpuSpeed, EmulationMachineTab.Cpu, SettingsBlocks.Cpu, SettingsResourceKeys.CpuSpeed,
                    frequency, frequencies)),
            Block(SettingsBlocks.MainMemory, EmulationMachineTab.Ram, SettingsResourceKeys.MemoryMain, SettingsGlyphs.Memory, MachineSettingsConstants.TwoSettingColumns,
                Select(SettingsConstants.OptionChipmemSize, EmulationMachineTab.Ram, SettingsBlocks.MainMemory, SettingsResourceKeys.MemoryMain,
                    Value(options, SettingsConstants.OptionChipmemSize, ChipMemoryValue(model.ChipMemory)),
                    ChipMemoryValues(model).Select(ChipMemoryChoice)),
                Select(SettingsConstants.OptionBogomemSize, EmulationMachineTab.Ram, SettingsBlocks.MainMemory, SettingsResourceKeys.MemorySlow,
                    Value(options, SettingsConstants.OptionBogomemSize, SlowMemoryValue(model.SlowMemory)),
                    SlowMemoryValues(model).Select(SlowMemoryChoice))),
            Block(SettingsBlocks.ExtensionMemory, EmulationMachineTab.Ram, SettingsResourceKeys.MemoryExtensions, SettingsGlyphs.Memory, MachineSettingsConstants.TwoSettingColumns,
                Select(SettingsConstants.OptionFastmemSize, EmulationMachineTab.Ram, SettingsBlocks.ExtensionMemory, SettingsResourceKeys.MemoryFast,
                    Value(options, SettingsConstants.OptionFastmemSize, model.FastMemoryMib.ToString()),
                    new[] { RamCapacity.None, RamCapacity._1MB, RamCapacity._2MB, RamCapacity._4MB, RamCapacity._8MB }.Select(MemoryMibChoice)),
                Select(SettingsConstants.OptionZ3memSize, EmulationMachineTab.Ram, SettingsBlocks.ExtensionMemory, SettingsResourceKeys.MemoryZ3,
                    Value(options, SettingsConstants.OptionZ3memSize, SettingsValueConstants.DisabledMemory), model.Id is GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A3000 or GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A4000
                        ? new[] { RamCapacity.None, RamCapacity._1MB, RamCapacity._2MB, RamCapacity._4MB, RamCapacity._8MB, RamCapacity._16MB, RamCapacity._32MB, RamCapacity._64MB, RamCapacity._128MB, RamCapacity._256MB, RamCapacity._512MB }
                            .Select(MemoryMibChoice)
                        : new[] { RamCapacity.None }.Select(MemoryMibChoice))),
            Block(SettingsBlocks.Firmware, EmulationMachineTab.Rom, SettingsResourceKeys.FirmwareRomSystem, SettingsGlyphs.Firmware, MachineSettingsConstants.InformationColumnCount,
                Path(SettingsConstants.KickstartPath, SettingsResourceKeys.FirmwareRomKickstart, configuration.FirmwarePath(SettingsConstants.KickstartPath)),
                Path(SettingsConstants.ExtendedRomPath, SettingsResourceKeys.FirmwareRomExtended, configuration.FirmwarePath(SettingsConstants.ExtendedRomPath)),
                Path(SettingsConstants.RomKeyPath, SettingsResourceKeys.FirmwareRomKey, configuration.FirmwarePath(SettingsConstants.RomKeyPath))),
            Block(SettingsBlocks.Display, EmulationMachineTab.Video, SettingsResourceKeys.VideoSettingsDisplay, SettingsGlyphs.Video, MachineSettingsConstants.TwoSettingColumns,
                Select(SettingsConstants.OptionVideoStandard, EmulationMachineTab.Video, SettingsBlocks.Display, SettingsResourceKeys.VideoStandard,
                    Value(options, SettingsConstants.OptionVideoStandard, SettingsValueConstants.PAL), VideoStandardChoices(),
                    refreshSettingsOnChange: true),
                Select(SettingsConstants.OptionVideoResolution, EmulationMachineTab.Video, SettingsBlocks.Display, SettingsResourceKeys.VideoResolution,
                    Value(options, SettingsConstants.OptionVideoResolution, SettingsValueConstants.Auto), VideoResolutionChoices()),
                Select(SettingsConstants.OptionVideoAspect, EmulationMachineTab.Video, SettingsBlocks.Display, SettingsResourceKeys.VideoAspectRatio,
                    Value(options, SettingsConstants.OptionVideoAspect, SettingsChoiceConstants.VideoAspectChoices[VideoAspect.Automatic].Id), VideoAspectChoices()),
                Select(SettingsConstants.OptionCrop, EmulationMachineTab.Video, SettingsBlocks.Display, SettingsResourceKeys.VideoCrop,
                    Value(options, SettingsConstants.OptionCrop, SettingsValueConstants.Disabled), CropChoices()),
                Select(SettingsConstants.OptionVideoVresolution, EmulationMachineTab.Video, SettingsBlocks.Display, SettingsResourceKeys.VideoLineMode,
                    Value(options, SettingsConstants.OptionVideoVresolution, SettingsValueConstants.Auto), LineModeChoices()),
                Select(SettingsConstants.OptionVideoAllowHzChange, EmulationMachineTab.Video, SettingsBlocks.Display, SettingsResourceKeys.VideoHzChange,
                    Value(options, SettingsConstants.OptionVideoAllowHzChange, SettingsChoiceConstants.RefreshRateChangeChoices[RefreshRateChange.Locked].Id), HzChangeChoices()),
                Select(SettingsConstants.OptionGfxFramerate, EmulationMachineTab.Video, SettingsBlocks.Display, SettingsResourceKeys.VideoFrameSkip,
                    Value(options, SettingsConstants.OptionGfxFramerate, SettingsValueConstants.Disabled), FrameSkipChoices()),
                Select(SettingsConstants.OptionGfxColors, EmulationMachineTab.Video, SettingsBlocks.Display, SettingsResourceKeys.VideoColors,
                    Value(options, SettingsConstants.OptionGfxColors, SettingsValueConstants.ColorDepth24Bit), InvariantChoices(SettingsValueConstants.ColorDepth16Bit, SettingsValueConstants.ColorDepth24Bit)),
                Select(SettingsConstants.OptionGfxGamma, EmulationMachineTab.Video, SettingsBlocks.Display, SettingsResourceKeys.VideoGamma,
                    Value(options, SettingsConstants.OptionGfxGamma, SettingsValueConstants.NeutralGamma), Enumerable.Range(SettingsValueConstants.MinimumGammaStep, SettingsValueConstants.GammaStepCount)
                        .Select(value => Invariant((value * SettingsValueConstants.NativeGammaUnitsPerStep).ToString(), value.ToString()))),
                Select(SettingsConstants.OptionImmediateBlits, EmulationMachineTab.Video, SettingsBlocks.Display, SettingsResourceKeys.StateImmediateBlits,
                    Value(options, SettingsConstants.OptionImmediateBlits, SettingsChoiceConstants.BlitterModeChoices[BlitterMode.Normal].Id), ImmediateBlitChoices()),
                Select(SettingsConstants.OptionCollisionLevel, EmulationMachineTab.Video, SettingsBlocks.Display, SettingsResourceKeys.VideoCollisionLevel,
                    Value(options, SettingsConstants.OptionCollisionLevel, SettingsChoiceConstants.CollisionModeChoices[CollisionMode.Playfields].Id), CollisionChoices()),
                Toggle(SettingsConstants.OptionGfxFlickerfixer, EmulationMachineTab.Video, SettingsBlocks.Display,
                    SettingsResourceKeys.VideoFlickerFixer, Value(options, SettingsConstants.OptionGfxFlickerfixer, SettingsValueConstants.Disabled) == SettingsValueConstants.Enabled)),
            Block(SettingsBlocks.Audio, EmulationMachineTab.Audio, SettingsResourceKeys.Audio, SettingsGlyphs.Audio, MachineSettingsConstants.TwoSettingColumns,
                Toggle(MachineSettingsConstants.AudioEnabled, EmulationMachineTab.Audio, SettingsBlocks.Audio,
                    SettingsResourceKeys.AudioEnabled, configuration.AudioEnabled),
                AudioOutput(configuration.Audio?.OutputDeviceId),
                Select(MachineSettingsConstants.AudioLatency, EmulationMachineTab.Audio, SettingsBlocks.Audio,
                    SettingsResourceKeys.AudioLatencyLabel, (configuration.Audio?.LatencyMilliseconds ?? MachineSettingsConstants.DefaultAudioLatencyMilliseconds).ToString(),
                    SettingsValueConstants.AudioLatencyChoicesMilliseconds.Select(value =>
                        Invariant(value.ToString(), string.Format(SettingsValueConstants.MillisecondsLabelFormat, value)))),
                Select(SettingsConstants.OptionSoundInterpol, EmulationMachineTab.Audio, SettingsBlocks.Audio, SettingsResourceKeys.AudioInterpolation,
                    Value(options, SettingsConstants.OptionSoundInterpol, SettingsChoiceConstants.AudioInterpolationChoices[AudioInterpolation.Anti].Id),
                    AudioInterpolationChoices()),
                Select(SettingsConstants.OptionSoundFilter, EmulationMachineTab.Audio, SettingsBlocks.Audio, SettingsResourceKeys.AudioFilter,
                    Value(options, SettingsConstants.OptionSoundFilter, SettingsChoiceConstants.AudioFilterChoices[AudioFilter.Emulated].Id),
                    AudioFilterChoices()),
                Select(SettingsConstants.OptionSoundFilterType, EmulationMachineTab.Audio, SettingsBlocks.Audio, SettingsResourceKeys.AudioFilterType,
                    Value(options, SettingsConstants.OptionSoundFilterType, SettingsChoiceConstants.AudioFilterTypeChoices[AudioFilterType.Automatic].Id), FilterTypeChoices()),
                Select(MachineSettingsConstants.AudioStereoSeparation, EmulationMachineTab.Audio, SettingsBlocks.Audio,
                    SettingsResourceKeys.AudioStereoSeparation, $"{configuration.Audio?.StereoSeparation ?? MachineSettingsConstants.FullStereoSeparation}",
                    PercentageChoices(SettingsValueConstants.MinimumPercentage, SettingsValueConstants.MaximumPercentage, SettingsValueConstants.StereoPercentageStep)),
                Select(SettingsConstants.OptionFloppySound, EmulationMachineTab.Audio, SettingsBlocks.Audio, SettingsResourceKeys.AudioFloppySound,
                    Value(options, SettingsConstants.OptionFloppySound, SettingsValueConstants.DefaultFloppyVolume), PercentageChoices(SettingsValueConstants.MinimumPercentage, SettingsValueConstants.MaximumPercentage, SettingsValueConstants.VolumePercentageStep)),
                Select(SettingsConstants.OptionFloppySoundType, EmulationMachineTab.Audio, SettingsBlocks.Audio,
                    SettingsResourceKeys.AudioFloppySoundType, Value(options, SettingsConstants.OptionFloppySoundType, SettingsValueConstants.Internal),
                    [new(SettingsValueConstants.Internal, SettingsResourceKeys.ValueInternal), Invariant(GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A500, GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A500),
                        new(SettingsValueConstants.LOUD, SettingsResourceKeys.ValueLoud)]),
                Toggle(SettingsConstants.OptionFloppySoundEmptyMute, EmulationMachineTab.Audio, SettingsBlocks.Audio,
                    SettingsResourceKeys.AudioFloppyMuteEmpty,
                    Value(options, SettingsConstants.OptionFloppySoundEmptyMute, SettingsValueConstants.Enabled) == SettingsValueConstants.Enabled),
                Select(SettingsConstants.OptionSoundVolumeCd, EmulationMachineTab.Audio, SettingsBlocks.Audio, SettingsResourceKeys.AudioCdVolume,
                    Value(options, SettingsConstants.OptionSoundVolumeCd, SettingsValueConstants.FullCdVolume).TrimEnd(SettingsValueConstants.PercentSuffix), PercentageChoices(SettingsValueConstants.MinimumPercentage, SettingsValueConstants.MaximumPercentage, SettingsValueConstants.VolumePercentageStep))),
            Block(SettingsBlocks.Mouse, EmulationMachineTab.Mouse, SettingsResourceKeys.TabMouse, SettingsGlyphs.Mouse, MachineSettingsConstants.TwoSettingColumns,
                Number(SettingsConstants.OptionMouseSpeed, EmulationMachineTab.Mouse, SettingsBlocks.Mouse, SettingsResourceKeys.MouseSpeed,
                    Value(options, SettingsConstants.OptionMouseSpeed, SettingsValueConstants.StandardMouseSpeedPercentage)),
                Select(SettingsConstants.OptionAnalogmouse, EmulationMachineTab.Mouse, SettingsBlocks.Mouse, SettingsResourceKeys.MouseAnalog,
                    Value(options, SettingsConstants.OptionAnalogmouse, SettingsChoiceConstants.AnalogMouseModeChoices[AnalogMouseMode.Both].Id), AnalogMouseChoices()),
                Select(SettingsConstants.OptionAnalogmouseDeadzone, EmulationMachineTab.Mouse, SettingsBlocks.Mouse,
                    SettingsResourceKeys.MouseAnalogDeadzone, Value(options, SettingsConstants.OptionAnalogmouseDeadzone, SettingsValueConstants.DefaultAnalogMouseDeadzone),
                    PercentageChoices(SettingsValueConstants.MinimumPercentage, SettingsValueConstants.MaximumAnalogDeadzonePercentage, SettingsValueConstants.VolumePercentageStep)),
                Select(SettingsConstants.OptionAnalogmouseSpeed, EmulationMachineTab.Mouse, SettingsBlocks.Mouse,
                    SettingsResourceKeys.MouseAnalogSpeed, Value(options, SettingsConstants.OptionAnalogmouseSpeed, SettingsValueConstants.UnitAnalogMouseSpeed),
                    RatioChoices()),
                Select(SettingsConstants.OptionAnalogmouseSpeedRight, EmulationMachineTab.Mouse, SettingsBlocks.Mouse,
                    SettingsResourceKeys.MouseAnalogSpeed, Value(options, SettingsConstants.OptionAnalogmouseSpeedRight, SettingsValueConstants.UnitAnalogMouseSpeed),
                    RatioChoices())),
            Block(SettingsBlocks.ControllerBehavior, EmulationMachineTab.Controllers,
                SettingsResourceKeys.ControllerActionTurboFire, SettingsGlyphs.Controller, MachineSettingsConstants.TwoSettingColumns,
                Select(SettingsConstants.OptionTurboPulse, EmulationMachineTab.Controllers, SettingsBlocks.ControllerBehavior,
                    SettingsResourceKeys.ControllerTurboPulse, Value(options, SettingsConstants.OptionTurboPulse, SettingsValueConstants.DefaultTurboPulseFrames),
                    SettingsValueConstants.TurboPulseFrameCounts.Select(value => Invariant(value.ToString(), value.ToString()))),
                Toggle(SettingsConstants.ParallelJoystickAdapter, EmulationMachineTab.Controllers,
                    SettingsBlocks.ControllerBehavior, SettingsResourceKeys.AmigaControllerParallelAdapter,
                    configuration.Input?.ParallelJoystickAdapterEnabled == true,
                    refreshSettingsOnChange: true))
        ];
    }

    private static EmulationSettingsBlock Block(string id, EmulationMachineTab tab, string title,
        string icon, int columns, params EmulationSettingsField[] fields) =>
        new(id, tab, title, fields, icon, columns);

    private static EmulationSettingsField Select(string id, EmulationMachineTab tab, string block,
        string label, string value, IEnumerable<string> choices) =>
        Select(id, tab, block, label, value,
            choices.Select(choice => new EmulationSettingsChoice(choice, string.Empty, choice)));

    private static EmulationSettingsField Select(string id, EmulationMachineTab tab, string block,
        string label, string value, IEnumerable<EmulationSettingsChoice> choices, bool isEnabled = true,
        bool refreshSettingsOnChange = false) =>
        new(id, tab, block, label, EmulationSettingsEditor.Selection, value,
            choices.ToArray(), IsEnabled: isEnabled, ExplanationResourceKey: ShortHelp(id),
            DetailedExplanationResourceKey: DetailedHelp(id), RefreshSettingsOnChange: refreshSettingsOnChange);
}
