using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    private static readonly IReadOnlyDictionary<string, string> FieldHelpResources =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SettingsDescriptionFunctionsConstants.OptionFpuModel] = "Emulation.Help.Cpu.FpuModel",
            [SettingsDescriptionFunctionsConstants.OptionCpuCompatibility] = "Emulation.Help.Cpu.Precision",
            [SettingsConstants.CpuSpeed] = "Emulation.Help.Cpu.Speed",
            [SettingsDescriptionFunctionsConstants.OptionBogomemSize] = "Emulation.Help.Memory.Slow",
            [SettingsDescriptionFunctionsConstants.OptionFastmemSize] = "Emulation.Help.Memory.Fast",
            [SettingsDescriptionFunctionsConstants.OptionZ3memSize] = "Emulation.Help.Memory.Z3",
            [SettingsConstants.ExtendedRomPath] = "Emulation.Help.Firmware.ExtendedRom",
            [SettingsConstants.RomKeyPath] = "Emulation.Help.Firmware.RomKey",
            [SettingsDescriptionFunctionsConstants.OptionVideoStandard] = "Emulation.Help.Video.Standard",
            [SettingsDescriptionFunctionsConstants.OptionVideoAspect] = "Emulation.Help.Video.AspectRatio",
            [SettingsDescriptionFunctionsConstants.OptionVideoVresolution] = "Emulation.Help.Video.LineMode",
            [SettingsDescriptionFunctionsConstants.OptionVideoAllowHzChange] = "Emulation.Help.Video.HzChange",
            [SettingsDescriptionFunctionsConstants.OptionGfxFramerate] = "Emulation.Help.Video.FrameSkip",
            [SettingsDescriptionFunctionsConstants.OptionGfxColors] = "Emulation.Help.Video.Colors",
            [SettingsDescriptionFunctionsConstants.OptionGfxGamma] = "Emulation.Help.Video.Gamma",
            [SettingsDescriptionFunctionsConstants.OptionImmediateBlits] = "Emulation.Help.Video.ImmediateBlits",
            [SettingsDescriptionFunctionsConstants.OptionCollisionLevel] = "Emulation.Help.Video.CollisionLevel",
            [SettingsDescriptionFunctionsConstants.OptionGfxFlickerfixer] = "Emulation.Help.Video.FlickerFixer",
            [SettingsConstants.AudioLatency] = "Emulation.Help.Audio.Latency",
            [SettingsDescriptionFunctionsConstants.OptionSoundInterpol] = "Emulation.Help.Audio.Interpolation",
            [SettingsDescriptionFunctionsConstants.OptionSoundFilter] = "Emulation.Help.Audio.Filter",
            [SettingsDescriptionFunctionsConstants.OptionSoundFilterType] = "Emulation.Help.Audio.FilterType",
            [SettingsConstants.AudioStereoSeparation] = "Emulation.Help.Audio.StereoSeparation",
            [SettingsDescriptionFunctionsConstants.OptionFloppySoundType] = "Emulation.Help.Audio.Floppy.SoundType",
            [SettingsDescriptionFunctionsConstants.OptionFloppySoundEmptyMute] = "Emulation.Help.Audio.Floppy.MuteEmpty",
            [SettingsDescriptionFunctionsConstants.OptionAnalogmouse] = "Emulation.Help.Mouse.Analog",
            [SettingsDescriptionFunctionsConstants.OptionAnalogmouseDeadzone] = "Emulation.Help.Mouse.AnalogDeadzone",
            [SettingsDescriptionFunctionsConstants.OptionAnalogmouseSpeed] = "Emulation.Help.Mouse.AnalogSpeed",
            [SettingsDescriptionFunctionsConstants.OptionAnalogmouseSpeedRight] = "Emulation.Help.Mouse.AnalogSpeed",
            [SettingsDescriptionFunctionsConstants.OptionTurboPulse] = "Emulation.Help.Controller.TurboPulse",
            [SettingsConstants.ParallelJoystickAdapter] = "Emulation.Help.Controller.ParallelAdapter"
        };

    internal static IReadOnlyList<EmulationSettingsBlock> Create(Model model,
        MachineConfiguration configuration)
    {
        var options = configuration.Options ?? new Dictionary<string, string>();
        var cpu = Value(options, SettingsDescriptionFunctionsConstants.OptionCpuModel, model.DefaultCpu);
        var compatibility = Value(options, SettingsDescriptionFunctionsConstants.OptionCpuCompatibility, SettingsDescriptionFunctionsConstants.Exact);
        var ntsc = Value(options, SettingsDescriptionFunctionsConstants.OptionVideoStandard, SettingsDescriptionFunctionsConstants.PAL)
            .StartsWith(SettingsDescriptionFunctionsConstants.NTSC, StringComparison.OrdinalIgnoreCase);
        var frequencies = CpuFrequencyChoices(model, compatibility, ntsc);
        var frequency = CpuFrequencyValue(options, compatibility, frequencies);
        return
        [
            Block(SettingsDescriptionFunctionsConstants.Cpu, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.ResourceCpuProcessor, SettingsDescriptionFunctionsConstants.Value, 2,
                Select(SettingsDescriptionFunctionsConstants.OptionCpuModel, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Cpu, SettingsDescriptionFunctionsConstants.ResourceCpuModel,
                    cpu, model.CpuModels.Select(CpuChoice), model.CpuModels.Count > 1,
                    refreshSettingsOnChange: true),
                Select(SettingsDescriptionFunctionsConstants.OptionFpuModel, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Cpu, SettingsDescriptionFunctionsConstants.ResourceFpuModel,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionFpuModel, DefaultFpu(cpu)), FpuValues(cpu).Select(FpuChoice)),
                Select(SettingsDescriptionFunctionsConstants.OptionCpuCompatibility, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Cpu, SettingsDescriptionFunctionsConstants.ResourceCpuPrecision,
                    compatibility, CompatibilityChoices(), refreshSettingsOnChange: true),
                Information(SettingsConstants.CpuOriginalSpeed, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Cpu,
                    SettingsDescriptionFunctionsConstants.ResourceCpuSpeedOriginal, FormatMhz(NominalCpuFrequencyMhz(model, ntsc))),
                Select(SettingsConstants.CpuSpeed, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Cpu, SettingsDescriptionFunctionsConstants.ResourceCpuSpeed,
                    frequency, frequencies)),
            Block(SettingsDescriptionFunctionsConstants.MainMemory, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.ResourceMemoryMain, SettingsDescriptionFunctionsConstants.Value2, 2,
                Select(SettingsDescriptionFunctionsConstants.OptionChipmemSize, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.MainMemory, SettingsDescriptionFunctionsConstants.ResourceMemoryMain,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionChipmemSize, ChipMemoryValue(model.ChipMemoryKib)),
                    ChipMemoryValues(model).Select(ChipMemoryChoice)),
                Select(SettingsDescriptionFunctionsConstants.OptionBogomemSize, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.MainMemory, SettingsDescriptionFunctionsConstants.ResourceMemorySlow,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionBogomemSize, SlowMemoryValue(model.SlowMemoryKib)),
                    SlowMemoryValues(model).Select(SlowMemoryChoice))),
            Block(SettingsDescriptionFunctionsConstants.ExtensionMemory, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.ResourceMemoryExtensions, SettingsDescriptionFunctionsConstants.Value2, 2,
                Select(SettingsDescriptionFunctionsConstants.OptionFastmemSize, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.ExtensionMemory, SettingsDescriptionFunctionsConstants.ResourceMemoryFast,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionFastmemSize, model.FastMemoryMib.ToString()),
                    new[] { SettingsDescriptionFunctionsConstants.Value0, SettingsDescriptionFunctionsConstants.Value1, SettingsDescriptionFunctionsConstants.Value22, SettingsDescriptionFunctionsConstants.Value4, SettingsDescriptionFunctionsConstants.Value8 }.Select(MemoryMibChoice)),
                Select(SettingsDescriptionFunctionsConstants.OptionZ3memSize, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.ExtensionMemory, SettingsDescriptionFunctionsConstants.ResourceMemoryZ3,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionZ3memSize, SettingsDescriptionFunctionsConstants.Value0), model.Id is SettingsDescriptionFunctionsConstants.A3000 or SettingsDescriptionFunctionsConstants.A4000
                        ? new[] { SettingsDescriptionFunctionsConstants.Value0, SettingsDescriptionFunctionsConstants.Value1, SettingsDescriptionFunctionsConstants.Value22, SettingsDescriptionFunctionsConstants.Value4, SettingsDescriptionFunctionsConstants.Value8, SettingsDescriptionFunctionsConstants.Value16, SettingsDescriptionFunctionsConstants.Value32, SettingsDescriptionFunctionsConstants.Value64, SettingsDescriptionFunctionsConstants.Value128, SettingsDescriptionFunctionsConstants.Value256, SettingsDescriptionFunctionsConstants.Value512 }
                            .Select(MemoryMibChoice)
                        : new[] { SettingsDescriptionFunctionsConstants.Value0 }.Select(MemoryMibChoice))),
            Block(SettingsDescriptionFunctionsConstants.Firmware, EmulationMachineTab.Rom, SettingsDescriptionFunctionsConstants.ResourceFirmwareRomSystem, SettingsDescriptionFunctionsConstants.Value3, 1,
                Path(SettingsConstants.KickstartPath, SettingsDescriptionFunctionsConstants.ResourceFirmwareRomKickstart, configuration.KickstartPath),
                Path(SettingsConstants.ExtendedRomPath, SettingsDescriptionFunctionsConstants.ResourceFirmwareRomExtended, configuration.ExtendedRomPath),
                Path(SettingsConstants.RomKeyPath, SettingsDescriptionFunctionsConstants.ResourceFirmwareRomKey, configuration.RomKeyPath)),
            Block(SettingsDescriptionFunctionsConstants.Display, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.ResourceVideoSettingsDisplay, SettingsDescriptionFunctionsConstants.Value5, 2,
                Select(SettingsDescriptionFunctionsConstants.OptionVideoStandard, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoStandard,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionVideoStandard, SettingsDescriptionFunctionsConstants.PAL), VideoStandardChoices(),
                    refreshSettingsOnChange: true),
                Select(SettingsDescriptionFunctionsConstants.OptionVideoResolution, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoResolution,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionVideoResolution, SettingsDescriptionFunctionsConstants.Auto), VideoResolutionChoices()),
                Select(SettingsDescriptionFunctionsConstants.OptionVideoAspect, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoAspectRatio,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionVideoAspect, SettingsDescriptionFunctionsConstants.Auto), VideoAspectChoices()),
                Select(SettingsDescriptionFunctionsConstants.OptionCrop, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoCrop,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionCrop, SettingsDescriptionFunctionsConstants.Disabled), CropChoices()),
                Select(SettingsDescriptionFunctionsConstants.OptionVideoVresolution, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoLineMode,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionVideoVresolution, SettingsDescriptionFunctionsConstants.Auto), LineModeChoices()),
                Select(SettingsDescriptionFunctionsConstants.OptionVideoAllowHzChange, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoHzChange,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionVideoAllowHzChange, SettingsDescriptionFunctionsConstants.Locked), HzChangeChoices()),
                Select(SettingsDescriptionFunctionsConstants.OptionGfxFramerate, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoFrameSkip,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionGfxFramerate, SettingsDescriptionFunctionsConstants.Disabled), FrameSkipChoices()),
                Select(SettingsDescriptionFunctionsConstants.OptionGfxColors, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoColors,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionGfxColors, SettingsDescriptionFunctionsConstants.Value24bit), InvariantChoices(SettingsDescriptionFunctionsConstants.Value16bit, SettingsDescriptionFunctionsConstants.Value24bit)),
                Select(SettingsDescriptionFunctionsConstants.OptionGfxGamma, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoGamma,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionGfxGamma, SettingsDescriptionFunctionsConstants.Value0), Enumerable.Range(-5, 11)
                        .Select(value => Invariant((value * 100).ToString(), value.ToString()))),
                Select(SettingsDescriptionFunctionsConstants.OptionImmediateBlits, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceStateImmediateBlits,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionImmediateBlits, SettingsDescriptionFunctionsConstants.False), ImmediateBlitChoices()),
                Select(SettingsDescriptionFunctionsConstants.OptionCollisionLevel, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoCollisionLevel,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionCollisionLevel, SettingsDescriptionFunctionsConstants.Playfields), CollisionChoices()),
                Toggle(SettingsDescriptionFunctionsConstants.OptionGfxFlickerfixer, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display,
                    SettingsDescriptionFunctionsConstants.ResourceVideoFlickerFixer, Value(options, SettingsDescriptionFunctionsConstants.OptionGfxFlickerfixer, SettingsDescriptionFunctionsConstants.Disabled) == SettingsDescriptionFunctionsConstants.Enabled)),
            Block(SettingsDescriptionFunctionsConstants.Audio, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.ResourceAudio, SettingsDescriptionFunctionsConstants.Value6, 2,
                Toggle(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudioEnabled, configuration.AudioEnabled),
                AudioOutput(configuration.Audio?.OutputDeviceId),
                Select(SettingsConstants.AudioLatency, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudioLatencyLabel, (configuration.Audio?.LatencyMilliseconds ?? 50).ToString(),
                    new[] { 20, 35, 50, 75, 100, 150, 250 }.Select(value =>
                        Invariant(value.ToString(), $"{value} ms"))),
                Select(SettingsDescriptionFunctionsConstants.OptionSoundInterpol, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio, SettingsDescriptionFunctionsConstants.ResourceAudioInterpolation,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionSoundInterpol, configuration.Audio?.Interpolation ?? SettingsDescriptionFunctionsConstants.Anti),
                    AudioInterpolationChoices()),
                Select(SettingsDescriptionFunctionsConstants.OptionSoundFilter, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio, SettingsDescriptionFunctionsConstants.ResourceAudioFilter,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionSoundFilter, configuration.Audio?.Filter ?? SettingsDescriptionFunctionsConstants.Emulated),
                    AudioFilterChoices()),
                Select(SettingsDescriptionFunctionsConstants.OptionSoundFilterType, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio, SettingsDescriptionFunctionsConstants.ResourceAudioFilterType,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionSoundFilterType, SettingsDescriptionFunctionsConstants.Auto), FilterTypeChoices()),
                Select(SettingsConstants.AudioStereoSeparation, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudioStereoSeparation, $"{configuration.Audio?.StereoSeparation ?? 100}",
                    PercentageChoices(0, 100, 10)),
                Select(SettingsDescriptionFunctionsConstants.OptionFloppySound, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio, SettingsDescriptionFunctionsConstants.ResourceAudioFloppySound,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionFloppySound, SettingsDescriptionFunctionsConstants.Value80), PercentageChoices(0, 100, 5)),
                Select(SettingsDescriptionFunctionsConstants.OptionFloppySoundType, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudioFloppySoundType, Value(options, SettingsDescriptionFunctionsConstants.OptionFloppySoundType, SettingsDescriptionFunctionsConstants.Internal),
                    [new(SettingsDescriptionFunctionsConstants.Internal, SettingsDescriptionFunctionsConstants.ResourceValueInternal), Invariant(SettingsDescriptionFunctionsConstants.A500, SettingsDescriptionFunctionsConstants.A500),
                        new(SettingsDescriptionFunctionsConstants.LOUD, SettingsDescriptionFunctionsConstants.ResourceValueLoud)]),
                Toggle(SettingsDescriptionFunctionsConstants.OptionFloppySoundEmptyMute, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudioFloppyMuteEmpty,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionFloppySoundEmptyMute, SettingsDescriptionFunctionsConstants.Enabled) == SettingsDescriptionFunctionsConstants.Enabled),
                Select(SettingsDescriptionFunctionsConstants.OptionSoundVolumeCd, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio, SettingsDescriptionFunctionsConstants.ResourceAudioCdVolume,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionSoundVolumeCd, SettingsDescriptionFunctionsConstants.Value100).TrimEnd('%'), PercentageChoices(0, 100, 5))),
            Block(SettingsDescriptionFunctionsConstants.Mouse, EmulationMachineTab.Mouse, SettingsDescriptionFunctionsConstants.ResourceTabMouse, SettingsDescriptionFunctionsConstants.Value7, 2,
                Number(SettingsDescriptionFunctionsConstants.OptionMouseSpeed, EmulationMachineTab.Mouse, SettingsDescriptionFunctionsConstants.Mouse, SettingsDescriptionFunctionsConstants.ResourceMouseSpeed,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionMouseSpeed, SettingsDescriptionFunctionsConstants.Value1002)),
                Select(SettingsDescriptionFunctionsConstants.OptionAnalogmouse, EmulationMachineTab.Mouse, SettingsDescriptionFunctionsConstants.Mouse, SettingsDescriptionFunctionsConstants.ResourceMouseAnalog,
                    Value(options, SettingsDescriptionFunctionsConstants.OptionAnalogmouse, SettingsDescriptionFunctionsConstants.Both), AnalogMouseChoices()),
                Select(SettingsDescriptionFunctionsConstants.OptionAnalogmouseDeadzone, EmulationMachineTab.Mouse, SettingsDescriptionFunctionsConstants.Mouse,
                    SettingsDescriptionFunctionsConstants.ResourceMouseAnalogDeadzone, Value(options, SettingsDescriptionFunctionsConstants.OptionAnalogmouseDeadzone, SettingsDescriptionFunctionsConstants.Value15),
                    PercentageChoices(0, 50, 5)),
                Select(SettingsDescriptionFunctionsConstants.OptionAnalogmouseSpeed, EmulationMachineTab.Mouse, SettingsDescriptionFunctionsConstants.Mouse,
                    SettingsDescriptionFunctionsConstants.ResourceMouseAnalogSpeed, Value(options, SettingsDescriptionFunctionsConstants.OptionAnalogmouseSpeed, SettingsDescriptionFunctionsConstants.Value10),
                    RatioChoices()),
                Select(SettingsDescriptionFunctionsConstants.OptionAnalogmouseSpeedRight, EmulationMachineTab.Mouse, SettingsDescriptionFunctionsConstants.Mouse,
                    SettingsDescriptionFunctionsConstants.ResourceMouseAnalogSpeed, Value(options, SettingsDescriptionFunctionsConstants.OptionAnalogmouseSpeedRight, SettingsDescriptionFunctionsConstants.Value10),
                    RatioChoices())),
            Block(SettingsDescriptionFunctionsConstants.ControllerBehavior, EmulationMachineTab.Controllers,
                SettingsDescriptionFunctionsConstants.ResourceControllerActionTurboFire, SettingsDescriptionFunctionsConstants.Value9, 2,
                Select(SettingsDescriptionFunctionsConstants.OptionTurboPulse, EmulationMachineTab.Controllers, SettingsDescriptionFunctionsConstants.ControllerBehavior,
                    SettingsDescriptionFunctionsConstants.ResourceControllerTurboPulse, Value(options, SettingsDescriptionFunctionsConstants.OptionTurboPulse, SettingsDescriptionFunctionsConstants.Value62),
                    InvariantChoices(SettingsDescriptionFunctionsConstants.Value22, SettingsDescriptionFunctionsConstants.Value4, SettingsDescriptionFunctionsConstants.Value62, SettingsDescriptionFunctionsConstants.Value8, SettingsDescriptionFunctionsConstants.Value102, SettingsDescriptionFunctionsConstants.Value12)),
                Toggle(SettingsConstants.ParallelJoystickAdapter, EmulationMachineTab.Controllers,
                    SettingsDescriptionFunctionsConstants.ControllerBehavior, SettingsDescriptionFunctionsConstants.ResourceAmigaControllerParallelAdapter,
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
