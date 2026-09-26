using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    private static readonly IReadOnlyDictionary<string, string> FieldHelpResources =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SettingsConstants.OptionFpuModel] = "Emulation.Help.Cpu.FpuModel",
            [SettingsConstants.OptionCpuCompatibility] = "Emulation.Help.Cpu.Precision",
            [SettingsConstants.CpuSpeed] = "Emulation.Help.Cpu.Speed",
            [SettingsConstants.OptionBogomemSize] = "Emulation.Help.Memory.Slow",
            [SettingsConstants.OptionFastmemSize] = "Emulation.Help.Memory.Fast",
            [SettingsConstants.OptionZ3memSize] = "Emulation.Help.Memory.Z3",
            [SettingsConstants.ExtendedRomPath] = "Emulation.Help.Firmware.ExtendedRom",
            [SettingsConstants.RomKeyPath] = "Emulation.Help.Firmware.RomKey",
            [SettingsConstants.OptionVideoStandard] = "Emulation.Help.Video.Standard",
            [SettingsConstants.OptionVideoAspect] = "Emulation.Help.Video.AspectRatio",
            [SettingsConstants.OptionVideoVresolution] = "Emulation.Help.Video.LineMode",
            [SettingsConstants.OptionVideoAllowHzChange] = "Emulation.Help.Video.HzChange",
            [SettingsConstants.OptionGfxFramerate] = "Emulation.Help.Video.FrameSkip",
            [SettingsConstants.OptionGfxColors] = "Emulation.Help.Video.Colors",
            [SettingsConstants.OptionGfxGamma] = "Emulation.Help.Video.Gamma",
            [SettingsConstants.OptionImmediateBlits] = "Emulation.Help.Video.ImmediateBlits",
            [SettingsConstants.OptionCollisionLevel] = "Emulation.Help.Video.CollisionLevel",
            [SettingsConstants.OptionGfxFlickerfixer] = "Emulation.Help.Video.FlickerFixer",
            [SettingsConstants.AudioLatency] = "Emulation.Help.Audio.Latency",
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
            [SettingsConstants.OptionTurboPulse] = "Emulation.Help.Controller.TurboPulse",
            [SettingsConstants.ParallelJoystickAdapter] = "Emulation.Help.Controller.ParallelAdapter"
        };

    internal static IReadOnlyList<EmulationSettingsBlock> Create(Model model,
        MachineConfiguration configuration)
    {
        var options = configuration.Options ?? new Dictionary<string, string>();
        var cpu = Value(options, SettingsConstants.OptionCpuModel, model.DefaultCpu);
        var compatibility = Value(options, SettingsConstants.OptionCpuCompatibility, SettingsDescriptionFunctionsConstants.Exact);
        var ntsc = Value(options, SettingsConstants.OptionVideoStandard, SettingsDescriptionFunctionsConstants.PAL)
            .StartsWith(SettingsDescriptionFunctionsConstants.NTSC, StringComparison.OrdinalIgnoreCase);
        var frequencies = CpuFrequencyChoices(model, compatibility, ntsc);
        var frequency = CpuFrequencyValue(options, compatibility, frequencies);
        return
        [
            Block(SettingsDescriptionFunctionsConstants.Cpu, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.ResourceCpuProcessor, SettingsDescriptionFunctionsConstants.Value, 2,
                Select(SettingsConstants.OptionCpuModel, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Cpu, SettingsDescriptionFunctionsConstants.ResourceCpuModel,
                    cpu, model.CpuModels.Select(CpuChoice), model.CpuModels.Count > 1,
                    refreshSettingsOnChange: true),
                Select(SettingsConstants.OptionFpuModel, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Cpu, SettingsDescriptionFunctionsConstants.ResourceFpuModel,
                    Value(options, SettingsConstants.OptionFpuModel, DefaultFpu(cpu)), FpuValues(cpu).Select(FpuChoice)),
                Select(SettingsConstants.OptionCpuCompatibility, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Cpu, SettingsDescriptionFunctionsConstants.ResourceCpuPrecision,
                    compatibility, CompatibilityChoices(), refreshSettingsOnChange: true),
                Information(SettingsConstants.CpuOriginalSpeed, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Cpu,
                    SettingsDescriptionFunctionsConstants.ResourceCpuSpeedOriginal, FormatMhz(NominalCpuFrequencyMhz(model, ntsc))),
                Select(SettingsConstants.CpuSpeed, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Cpu, SettingsDescriptionFunctionsConstants.ResourceCpuSpeed,
                    frequency, frequencies)),
            Block(SettingsDescriptionFunctionsConstants.MainMemory, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.ResourceMemoryMain, SettingsDescriptionFunctionsConstants.Value2, 2,
                Select(SettingsConstants.OptionChipmemSize, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.MainMemory, SettingsDescriptionFunctionsConstants.ResourceMemoryMain,
                    Value(options, SettingsConstants.OptionChipmemSize, ChipMemoryValue(model.ChipMemoryKib)),
                    ChipMemoryValues(model).Select(ChipMemoryChoice)),
                Select(SettingsConstants.OptionBogomemSize, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.MainMemory, SettingsDescriptionFunctionsConstants.ResourceMemorySlow,
                    Value(options, SettingsConstants.OptionBogomemSize, SlowMemoryValue(model.SlowMemoryKib)),
                    SlowMemoryValues(model).Select(SlowMemoryChoice))),
            Block(SettingsDescriptionFunctionsConstants.ExtensionMemory, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.ResourceMemoryExtensions, SettingsDescriptionFunctionsConstants.Value2, 2,
                Select(SettingsConstants.OptionFastmemSize, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.ExtensionMemory, SettingsDescriptionFunctionsConstants.ResourceMemoryFast,
                    Value(options, SettingsConstants.OptionFastmemSize, model.FastMemoryMib.ToString()),
                    new[] { SettingsDescriptionFunctionsConstants.Value0, SettingsDescriptionFunctionsConstants.Value1, SettingsDescriptionFunctionsConstants.Value22, SettingsDescriptionFunctionsConstants.Value4, SettingsDescriptionFunctionsConstants.Value8 }.Select(MemoryMibChoice)),
                Select(SettingsConstants.OptionZ3memSize, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.ExtensionMemory, SettingsDescriptionFunctionsConstants.ResourceMemoryZ3,
                    Value(options, SettingsConstants.OptionZ3memSize, SettingsDescriptionFunctionsConstants.Value0), model.Id is SettingsDescriptionFunctionsConstants.A3000 or SettingsDescriptionFunctionsConstants.A4000
                        ? new[] { SettingsDescriptionFunctionsConstants.Value0, SettingsDescriptionFunctionsConstants.Value1, SettingsDescriptionFunctionsConstants.Value22, SettingsDescriptionFunctionsConstants.Value4, SettingsDescriptionFunctionsConstants.Value8, SettingsDescriptionFunctionsConstants.Value16, SettingsDescriptionFunctionsConstants.Value32, SettingsDescriptionFunctionsConstants.Value64, SettingsDescriptionFunctionsConstants.Value128, SettingsDescriptionFunctionsConstants.Value256, SettingsDescriptionFunctionsConstants.Value512 }
                            .Select(MemoryMibChoice)
                        : new[] { SettingsDescriptionFunctionsConstants.Value0 }.Select(MemoryMibChoice))),
            Block(SettingsDescriptionFunctionsConstants.Firmware, EmulationMachineTab.Rom, SettingsDescriptionFunctionsConstants.ResourceFirmwareRomSystem, SettingsDescriptionFunctionsConstants.Value3, 1,
                Path(SettingsConstants.KickstartPath, SettingsDescriptionFunctionsConstants.ResourceFirmwareRomKickstart, configuration.KickstartPath),
                Path(SettingsConstants.ExtendedRomPath, SettingsDescriptionFunctionsConstants.ResourceFirmwareRomExtended, configuration.ExtendedRomPath),
                Path(SettingsConstants.RomKeyPath, SettingsDescriptionFunctionsConstants.ResourceFirmwareRomKey, configuration.RomKeyPath)),
            Block(SettingsDescriptionFunctionsConstants.Display, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.ResourceVideoSettingsDisplay, SettingsDescriptionFunctionsConstants.Value5, 2,
                Select(SettingsConstants.OptionVideoStandard, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoStandard,
                    Value(options, SettingsConstants.OptionVideoStandard, SettingsDescriptionFunctionsConstants.PAL), VideoStandardChoices(),
                    refreshSettingsOnChange: true),
                Select(SettingsConstants.OptionVideoResolution, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoResolution,
                    Value(options, SettingsConstants.OptionVideoResolution, SettingsDescriptionFunctionsConstants.Auto), VideoResolutionChoices()),
                Select(SettingsConstants.OptionVideoAspect, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoAspectRatio,
                    Value(options, SettingsConstants.OptionVideoAspect, SettingsDescriptionFunctionsConstants.Auto), VideoAspectChoices()),
                Select(SettingsConstants.OptionCrop, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoCrop,
                    Value(options, SettingsConstants.OptionCrop, SettingsDescriptionFunctionsConstants.Disabled), CropChoices()),
                Select(SettingsConstants.OptionVideoVresolution, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoLineMode,
                    Value(options, SettingsConstants.OptionVideoVresolution, SettingsDescriptionFunctionsConstants.Auto), LineModeChoices()),
                Select(SettingsConstants.OptionVideoAllowHzChange, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoHzChange,
                    Value(options, SettingsConstants.OptionVideoAllowHzChange, SettingsDescriptionFunctionsConstants.Locked), HzChangeChoices()),
                Select(SettingsConstants.OptionGfxFramerate, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoFrameSkip,
                    Value(options, SettingsConstants.OptionGfxFramerate, SettingsDescriptionFunctionsConstants.Disabled), FrameSkipChoices()),
                Select(SettingsConstants.OptionGfxColors, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoColors,
                    Value(options, SettingsConstants.OptionGfxColors, SettingsDescriptionFunctionsConstants.Value24bit), InvariantChoices(SettingsDescriptionFunctionsConstants.Value16bit, SettingsDescriptionFunctionsConstants.Value24bit)),
                Select(SettingsConstants.OptionGfxGamma, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoGamma,
                    Value(options, SettingsConstants.OptionGfxGamma, SettingsDescriptionFunctionsConstants.Value0), Enumerable.Range(-5, 11)
                        .Select(value => Invariant((value * 100).ToString(), value.ToString()))),
                Select(SettingsConstants.OptionImmediateBlits, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceStateImmediateBlits,
                    Value(options, SettingsConstants.OptionImmediateBlits, SettingsDescriptionFunctionsConstants.False), ImmediateBlitChoices()),
                Select(SettingsConstants.OptionCollisionLevel, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display, SettingsDescriptionFunctionsConstants.ResourceVideoCollisionLevel,
                    Value(options, SettingsConstants.OptionCollisionLevel, SettingsDescriptionFunctionsConstants.Playfields), CollisionChoices()),
                Toggle(SettingsConstants.OptionGfxFlickerfixer, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Display,
                    SettingsDescriptionFunctionsConstants.ResourceVideoFlickerFixer, Value(options, SettingsConstants.OptionGfxFlickerfixer, SettingsDescriptionFunctionsConstants.Disabled) == SettingsDescriptionFunctionsConstants.Enabled)),
            Block(SettingsDescriptionFunctionsConstants.Audio, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.ResourceAudio, SettingsDescriptionFunctionsConstants.Value6, 2,
                Toggle(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudioEnabled, configuration.AudioEnabled),
                AudioOutput(configuration.Audio?.OutputDeviceId),
                Select(SettingsConstants.AudioLatency, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudioLatencyLabel, (configuration.Audio?.LatencyMilliseconds ?? 50).ToString(),
                    new[] { 20, 35, 50, 75, 100, 150, 250 }.Select(value =>
                        Invariant(value.ToString(), $"{value} ms"))),
                Select(SettingsConstants.OptionSoundInterpol, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio, SettingsDescriptionFunctionsConstants.ResourceAudioInterpolation,
                    Value(options, SettingsConstants.OptionSoundInterpol, configuration.Audio?.Interpolation ?? SettingsDescriptionFunctionsConstants.Anti),
                    AudioInterpolationChoices()),
                Select(SettingsConstants.OptionSoundFilter, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio, SettingsDescriptionFunctionsConstants.ResourceAudioFilter,
                    Value(options, SettingsConstants.OptionSoundFilter, configuration.Audio?.Filter ?? SettingsDescriptionFunctionsConstants.Emulated),
                    AudioFilterChoices()),
                Select(SettingsConstants.OptionSoundFilterType, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio, SettingsDescriptionFunctionsConstants.ResourceAudioFilterType,
                    Value(options, SettingsConstants.OptionSoundFilterType, SettingsDescriptionFunctionsConstants.Auto), FilterTypeChoices()),
                Select(SettingsConstants.AudioStereoSeparation, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudioStereoSeparation, $"{configuration.Audio?.StereoSeparation ?? 100}",
                    PercentageChoices(0, 100, 10)),
                Select(SettingsConstants.OptionFloppySound, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio, SettingsDescriptionFunctionsConstants.ResourceAudioFloppySound,
                    Value(options, SettingsConstants.OptionFloppySound, SettingsDescriptionFunctionsConstants.Value80), PercentageChoices(0, 100, 5)),
                Select(SettingsConstants.OptionFloppySoundType, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudioFloppySoundType, Value(options, SettingsConstants.OptionFloppySoundType, SettingsDescriptionFunctionsConstants.Internal),
                    [new(SettingsDescriptionFunctionsConstants.Internal, SettingsDescriptionFunctionsConstants.ResourceValueInternal), Invariant(SettingsDescriptionFunctionsConstants.A500, SettingsDescriptionFunctionsConstants.A500),
                        new(SettingsDescriptionFunctionsConstants.LOUD, SettingsDescriptionFunctionsConstants.ResourceValueLoud)]),
                Toggle(SettingsConstants.OptionFloppySoundEmptyMute, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio,
                    SettingsDescriptionFunctionsConstants.ResourceAudioFloppyMuteEmpty,
                    Value(options, SettingsConstants.OptionFloppySoundEmptyMute, SettingsDescriptionFunctionsConstants.Enabled) == SettingsDescriptionFunctionsConstants.Enabled),
                Select(SettingsConstants.OptionSoundVolumeCd, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio, SettingsDescriptionFunctionsConstants.ResourceAudioCdVolume,
                    Value(options, SettingsConstants.OptionSoundVolumeCd, SettingsDescriptionFunctionsConstants.Value100).TrimEnd('%'), PercentageChoices(0, 100, 5))),
            Block(SettingsDescriptionFunctionsConstants.Mouse, EmulationMachineTab.Mouse, SettingsDescriptionFunctionsConstants.ResourceTabMouse, SettingsDescriptionFunctionsConstants.Value7, 2,
                Number(SettingsConstants.OptionMouseSpeed, EmulationMachineTab.Mouse, SettingsDescriptionFunctionsConstants.Mouse, SettingsDescriptionFunctionsConstants.ResourceMouseSpeed,
                    Value(options, SettingsConstants.OptionMouseSpeed, SettingsDescriptionFunctionsConstants.Value1002)),
                Select(SettingsConstants.OptionAnalogmouse, EmulationMachineTab.Mouse, SettingsDescriptionFunctionsConstants.Mouse, SettingsDescriptionFunctionsConstants.ResourceMouseAnalog,
                    Value(options, SettingsConstants.OptionAnalogmouse, SettingsDescriptionFunctionsConstants.Both), AnalogMouseChoices()),
                Select(SettingsConstants.OptionAnalogmouseDeadzone, EmulationMachineTab.Mouse, SettingsDescriptionFunctionsConstants.Mouse,
                    SettingsDescriptionFunctionsConstants.ResourceMouseAnalogDeadzone, Value(options, SettingsConstants.OptionAnalogmouseDeadzone, SettingsDescriptionFunctionsConstants.Value15),
                    PercentageChoices(0, 50, 5)),
                Select(SettingsConstants.OptionAnalogmouseSpeed, EmulationMachineTab.Mouse, SettingsDescriptionFunctionsConstants.Mouse,
                    SettingsDescriptionFunctionsConstants.ResourceMouseAnalogSpeed, Value(options, SettingsConstants.OptionAnalogmouseSpeed, SettingsDescriptionFunctionsConstants.Value10),
                    RatioChoices()),
                Select(SettingsConstants.OptionAnalogmouseSpeedRight, EmulationMachineTab.Mouse, SettingsDescriptionFunctionsConstants.Mouse,
                    SettingsDescriptionFunctionsConstants.ResourceMouseAnalogSpeed, Value(options, SettingsConstants.OptionAnalogmouseSpeedRight, SettingsDescriptionFunctionsConstants.Value10),
                    RatioChoices())),
            Block(SettingsDescriptionFunctionsConstants.ControllerBehavior, EmulationMachineTab.Controllers,
                SettingsDescriptionFunctionsConstants.ResourceControllerActionTurboFire, SettingsDescriptionFunctionsConstants.Value9, 2,
                Select(SettingsConstants.OptionTurboPulse, EmulationMachineTab.Controllers, SettingsDescriptionFunctionsConstants.ControllerBehavior,
                    SettingsDescriptionFunctionsConstants.ResourceControllerTurboPulse, Value(options, SettingsConstants.OptionTurboPulse, SettingsDescriptionFunctionsConstants.Value62),
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
