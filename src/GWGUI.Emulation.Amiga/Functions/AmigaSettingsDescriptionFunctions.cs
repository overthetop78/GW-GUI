using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Functions;

internal static class AmigaSettingsDescriptionFunctions
{
    private static readonly IReadOnlyDictionary<string, string> FieldHelpResources =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [AmigaSettingsDescriptionFunctionsConstants.OptionFpuModel] = "Emulation.Help.Cpu.FpuModel",
            [AmigaSettingsDescriptionFunctionsConstants.OptionCpuCompatibility] = "Emulation.Help.Cpu.Precision",
            [AmigaSettingsConstants.CpuSpeed] = "Emulation.Help.Cpu.Speed",
            [AmigaSettingsDescriptionFunctionsConstants.OptionBogomemSize] = "Emulation.Help.Memory.Slow",
            [AmigaSettingsDescriptionFunctionsConstants.OptionFastmemSize] = "Emulation.Help.Memory.Fast",
            [AmigaSettingsDescriptionFunctionsConstants.OptionZ3memSize] = "Emulation.Help.Memory.Z3",
            [AmigaSettingsConstants.ExtendedRomPath] = "Emulation.Help.Firmware.ExtendedRom",
            [AmigaSettingsConstants.RomKeyPath] = "Emulation.Help.Firmware.RomKey",
            [AmigaSettingsDescriptionFunctionsConstants.OptionVideoStandard] = "Emulation.Help.Video.Standard",
            [AmigaSettingsDescriptionFunctionsConstants.OptionVideoAspect] = "Emulation.Help.Video.AspectRatio",
            [AmigaSettingsDescriptionFunctionsConstants.OptionVideoVresolution] = "Emulation.Help.Video.LineMode",
            [AmigaSettingsDescriptionFunctionsConstants.OptionVideoAllowHzChange] = "Emulation.Help.Video.HzChange",
            [AmigaSettingsDescriptionFunctionsConstants.OptionGfxFramerate] = "Emulation.Help.Video.FrameSkip",
            [AmigaSettingsDescriptionFunctionsConstants.OptionGfxColors] = "Emulation.Help.Video.Colors",
            [AmigaSettingsDescriptionFunctionsConstants.OptionGfxGamma] = "Emulation.Help.Video.Gamma",
            [AmigaSettingsDescriptionFunctionsConstants.OptionImmediateBlits] = "Emulation.Help.Video.ImmediateBlits",
            [AmigaSettingsDescriptionFunctionsConstants.OptionCollisionLevel] = "Emulation.Help.Video.CollisionLevel",
            [AmigaSettingsDescriptionFunctionsConstants.OptionGfxFlickerfixer] = "Emulation.Help.Video.FlickerFixer",
            [AmigaSettingsConstants.AudioLatency] = "Emulation.Help.Audio.Latency",
            [AmigaSettingsDescriptionFunctionsConstants.OptionSoundInterpol] = "Emulation.Help.Audio.Interpolation",
            [AmigaSettingsDescriptionFunctionsConstants.OptionSoundFilter] = "Emulation.Help.Audio.Filter",
            [AmigaSettingsDescriptionFunctionsConstants.OptionSoundFilterType] = "Emulation.Help.Audio.FilterType",
            [AmigaSettingsConstants.AudioStereoSeparation] = "Emulation.Help.Audio.StereoSeparation",
            [AmigaSettingsDescriptionFunctionsConstants.OptionFloppySoundType] = "Emulation.Help.Audio.Floppy.SoundType",
            [AmigaSettingsDescriptionFunctionsConstants.OptionFloppySoundEmptyMute] = "Emulation.Help.Audio.Floppy.MuteEmpty",
            [AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouse] = "Emulation.Help.Mouse.Analog",
            [AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouseDeadzone] = "Emulation.Help.Mouse.AnalogDeadzone",
            [AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouseSpeed] = "Emulation.Help.Mouse.AnalogSpeed",
            [AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouseSpeedRight] = "Emulation.Help.Mouse.AnalogSpeed",
            [AmigaSettingsDescriptionFunctionsConstants.OptionTurboPulse] = "Emulation.Help.Controller.TurboPulse",
            [AmigaSettingsConstants.ParallelJoystickAdapter] = "Emulation.Help.Controller.ParallelAdapter"
        };

    internal static IReadOnlyList<EmulationSettingsBlock> Create(AmigaModel model,
        AmigaMachineConfiguration configuration)
    {
        var options = configuration.Options ?? new Dictionary<string, string>();
        var cpu = Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionCpuModel, model.DefaultCpu);
        var compatibility = Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionCpuCompatibility, AmigaSettingsDescriptionFunctionsConstants.Exact);
        var ntsc = Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionVideoStandard, AmigaSettingsDescriptionFunctionsConstants.PAL)
            .StartsWith(AmigaSettingsDescriptionFunctionsConstants.NTSC, StringComparison.OrdinalIgnoreCase);
        var frequencies = CpuFrequencyChoices(model, compatibility, ntsc);
        var frequency = CpuFrequencyValue(options, compatibility, frequencies);
        return
        [
            Block(AmigaSettingsDescriptionFunctionsConstants.Cpu, EmulationMachineTab.Cpu, AmigaSettingsDescriptionFunctionsConstants.ResourceCpuProcessor, AmigaSettingsDescriptionFunctionsConstants.Value, 2,
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionCpuModel, EmulationMachineTab.Cpu, AmigaSettingsDescriptionFunctionsConstants.Cpu, AmigaSettingsDescriptionFunctionsConstants.ResourceCpuModel,
                    cpu, model.CpuModels.Select(CpuChoice), model.CpuModels.Count > 1,
                    refreshSettingsOnChange: true),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionFpuModel, EmulationMachineTab.Cpu, AmigaSettingsDescriptionFunctionsConstants.Cpu, AmigaSettingsDescriptionFunctionsConstants.ResourceFpuModel,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionFpuModel, DefaultFpu(cpu)), FpuValues(cpu).Select(FpuChoice)),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionCpuCompatibility, EmulationMachineTab.Cpu, AmigaSettingsDescriptionFunctionsConstants.Cpu, AmigaSettingsDescriptionFunctionsConstants.ResourceCpuPrecision,
                    compatibility, CompatibilityChoices(), refreshSettingsOnChange: true),
                Information(AmigaSettingsConstants.CpuOriginalSpeed, EmulationMachineTab.Cpu, AmigaSettingsDescriptionFunctionsConstants.Cpu,
                    AmigaSettingsDescriptionFunctionsConstants.ResourceCpuSpeedOriginal, FormatMhz(NominalCpuFrequencyMhz(model, ntsc))),
                Select(AmigaSettingsConstants.CpuSpeed, EmulationMachineTab.Cpu, AmigaSettingsDescriptionFunctionsConstants.Cpu, AmigaSettingsDescriptionFunctionsConstants.ResourceCpuSpeed,
                    frequency, frequencies)),
            Block(AmigaSettingsDescriptionFunctionsConstants.MainMemory, EmulationMachineTab.Ram, AmigaSettingsDescriptionFunctionsConstants.ResourceMemoryMain, AmigaSettingsDescriptionFunctionsConstants.Value2, 2,
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionChipmemSize, EmulationMachineTab.Ram, AmigaSettingsDescriptionFunctionsConstants.MainMemory, AmigaSettingsDescriptionFunctionsConstants.ResourceMemoryMain,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionChipmemSize, ChipMemoryValue(model.ChipMemoryKib)),
                    ChipMemoryValues(model).Select(ChipMemoryChoice)),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionBogomemSize, EmulationMachineTab.Ram, AmigaSettingsDescriptionFunctionsConstants.MainMemory, AmigaSettingsDescriptionFunctionsConstants.ResourceMemorySlow,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionBogomemSize, SlowMemoryValue(model.SlowMemoryKib)),
                    SlowMemoryValues(model).Select(SlowMemoryChoice))),
            Block(AmigaSettingsDescriptionFunctionsConstants.ExtensionMemory, EmulationMachineTab.Ram, AmigaSettingsDescriptionFunctionsConstants.ResourceMemoryExtensions, AmigaSettingsDescriptionFunctionsConstants.Value2, 2,
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionFastmemSize, EmulationMachineTab.Ram, AmigaSettingsDescriptionFunctionsConstants.ExtensionMemory, AmigaSettingsDescriptionFunctionsConstants.ResourceMemoryFast,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionFastmemSize, model.FastMemoryMib.ToString()),
                    new[] { AmigaSettingsDescriptionFunctionsConstants.Value0, AmigaSettingsDescriptionFunctionsConstants.Value1, AmigaSettingsDescriptionFunctionsConstants.Value22, AmigaSettingsDescriptionFunctionsConstants.Value4, AmigaSettingsDescriptionFunctionsConstants.Value8 }.Select(MemoryMibChoice)),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionZ3memSize, EmulationMachineTab.Ram, AmigaSettingsDescriptionFunctionsConstants.ExtensionMemory, AmigaSettingsDescriptionFunctionsConstants.ResourceMemoryZ3,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionZ3memSize, AmigaSettingsDescriptionFunctionsConstants.Value0), model.Id is AmigaSettingsDescriptionFunctionsConstants.A3000 or AmigaSettingsDescriptionFunctionsConstants.A4000
                        ? new[] { AmigaSettingsDescriptionFunctionsConstants.Value0, AmigaSettingsDescriptionFunctionsConstants.Value1, AmigaSettingsDescriptionFunctionsConstants.Value22, AmigaSettingsDescriptionFunctionsConstants.Value4, AmigaSettingsDescriptionFunctionsConstants.Value8, AmigaSettingsDescriptionFunctionsConstants.Value16, AmigaSettingsDescriptionFunctionsConstants.Value32, AmigaSettingsDescriptionFunctionsConstants.Value64, AmigaSettingsDescriptionFunctionsConstants.Value128, AmigaSettingsDescriptionFunctionsConstants.Value256, AmigaSettingsDescriptionFunctionsConstants.Value512 }
                            .Select(MemoryMibChoice)
                        : new[] { AmigaSettingsDescriptionFunctionsConstants.Value0 }.Select(MemoryMibChoice))),
            Block(AmigaSettingsDescriptionFunctionsConstants.Firmware, EmulationMachineTab.Rom, AmigaSettingsDescriptionFunctionsConstants.ResourceFirmwareRomSystem, AmigaSettingsDescriptionFunctionsConstants.Value3, 1,
                Path(AmigaSettingsConstants.KickstartPath, AmigaSettingsDescriptionFunctionsConstants.ResourceFirmwareRomKickstart, configuration.KickstartPath),
                Path(AmigaSettingsConstants.ExtendedRomPath, AmigaSettingsDescriptionFunctionsConstants.ResourceFirmwareRomExtended, configuration.ExtendedRomPath),
                Path(AmigaSettingsConstants.RomKeyPath, AmigaSettingsDescriptionFunctionsConstants.ResourceFirmwareRomKey, configuration.RomKeyPath)),
            Block(AmigaSettingsDescriptionFunctionsConstants.Display, EmulationMachineTab.Video, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoSettingsDisplay, AmigaSettingsDescriptionFunctionsConstants.Value5, 2,
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionVideoStandard, EmulationMachineTab.Video, AmigaSettingsDescriptionFunctionsConstants.Display, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoStandard,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionVideoStandard, AmigaSettingsDescriptionFunctionsConstants.PAL), VideoStandardChoices(),
                    refreshSettingsOnChange: true),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionVideoResolution, EmulationMachineTab.Video, AmigaSettingsDescriptionFunctionsConstants.Display, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoResolution,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionVideoResolution, AmigaSettingsDescriptionFunctionsConstants.Auto), VideoResolutionChoices()),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionVideoAspect, EmulationMachineTab.Video, AmigaSettingsDescriptionFunctionsConstants.Display, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoAspectRatio,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionVideoAspect, AmigaSettingsDescriptionFunctionsConstants.Auto), VideoAspectChoices()),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionCrop, EmulationMachineTab.Video, AmigaSettingsDescriptionFunctionsConstants.Display, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoCrop,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionCrop, AmigaSettingsDescriptionFunctionsConstants.Disabled), CropChoices()),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionVideoVresolution, EmulationMachineTab.Video, AmigaSettingsDescriptionFunctionsConstants.Display, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoLineMode,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionVideoVresolution, AmigaSettingsDescriptionFunctionsConstants.Auto), LineModeChoices()),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionVideoAllowHzChange, EmulationMachineTab.Video, AmigaSettingsDescriptionFunctionsConstants.Display, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoHzChange,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionVideoAllowHzChange, AmigaSettingsDescriptionFunctionsConstants.Locked), HzChangeChoices()),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionGfxFramerate, EmulationMachineTab.Video, AmigaSettingsDescriptionFunctionsConstants.Display, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoFrameSkip,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionGfxFramerate, AmigaSettingsDescriptionFunctionsConstants.Disabled), FrameSkipChoices()),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionGfxColors, EmulationMachineTab.Video, AmigaSettingsDescriptionFunctionsConstants.Display, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoColors,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionGfxColors, AmigaSettingsDescriptionFunctionsConstants.Value24bit), InvariantChoices(AmigaSettingsDescriptionFunctionsConstants.Value16bit, AmigaSettingsDescriptionFunctionsConstants.Value24bit)),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionGfxGamma, EmulationMachineTab.Video, AmigaSettingsDescriptionFunctionsConstants.Display, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoGamma,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionGfxGamma, AmigaSettingsDescriptionFunctionsConstants.Value0), Enumerable.Range(-5, 11)
                        .Select(value => Invariant((value * 100).ToString(), value.ToString()))),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionImmediateBlits, EmulationMachineTab.Video, AmigaSettingsDescriptionFunctionsConstants.Display, AmigaSettingsDescriptionFunctionsConstants.ResourceStateImmediateBlits,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionImmediateBlits, AmigaSettingsDescriptionFunctionsConstants.False), ImmediateBlitChoices()),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionCollisionLevel, EmulationMachineTab.Video, AmigaSettingsDescriptionFunctionsConstants.Display, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoCollisionLevel,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionCollisionLevel, AmigaSettingsDescriptionFunctionsConstants.Playfields), CollisionChoices()),
                Toggle(AmigaSettingsDescriptionFunctionsConstants.OptionGfxFlickerfixer, EmulationMachineTab.Video, AmigaSettingsDescriptionFunctionsConstants.Display,
                    AmigaSettingsDescriptionFunctionsConstants.ResourceVideoFlickerFixer, Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionGfxFlickerfixer, AmigaSettingsDescriptionFunctionsConstants.Disabled) == AmigaSettingsDescriptionFunctionsConstants.Enabled)),
            Block(AmigaSettingsDescriptionFunctionsConstants.Audio, EmulationMachineTab.Audio, AmigaSettingsDescriptionFunctionsConstants.ResourceAudio, AmigaSettingsDescriptionFunctionsConstants.Value6, 2,
                Toggle(AmigaSettingsConstants.AudioEnabled, EmulationMachineTab.Audio, AmigaSettingsDescriptionFunctionsConstants.Audio,
                    AmigaSettingsDescriptionFunctionsConstants.ResourceAudioEnabled, configuration.AudioEnabled),
                AudioOutput(configuration.Audio?.OutputDeviceId),
                Select(AmigaSettingsConstants.AudioLatency, EmulationMachineTab.Audio, AmigaSettingsDescriptionFunctionsConstants.Audio,
                    AmigaSettingsDescriptionFunctionsConstants.ResourceAudioLatencyLabel, (configuration.Audio?.LatencyMilliseconds ?? 50).ToString(),
                    new[] { 20, 35, 50, 75, 100, 150, 250 }.Select(value =>
                        Invariant(value.ToString(), $"{value} ms"))),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionSoundInterpol, EmulationMachineTab.Audio, AmigaSettingsDescriptionFunctionsConstants.Audio, AmigaSettingsDescriptionFunctionsConstants.ResourceAudioInterpolation,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionSoundInterpol, configuration.Audio?.Interpolation ?? AmigaSettingsDescriptionFunctionsConstants.Anti),
                    AudioInterpolationChoices()),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionSoundFilter, EmulationMachineTab.Audio, AmigaSettingsDescriptionFunctionsConstants.Audio, AmigaSettingsDescriptionFunctionsConstants.ResourceAudioFilter,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionSoundFilter, configuration.Audio?.Filter ?? AmigaSettingsDescriptionFunctionsConstants.Emulated),
                    AudioFilterChoices()),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionSoundFilterType, EmulationMachineTab.Audio, AmigaSettingsDescriptionFunctionsConstants.Audio, AmigaSettingsDescriptionFunctionsConstants.ResourceAudioFilterType,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionSoundFilterType, AmigaSettingsDescriptionFunctionsConstants.Auto), FilterTypeChoices()),
                Select(AmigaSettingsConstants.AudioStereoSeparation, EmulationMachineTab.Audio, AmigaSettingsDescriptionFunctionsConstants.Audio,
                    AmigaSettingsDescriptionFunctionsConstants.ResourceAudioStereoSeparation, $"{configuration.Audio?.StereoSeparation ?? 100}",
                    PercentageChoices(0, 100, 10)),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionFloppySound, EmulationMachineTab.Audio, AmigaSettingsDescriptionFunctionsConstants.Audio, AmigaSettingsDescriptionFunctionsConstants.ResourceAudioFloppySound,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionFloppySound, AmigaSettingsDescriptionFunctionsConstants.Value80), PercentageChoices(0, 100, 5)),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionFloppySoundType, EmulationMachineTab.Audio, AmigaSettingsDescriptionFunctionsConstants.Audio,
                    AmigaSettingsDescriptionFunctionsConstants.ResourceAudioFloppySoundType, Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionFloppySoundType, AmigaSettingsDescriptionFunctionsConstants.Internal),
                    [new(AmigaSettingsDescriptionFunctionsConstants.Internal, AmigaSettingsDescriptionFunctionsConstants.ResourceValueInternal), Invariant(AmigaSettingsDescriptionFunctionsConstants.A500, AmigaSettingsDescriptionFunctionsConstants.A500),
                        new(AmigaSettingsDescriptionFunctionsConstants.LOUD, AmigaSettingsDescriptionFunctionsConstants.ResourceValueLoud)]),
                Toggle(AmigaSettingsDescriptionFunctionsConstants.OptionFloppySoundEmptyMute, EmulationMachineTab.Audio, AmigaSettingsDescriptionFunctionsConstants.Audio,
                    AmigaSettingsDescriptionFunctionsConstants.ResourceAudioFloppyMuteEmpty,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionFloppySoundEmptyMute, AmigaSettingsDescriptionFunctionsConstants.Enabled) == AmigaSettingsDescriptionFunctionsConstants.Enabled),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionSoundVolumeCd, EmulationMachineTab.Audio, AmigaSettingsDescriptionFunctionsConstants.Audio, AmigaSettingsDescriptionFunctionsConstants.ResourceAudioCdVolume,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionSoundVolumeCd, AmigaSettingsDescriptionFunctionsConstants.Value100).TrimEnd('%'), PercentageChoices(0, 100, 5))),
            Block(AmigaSettingsDescriptionFunctionsConstants.Mouse, EmulationMachineTab.Mouse, AmigaSettingsDescriptionFunctionsConstants.ResourceTabMouse, AmigaSettingsDescriptionFunctionsConstants.Value7, 2,
                Number(AmigaSettingsDescriptionFunctionsConstants.OptionMouseSpeed, EmulationMachineTab.Mouse, AmigaSettingsDescriptionFunctionsConstants.Mouse, AmigaSettingsDescriptionFunctionsConstants.ResourceMouseSpeed,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionMouseSpeed, AmigaSettingsDescriptionFunctionsConstants.Value1002)),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouse, EmulationMachineTab.Mouse, AmigaSettingsDescriptionFunctionsConstants.Mouse, AmigaSettingsDescriptionFunctionsConstants.ResourceMouseAnalog,
                    Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouse, AmigaSettingsDescriptionFunctionsConstants.Both), AnalogMouseChoices()),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouseDeadzone, EmulationMachineTab.Mouse, AmigaSettingsDescriptionFunctionsConstants.Mouse,
                    AmigaSettingsDescriptionFunctionsConstants.ResourceMouseAnalogDeadzone, Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouseDeadzone, AmigaSettingsDescriptionFunctionsConstants.Value15),
                    PercentageChoices(0, 50, 5)),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouseSpeed, EmulationMachineTab.Mouse, AmigaSettingsDescriptionFunctionsConstants.Mouse,
                    AmigaSettingsDescriptionFunctionsConstants.ResourceMouseAnalogSpeed, Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouseSpeed, AmigaSettingsDescriptionFunctionsConstants.Value10),
                    RatioChoices()),
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouseSpeedRight, EmulationMachineTab.Mouse, AmigaSettingsDescriptionFunctionsConstants.Mouse,
                    AmigaSettingsDescriptionFunctionsConstants.ResourceMouseAnalogSpeed, Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouseSpeedRight, AmigaSettingsDescriptionFunctionsConstants.Value10),
                    RatioChoices())),
            Block(AmigaSettingsDescriptionFunctionsConstants.ControllerBehavior, EmulationMachineTab.Controllers,
                AmigaSettingsDescriptionFunctionsConstants.ResourceControllerActionTurboFire, AmigaSettingsDescriptionFunctionsConstants.Value9, 2,
                Select(AmigaSettingsDescriptionFunctionsConstants.OptionTurboPulse, EmulationMachineTab.Controllers, AmigaSettingsDescriptionFunctionsConstants.ControllerBehavior,
                    AmigaSettingsDescriptionFunctionsConstants.ResourceControllerTurboPulse, Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionTurboPulse, AmigaSettingsDescriptionFunctionsConstants.Value62),
                    InvariantChoices(AmigaSettingsDescriptionFunctionsConstants.Value22, AmigaSettingsDescriptionFunctionsConstants.Value4, AmigaSettingsDescriptionFunctionsConstants.Value62, AmigaSettingsDescriptionFunctionsConstants.Value8, AmigaSettingsDescriptionFunctionsConstants.Value102, AmigaSettingsDescriptionFunctionsConstants.Value12)),
                Toggle(AmigaSettingsConstants.ParallelJoystickAdapter, EmulationMachineTab.Controllers,
                    AmigaSettingsDescriptionFunctionsConstants.ControllerBehavior, AmigaSettingsDescriptionFunctionsConstants.ResourceAmigaControllerParallelAdapter,
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

    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block,
        string label, string value) => new(id, tab, block, label, EmulationSettingsEditor.Information, value,
            ExplanationResourceKey: ShortHelp(id), DetailedExplanationResourceKey: DetailedHelp(id));

    private static EmulationSettingsField Toggle(string id, EmulationMachineTab tab, string block,
        string label, bool value, bool refreshSettingsOnChange = false) =>
        new(id, tab, block, label, EmulationSettingsEditor.Toggle,
            value ? AmigaSettingsDescriptionFunctionsConstants.Enabled : AmigaSettingsDescriptionFunctionsConstants.Disabled,
            ExplanationResourceKey: ShortHelp(id), DetailedExplanationResourceKey: DetailedHelp(id),
            RefreshSettingsOnChange: refreshSettingsOnChange);

    private static EmulationSettingsField Number(string id, EmulationMachineTab tab, string block,
        string label, string value) => new(id, tab, block, label, EmulationSettingsEditor.Number, value,
            ExplanationResourceKey: ShortHelp(id), DetailedExplanationResourceKey: DetailedHelp(id));

    private static EmulationSettingsField Path(string id, string label, string? value) =>
        new(id, EmulationMachineTab.Rom, AmigaSettingsDescriptionFunctionsConstants.Firmware, label, EmulationSettingsEditor.Path, value,
            ExplanationResourceKey: ShortHelp(id), DetailedExplanationResourceKey: DetailedHelp(id),
            DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware);

    private static EmulationSettingsField AudioOutput(string? value) => new(
        AmigaSettingsConstants.AudioOutput, EmulationMachineTab.Audio, AmigaSettingsDescriptionFunctionsConstants.Audio, AmigaSettingsDescriptionFunctionsConstants.ResourceAudioDevice,
        EmulationSettingsEditor.Selection, value ?? string.Empty,
        [new EmulationSettingsChoice(string.Empty, AmigaSettingsDescriptionFunctionsConstants.ResourceAudioDefaultOutput)],
        ChoiceSource: EmulationSettingsChoiceSource.AudioOutputDevices);

    private static string? ShortHelp(string id) => FieldHelpResources.TryGetValue(id, out var resource)
        ? resource + ".Short" : null;

    private static string? DetailedHelp(string id) => FieldHelpResources.TryGetValue(id, out var resource)
        ? resource + ".Detailed" : null;

    private static string Value(IReadOnlyDictionary<string, string> values, string key, string fallback) =>
        values.GetValueOrDefault(key) ?? fallback;

    private static string DefaultFpu(string cpu) => cpu is AmigaSettingsDescriptionFunctionsConstants.Value68040 or AmigaSettingsDescriptionFunctionsConstants.Value68060 ? AmigaSettingsDescriptionFunctionsConstants.Cpu : AmigaSettingsDescriptionFunctionsConstants.Value0;
    private static IReadOnlyList<string> FpuValues(string cpu) => cpu switch
    {
        AmigaSettingsDescriptionFunctionsConstants.Value68000 or AmigaSettingsDescriptionFunctionsConstants.Value68010 => [AmigaSettingsDescriptionFunctionsConstants.Value0],
        AmigaSettingsDescriptionFunctionsConstants.Value68020 or AmigaSettingsDescriptionFunctionsConstants.Value68030 => [AmigaSettingsDescriptionFunctionsConstants.Value0, AmigaSettingsDescriptionFunctionsConstants.Value68881, AmigaSettingsDescriptionFunctionsConstants.Value68882],
        _ => [AmigaSettingsDescriptionFunctionsConstants.Cpu, AmigaSettingsDescriptionFunctionsConstants.Value0, AmigaSettingsDescriptionFunctionsConstants.Value68881, AmigaSettingsDescriptionFunctionsConstants.Value68882]
    };
    private static IReadOnlyList<string> ChipMemoryValues(AmigaModel model) => model.Id switch
    {
        AmigaSettingsDescriptionFunctionsConstants.A1000 => [AmigaSettingsDescriptionFunctionsConstants.Value1], AmigaSettingsDescriptionFunctionsConstants.A500 => [AmigaSettingsDescriptionFunctionsConstants.Value1, AmigaSettingsDescriptionFunctionsConstants.Value22, AmigaSettingsDescriptionFunctionsConstants.Value33, AmigaSettingsDescriptionFunctionsConstants.Value4],
        AmigaSettingsDescriptionFunctionsConstants.A500PLUS or AmigaSettingsDescriptionFunctionsConstants.A600 => [AmigaSettingsDescriptionFunctionsConstants.Value22, AmigaSettingsDescriptionFunctionsConstants.Value4], AmigaSettingsDescriptionFunctionsConstants.A2000 => [AmigaSettingsDescriptionFunctionsConstants.Value1, AmigaSettingsDescriptionFunctionsConstants.Value22, AmigaSettingsDescriptionFunctionsConstants.Value4], _ => [AmigaSettingsDescriptionFunctionsConstants.Value4]
    };
    private static IReadOnlyList<string> SlowMemoryValues(AmigaModel model) => model.Id switch
    {
        AmigaSettingsDescriptionFunctionsConstants.A1000 or AmigaSettingsDescriptionFunctionsConstants.A500 or AmigaSettingsDescriptionFunctionsConstants.A500PLUS or AmigaSettingsDescriptionFunctionsConstants.A2000 => [AmigaSettingsDescriptionFunctionsConstants.Value0, AmigaSettingsDescriptionFunctionsConstants.Value22, AmigaSettingsDescriptionFunctionsConstants.Value4, AmigaSettingsDescriptionFunctionsConstants.Value62, AmigaSettingsDescriptionFunctionsConstants.Value72], _ => [AmigaSettingsDescriptionFunctionsConstants.Value0]
    };
    private static string ChipMemoryValue(int kib) => Math.Clamp(kib / 512, 1, 4).ToString();
    private static string SlowMemoryValue(int kib) => kib switch
    {
        512 => AmigaSettingsDescriptionFunctionsConstants.Value22, 1024 => AmigaSettingsDescriptionFunctionsConstants.Value4, 1536 => AmigaSettingsDescriptionFunctionsConstants.Value62, 1792 => AmigaSettingsDescriptionFunctionsConstants.Value72, _ => AmigaSettingsDescriptionFunctionsConstants.Value0
    };

    private static EmulationSettingsChoice CpuChoice(string value) =>
        new(value, string.Empty, value == AmigaSettingsDescriptionFunctionsConstants.Value68020 ? AmigaSettingsDescriptionFunctionsConstants.Motorola68EC020 : $"Motorola {value}");

    private static EmulationSettingsChoice FpuChoice(string value) => value switch
    {
        AmigaSettingsDescriptionFunctionsConstants.Value0 => new(value, AmigaSettingsDescriptionFunctionsConstants.ResourceMemoryNone),
        AmigaSettingsDescriptionFunctionsConstants.Cpu => new(value, string.Empty, AmigaSettingsDescriptionFunctionsConstants.CPU),
        _ => new(value, string.Empty, value)
    };

    private static IReadOnlyList<EmulationSettingsChoice> CompatibilityChoices() =>
    [
        new(AmigaSettingsDescriptionFunctionsConstants.Normal, AmigaSettingsDescriptionFunctionsConstants.ResourceCpuCompatibilityNormal),
        new(AmigaSettingsDescriptionFunctionsConstants.Compatible, AmigaSettingsDescriptionFunctionsConstants.ResourceCpuCompatibilityCompatible),
        new(AmigaSettingsDescriptionFunctionsConstants.Memory, AmigaSettingsDescriptionFunctionsConstants.ResourceCpuCompatibilityMemory),
        new(AmigaSettingsDescriptionFunctionsConstants.Exact, AmigaSettingsDescriptionFunctionsConstants.ResourceCpuCompatibilityExact)
    ];

    private static EmulationSettingsChoice ChipMemoryChoice(string value)
    {
        var kib = int.TryParse(value, out var units) ? units * 512 : 0;
        return MemoryChoice(value, kib * 1024L);
    }

    private static EmulationSettingsChoice SlowMemoryChoice(string value)
    {
        var kib = value switch { AmigaSettingsDescriptionFunctionsConstants.Value22 => 512, AmigaSettingsDescriptionFunctionsConstants.Value4 => 1024, AmigaSettingsDescriptionFunctionsConstants.Value62 => 1536, AmigaSettingsDescriptionFunctionsConstants.Value72 => 1792, _ => 0 };
        return MemoryChoice(value, kib * 1024L);
    }

    private static EmulationSettingsChoice MemoryMibChoice(string value)
    {
        var mib = int.TryParse(value, out var parsed) ? parsed : 0;
        return MemoryChoice(value, mib * 1024L * 1024L);
    }

    private static EmulationSettingsChoice MemoryChoice(string id, long bytes) => bytes == 0
        ? new EmulationSettingsChoice(id, AmigaSettingsDescriptionFunctionsConstants.ResourceMemoryNone, NumericValue: 0)
        : bytes < 1024L * 1024L
            ? new EmulationSettingsChoice(id, string.Empty, $"{bytes / 1024L} KiB", bytes)
            : new EmulationSettingsChoice(id, string.Empty, $"{bytes / (1024L * 1024L)} MiB", bytes);

    private static IReadOnlyList<EmulationSettingsChoice> CpuFrequencyChoices(AmigaModel model,
        string compatibility, bool ntsc)
    {
        var nominal = NominalCpuFrequencyMhz(model, ntsc);
        if (compatibility is AmigaSettingsDescriptionFunctionsConstants.Memory or AmigaSettingsDescriptionFunctionsConstants.Exact)
        {
            var halfA500Clock = ntsc ? 3.579545d : 3.546895d;
            var choices = new[] { 1, 2, 4, 8, 16 }.Select(multiplier =>
            {
                var frequency = halfA500Clock * multiplier;
                var ratio = frequency / nominal;
                return FrequencyChoice(ratio, AmigaSettingsDescriptionFunctionsConstants.Value00, multiplier.ToString(), frequency);
            }).Where(choice => !Approximately(choice.NumericValue.GetValueOrDefault() / 1_000_000d, nominal))
                .ToList();
            choices.Add(FrequencyChoice(1d, AmigaSettingsDescriptionFunctionsConstants.Value00, AmigaSettingsDescriptionFunctionsConstants.Value0, nominal));
            return choices.OrderBy(choice => choice.NumericValue).ToArray();
        }

        return new[]
        {
            (Ratio: 0.5d, Throttle: AmigaSettingsDescriptionFunctionsConstants.Value5000), (Ratio: 1d, Throttle: AmigaSettingsDescriptionFunctionsConstants.Value00),
            (Ratio: 2d, Throttle: AmigaSettingsDescriptionFunctionsConstants.Value10000), (Ratio: 4d, Throttle: AmigaSettingsDescriptionFunctionsConstants.Value30000),
            (Ratio: 8d, Throttle: AmigaSettingsDescriptionFunctionsConstants.Value70000)
        }.Select(item => FrequencyChoice(item.Ratio, item.Throttle, AmigaSettingsDescriptionFunctionsConstants.Value0, nominal * item.Ratio)).ToArray();
    }

    private static EmulationSettingsChoice FrequencyChoice(double ratio, string throttle, string multiplier,
        double frequency)
    {
        var prefix = Approximately(ratio, 1d) ? AmigaSettingsDescriptionFunctionsConstants.Value1003 : $"{Math.Round(ratio * 100d):0} %";
        return new EmulationSettingsChoice($"{throttle}|{multiplier}", string.Empty,
            $"{prefix} — {FormatMhz(frequency)}", (long)Math.Round(frequency * 1_000_000d));
    }

    private static string CpuFrequencyValue(IReadOnlyDictionary<string, string> options, string compatibility,
        IReadOnlyList<EmulationSettingsChoice> choices)
    {
        var throttle = Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionCpuThrottle, AmigaSettingsDescriptionFunctionsConstants.Value00);
        var multiplier = Value(options, AmigaSettingsDescriptionFunctionsConstants.OptionCpuMultiplier, AmigaSettingsDescriptionFunctionsConstants.Value0);
        var expected = compatibility is AmigaSettingsDescriptionFunctionsConstants.Memory or AmigaSettingsDescriptionFunctionsConstants.Exact ? $"0.0|{multiplier}" : $"{throttle}|0";
        return choices.Any(choice => choice.Id == expected)
            ? expected : choices.FirstOrDefault(choice => choice.Id == AmigaSettingsDescriptionFunctionsConstants.Value000)?.Id ?? choices[0].Id;
    }

    private static double NominalCpuFrequencyMhz(AmigaModel model, bool ntsc) => model.Id switch
    {
        AmigaSettingsDescriptionFunctionsConstants.A1200 or AmigaSettingsDescriptionFunctionsConstants.CD32 => ntsc ? 14.31818d : 14.18758d,
        AmigaSettingsDescriptionFunctionsConstants.A3000 or AmigaSettingsDescriptionFunctionsConstants.A4000 => 25d,
        _ => ntsc ? 7.15909d : 7.09379d
    };

    private static string FormatMhz(double frequency) => $"{frequency:0.00} MHz";
    private static bool Approximately(double left, double right) => Math.Abs(left - right) < 0.0001d;

    private static EmulationSettingsChoice Invariant(string id, string text, long? numericValue = null) =>
        new(id, string.Empty, text, numericValue);

    private static IEnumerable<EmulationSettingsChoice> InvariantChoices(params string[] values) =>
        values.Select(value => Invariant(value, value));

    private static IReadOnlyList<EmulationSettingsChoice> VideoStandardChoices() =>
    [Invariant(AmigaSettingsDescriptionFunctionsConstants.PALAuto, AmigaSettingsDescriptionFunctionsConstants.PALAuto), Invariant(AmigaSettingsDescriptionFunctionsConstants.NTSCAuto, AmigaSettingsDescriptionFunctionsConstants.NTSCAuto),
        Invariant(AmigaSettingsDescriptionFunctionsConstants.PAL, AmigaSettingsDescriptionFunctionsConstants.PAL), Invariant(AmigaSettingsDescriptionFunctionsConstants.NTSC, AmigaSettingsDescriptionFunctionsConstants.NTSC)];

    private static IReadOnlyList<EmulationSettingsChoice> VideoResolutionChoices() =>
    [new(AmigaSettingsDescriptionFunctionsConstants.Auto, AmigaSettingsDescriptionFunctionsConstants.VisualAutomatic), new(AmigaSettingsDescriptionFunctionsConstants.AutoLores, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoResolutionAutoLow),
        new(AmigaSettingsDescriptionFunctionsConstants.AutoSuperhires, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoResolutionAutoSuperHigh),
        new(AmigaSettingsDescriptionFunctionsConstants.Lores, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoResolutionLow), new(AmigaSettingsDescriptionFunctionsConstants.Hires, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoResolutionHigh),
        new(AmigaSettingsDescriptionFunctionsConstants.Superhires, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoResolutionSuperHigh)];

    private static IReadOnlyList<EmulationSettingsChoice> VideoAspectChoices() =>
    [new(AmigaSettingsDescriptionFunctionsConstants.Auto, AmigaSettingsDescriptionFunctionsConstants.VisualAutomatic), Invariant(AmigaSettingsDescriptionFunctionsConstants.PAL, AmigaSettingsDescriptionFunctionsConstants.PAL), Invariant(AmigaSettingsDescriptionFunctionsConstants.NTSC, AmigaSettingsDescriptionFunctionsConstants.NTSC),
        Invariant(AmigaSettingsDescriptionFunctionsConstants.Value11, AmigaSettingsDescriptionFunctionsConstants.Value11)];

    private static IReadOnlyList<EmulationSettingsChoice> CropChoices() =>
    [new(AmigaSettingsDescriptionFunctionsConstants.Disabled, AmigaSettingsDescriptionFunctionsConstants.ResourceValueDisabled), new(AmigaSettingsDescriptionFunctionsConstants.Minimum, AmigaSettingsDescriptionFunctionsConstants.ResourceValueMinimum),
        new(AmigaSettingsDescriptionFunctionsConstants.Smaller, AmigaSettingsDescriptionFunctionsConstants.ResourceValueVerySmall), new(AmigaSettingsDescriptionFunctionsConstants.Small, AmigaSettingsDescriptionFunctionsConstants.ResourceValueSmall),
        new(AmigaSettingsDescriptionFunctionsConstants.Medium, AmigaSettingsDescriptionFunctionsConstants.ResourceValueMedium), new(AmigaSettingsDescriptionFunctionsConstants.Large, AmigaSettingsDescriptionFunctionsConstants.ResourceValueLarge),
        new(AmigaSettingsDescriptionFunctionsConstants.Larger, AmigaSettingsDescriptionFunctionsConstants.ResourceValueVeryLarge), new(AmigaSettingsDescriptionFunctionsConstants.Maximum, AmigaSettingsDescriptionFunctionsConstants.ResourceValueMaximum),
        new(AmigaSettingsDescriptionFunctionsConstants.Auto, AmigaSettingsDescriptionFunctionsConstants.VisualAutomatic)];

    private static IReadOnlyList<EmulationSettingsChoice> LineModeChoices() =>
    [new(AmigaSettingsDescriptionFunctionsConstants.Auto, AmigaSettingsDescriptionFunctionsConstants.VisualAutomatic), new(AmigaSettingsDescriptionFunctionsConstants.Single, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoLineModeSingle),
        new(AmigaSettingsDescriptionFunctionsConstants.Double, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoLineModeDouble)];

    private static IReadOnlyList<EmulationSettingsChoice> HzChangeChoices() =>
    [new(AmigaSettingsDescriptionFunctionsConstants.Disabled, AmigaSettingsDescriptionFunctionsConstants.ResourceValueDisabled), new(AmigaSettingsDescriptionFunctionsConstants.Enabled, AmigaSettingsDescriptionFunctionsConstants.ResourceValueEnabled),
        new(AmigaSettingsDescriptionFunctionsConstants.Locked, AmigaSettingsDescriptionFunctionsConstants.ResourceStateLocked)];

    private static IReadOnlyList<EmulationSettingsChoice> FrameSkipChoices() =>
    [new(AmigaSettingsDescriptionFunctionsConstants.Disabled, AmigaSettingsDescriptionFunctionsConstants.ResourceValueDisabled), Invariant(AmigaSettingsDescriptionFunctionsConstants.Value1, AmigaSettingsDescriptionFunctionsConstants.Value1), Invariant(AmigaSettingsDescriptionFunctionsConstants.Value22, AmigaSettingsDescriptionFunctionsConstants.Value22)];

    private static IReadOnlyList<EmulationSettingsChoice> ImmediateBlitChoices() =>
    [new(AmigaSettingsDescriptionFunctionsConstants.False, AmigaSettingsDescriptionFunctionsConstants.ResourceValueDisabled), new(AmigaSettingsDescriptionFunctionsConstants.Immediate, AmigaSettingsDescriptionFunctionsConstants.ResourceStateImmediate),
        new(AmigaSettingsDescriptionFunctionsConstants.Waiting, AmigaSettingsDescriptionFunctionsConstants.ResourceStateWaiting)];

    private static IReadOnlyList<EmulationSettingsChoice> CollisionChoices() =>
    [new(AmigaSettingsDescriptionFunctionsConstants.None, AmigaSettingsDescriptionFunctionsConstants.HostToolsNone), new(AmigaSettingsDescriptionFunctionsConstants.Sprites, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoCollisionSprites),
        new(AmigaSettingsDescriptionFunctionsConstants.Playfields, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoCollisionPlayfields),
        new(AmigaSettingsDescriptionFunctionsConstants.Full, AmigaSettingsDescriptionFunctionsConstants.ResourceVideoCollisionFull)];

    private static IReadOnlyList<EmulationSettingsChoice> AudioInterpolationChoices() =>
    [new(AmigaSettingsDescriptionFunctionsConstants.None, AmigaSettingsDescriptionFunctionsConstants.HostToolsNone), new(AmigaSettingsDescriptionFunctionsConstants.Anti, AmigaSettingsDescriptionFunctionsConstants.ResourceAudioInterpolationAnti),
        Invariant(AmigaSettingsDescriptionFunctionsConstants.Sinc, AmigaSettingsDescriptionFunctionsConstants.Sinc2), Invariant(AmigaSettingsDescriptionFunctionsConstants.Rh, AmigaSettingsDescriptionFunctionsConstants.RH), Invariant(AmigaSettingsDescriptionFunctionsConstants.Crux, AmigaSettingsDescriptionFunctionsConstants.Crux2)];

    private static IReadOnlyList<EmulationSettingsChoice> AudioFilterChoices() =>
    [new(AmigaSettingsDescriptionFunctionsConstants.Emulated, AmigaSettingsDescriptionFunctionsConstants.ResourceAudioFilterEmulated), new(AmigaSettingsDescriptionFunctionsConstants.Off, AmigaSettingsDescriptionFunctionsConstants.ResourceValueDisabled),
        new(AmigaSettingsDescriptionFunctionsConstants.On, AmigaSettingsDescriptionFunctionsConstants.ResourceValueEnabled)];

    private static IReadOnlyList<EmulationSettingsChoice> FilterTypeChoices() =>
    [new(AmigaSettingsDescriptionFunctionsConstants.Auto, AmigaSettingsDescriptionFunctionsConstants.VisualAutomatic), new(AmigaSettingsDescriptionFunctionsConstants.Standard, AmigaSettingsDescriptionFunctionsConstants.ResourceValueStandard),
        new(AmigaSettingsDescriptionFunctionsConstants.Enhanced, AmigaSettingsDescriptionFunctionsConstants.ResourceValueEnhanced)];

    private static IEnumerable<EmulationSettingsChoice> PercentageChoices(int minimum, int maximum, int step) =>
        Enumerable.Range(0, (maximum - minimum) / step + 1)
            .Select(index => minimum + index * step).Select(value => Invariant(value.ToString(), $"{value} %", value));

    private static IReadOnlyList<EmulationSettingsChoice> AnalogMouseChoices() =>
    [new(AmigaSettingsDescriptionFunctionsConstants.Disabled, AmigaSettingsDescriptionFunctionsConstants.ResourceValueDisabled), new(AmigaSettingsDescriptionFunctionsConstants.Left, AmigaSettingsDescriptionFunctionsConstants.ResourceControllerStickLeft),
        new(AmigaSettingsDescriptionFunctionsConstants.Right, AmigaSettingsDescriptionFunctionsConstants.ResourceControllerStickRight), new(AmigaSettingsDescriptionFunctionsConstants.Both, AmigaSettingsDescriptionFunctionsConstants.ResourceControllerStickBoth)];

    private static IEnumerable<EmulationSettingsChoice> RatioChoices() => Enumerable.Range(1, 30)
        .Select(value => value / 10d).Select(value => Invariant(value.ToString(AmigaSettingsDescriptionFunctionsConstants.Value00,
            System.Globalization.CultureInfo.InvariantCulture), $"{value:0.0}×"));
}
