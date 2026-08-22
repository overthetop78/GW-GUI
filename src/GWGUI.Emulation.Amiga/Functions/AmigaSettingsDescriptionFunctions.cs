using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga;

internal static class AmigaSettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(AmigaModel model,
        AmigaMachineConfiguration configuration)
    {
        var options = configuration.Options ?? new Dictionary<string, string>();
        var cpu = Value(options, "puae_cpu_model", model.DefaultCpu);
        var compatibility = Value(options, "puae_cpu_compatibility", "exact");
        var ntsc = Value(options, "puae_video_standard", "PAL")
            .StartsWith("NTSC", StringComparison.OrdinalIgnoreCase);
        var frequencies = CpuFrequencyChoices(model, compatibility, ntsc);
        var frequency = CpuFrequencyValue(options, compatibility, frequencies);
        return
        [
            Block("cpu", EmulationMachineTab.Cpu, "Emulation.Cpu.Processor", "\uE950", 2,
                Select("puae_cpu_model", EmulationMachineTab.Cpu, "cpu", "Emulation.Cpu.Model",
                    cpu, model.CpuModels.Select(CpuChoice), model.CpuModels.Count > 1,
                    refreshSettingsOnChange: true),
                Select("puae_fpu_model", EmulationMachineTab.Cpu, "cpu", "Emulation.Fpu.Model",
                    Value(options, "puae_fpu_model", DefaultFpu(cpu)), FpuValues(cpu).Select(FpuChoice)),
                Select("puae_cpu_compatibility", EmulationMachineTab.Cpu, "cpu", "Emulation.Cpu.Precision",
                    compatibility, CompatibilityChoices(), refreshSettingsOnChange: true),
                Information(AmigaSettingsConstants.CpuOriginalSpeed, EmulationMachineTab.Cpu, "cpu",
                    "Emulation.Cpu.SpeedOriginal", FormatMhz(NominalCpuFrequencyMhz(model, ntsc))),
                Select(AmigaSettingsConstants.CpuSpeed, EmulationMachineTab.Cpu, "cpu", "Emulation.Cpu.Speed",
                    frequency, frequencies)),
            Block("main-memory", EmulationMachineTab.Ram, "Emulation.Memory.Main", "\uE964", 2,
                Select("puae_chipmem_size", EmulationMachineTab.Ram, "main-memory", "Emulation.Memory.Main",
                    Value(options, "puae_chipmem_size", ChipMemoryValue(model.ChipMemoryKib)),
                    ChipMemoryValues(model).Select(ChipMemoryChoice)),
                Select("puae_bogomem_size", EmulationMachineTab.Ram, "main-memory", "Emulation.Memory.Slow",
                    Value(options, "puae_bogomem_size", SlowMemoryValue(model.SlowMemoryKib)),
                    SlowMemoryValues(model).Select(SlowMemoryChoice))),
            Block("extension-memory", EmulationMachineTab.Ram, "Emulation.Memory.Extensions", "\uE964", 2,
                Select("puae_fastmem_size", EmulationMachineTab.Ram, "extension-memory", "Emulation.Memory.Fast",
                    Value(options, "puae_fastmem_size", model.FastMemoryMib.ToString()),
                    new[] { "0", "1", "2", "4", "8" }.Select(MemoryMibChoice)),
                Select("puae_z3mem_size", EmulationMachineTab.Ram, "extension-memory", "Emulation.Memory.Z3",
                    Value(options, "puae_z3mem_size", "0"), model.Id is "A3000" or "A4000"
                        ? new[] { "0", "1", "2", "4", "8", "16", "32", "64", "128", "256", "512" }
                            .Select(MemoryMibChoice)
                        : new[] { "0" }.Select(MemoryMibChoice))),
            Block("firmware", EmulationMachineTab.Rom, "Emulation.Firmware.Rom.System", "\uE8B7", 1,
                Path(AmigaSettingsConstants.KickstartPath, "Kickstart", configuration.KickstartPath),
                Path(AmigaSettingsConstants.ExtendedRomPath, "Emulation.Firmware.Rom.Extended", configuration.ExtendedRomPath),
                Path(AmigaSettingsConstants.RomKeyPath, "Emulation.Firmware.Rom.Key", configuration.RomKeyPath)),
            Block("display", EmulationMachineTab.Video, "Emulation.Video.Settings.Display", "\uE7F4", 2,
                Select("puae_video_standard", EmulationMachineTab.Video, "display", "Emulation.Video.Standard",
                    Value(options, "puae_video_standard", "PAL"), VideoStandardChoices(),
                    refreshSettingsOnChange: true),
                Select("puae_video_resolution", EmulationMachineTab.Video, "display", "Emulation.Video.Resolution",
                    Value(options, "puae_video_resolution", "auto"), VideoResolutionChoices()),
                Select("puae_video_aspect", EmulationMachineTab.Video, "display", "Emulation.Video.AspectRatio",
                    Value(options, "puae_video_aspect", "auto"), VideoAspectChoices()),
                Select("puae_crop", EmulationMachineTab.Video, "display", "Emulation.Video.Crop",
                    Value(options, "puae_crop", "disabled"), CropChoices()),
                Select("puae_video_vresolution", EmulationMachineTab.Video, "display", "Emulation.Video.LineMode",
                    Value(options, "puae_video_vresolution", "auto"), LineModeChoices()),
                Select("puae_video_allow_hz_change", EmulationMachineTab.Video, "display", "Emulation.Video.HzChange",
                    Value(options, "puae_video_allow_hz_change", "locked"), HzChangeChoices()),
                Select("puae_gfx_framerate", EmulationMachineTab.Video, "display", "Emulation.Video.FrameSkip",
                    Value(options, "puae_gfx_framerate", "disabled"), FrameSkipChoices()),
                Select("puae_gfx_colors", EmulationMachineTab.Video, "display", "Emulation.Video.Colors",
                    Value(options, "puae_gfx_colors", "24bit"), InvariantChoices("16bit", "24bit")),
                Select("puae_gfx_gamma", EmulationMachineTab.Video, "display", "Emulation.Video.Gamma",
                    Value(options, "puae_gfx_gamma", "0"), Enumerable.Range(-5, 11)
                        .Select(value => Invariant((value * 100).ToString(), value.ToString()))),
                Select("puae_immediate_blits", EmulationMachineTab.Video, "display", "Emulation.State.ImmediateBlits",
                    Value(options, "puae_immediate_blits", "false"), ImmediateBlitChoices()),
                Select("puae_collision_level", EmulationMachineTab.Video, "display", "Emulation.Video.Collision.Level",
                    Value(options, "puae_collision_level", "playfields"), CollisionChoices()),
                Toggle("puae_gfx_flickerfixer", EmulationMachineTab.Video, "display",
                    "Emulation.Video.FlickerFixer", Value(options, "puae_gfx_flickerfixer", "disabled") == "enabled"),
                Select(AmigaSettingsConstants.VideoRenderer, EmulationMachineTab.Video, "display", "Emulation.Video.Settings.Rendering",
                    configuration.VideoRenderer.ToString(), RendererChoices())),
            Block("audio", EmulationMachineTab.Audio, "Emulation.Audio", "\uE767", 2,
                Toggle(AmigaSettingsConstants.AudioEnabled, EmulationMachineTab.Audio, "audio",
                    "Emulation.Audio.Enabled", configuration.AudioEnabled),
                AudioOutput(configuration.Audio?.OutputDeviceId),
                Select(AmigaSettingsConstants.AudioLatency, EmulationMachineTab.Audio, "audio",
                    "Emulation.Audio.LatencyLabel", (configuration.Audio?.LatencyMilliseconds ?? 50).ToString(),
                    new[] { 20, 35, 50, 75, 100, 150, 250 }.Select(value =>
                        Invariant(value.ToString(), $"{value} ms"))),
                Select("puae_sound_interpol", EmulationMachineTab.Audio, "audio", "Emulation.Audio.Interpolation",
                    Value(options, "puae_sound_interpol", configuration.Audio?.Interpolation ?? "anti"),
                    AudioInterpolationChoices()),
                Select("puae_sound_filter", EmulationMachineTab.Audio, "audio", "Emulation.Audio.Filter",
                    Value(options, "puae_sound_filter", configuration.Audio?.Filter ?? "emulated"),
                    AudioFilterChoices()),
                Select("puae_sound_filter_type", EmulationMachineTab.Audio, "audio", "Emulation.Audio.FilterType",
                    Value(options, "puae_sound_filter_type", "auto"), FilterTypeChoices()),
                Select(AmigaSettingsConstants.AudioStereoSeparation, EmulationMachineTab.Audio, "audio",
                    "Emulation.Audio.StereoSeparation", $"{configuration.Audio?.StereoSeparation ?? 100}",
                    PercentageChoices(0, 100, 10)),
                Select("puae_floppy_sound", EmulationMachineTab.Audio, "audio", "Emulation.Audio.Floppy.Sound",
                    Value(options, "puae_floppy_sound", "80"), PercentageChoices(0, 100, 5)),
                Select("puae_floppy_sound_type", EmulationMachineTab.Audio, "audio",
                    "Emulation.Audio.Floppy.SoundType", Value(options, "puae_floppy_sound_type", "internal"),
                    [new("internal", "Emulation.Value.Internal"), Invariant("A500", "A500"),
                        new("LOUD", "Emulation.Value.Loud")]),
                Toggle("puae_floppy_sound_empty_mute", EmulationMachineTab.Audio, "audio",
                    "Emulation.Audio.Floppy.MuteEmpty",
                    Value(options, "puae_floppy_sound_empty_mute", "enabled") == "enabled"),
                Select("puae_sound_volume_cd", EmulationMachineTab.Audio, "audio", "Emulation.Audio.Cd.Volume",
                    Value(options, "puae_sound_volume_cd", "100%").TrimEnd('%'), PercentageChoices(0, 100, 5))),
            Block("mouse", EmulationMachineTab.Mouse, "Emulation.Tab.Mouse", "\uE962", 2,
                Number("puae_mouse_speed", EmulationMachineTab.Mouse, "mouse", "Emulation.Mouse.Speed",
                    Value(options, "puae_mouse_speed", "100")),
                Select("puae_analogmouse", EmulationMachineTab.Mouse, "mouse", "Emulation.Mouse.Analog",
                    Value(options, "puae_analogmouse", "both"), AnalogMouseChoices()),
                Select("puae_analogmouse_deadzone", EmulationMachineTab.Mouse, "mouse",
                    "Emulation.Mouse.AnalogDeadzone", Value(options, "puae_analogmouse_deadzone", "15"),
                    PercentageChoices(0, 50, 5)),
                Select("puae_analogmouse_speed", EmulationMachineTab.Mouse, "mouse",
                    "Emulation.Mouse.AnalogSpeed", Value(options, "puae_analogmouse_speed", "1.0"),
                    RatioChoices()),
                Select("puae_analogmouse_speed_right", EmulationMachineTab.Mouse, "mouse",
                    "Emulation.Mouse.AnalogSpeed", Value(options, "puae_analogmouse_speed_right", "1.0"),
                    RatioChoices())),
            Block("controller-behavior", EmulationMachineTab.Controllers,
                "Emulation.Controller.Action.TurboFire", "\uE945", 2,
                Select("puae_turbo_pulse", EmulationMachineTab.Controllers, "controller-behavior",
                    "Emulation.Controller.Turbo.Pulse", Value(options, "puae_turbo_pulse", "6"),
                    InvariantChoices("2", "4", "6", "8", "10", "12")),
                Toggle(AmigaSettingsConstants.ParallelJoystickAdapter, EmulationMachineTab.Controllers,
                    "controller-behavior", "Emulation.Amiga.Controller.ParallelAdapter",
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
            choices.ToArray(), IsEnabled: isEnabled, RefreshSettingsOnChange: refreshSettingsOnChange);

    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block,
        string label, string value) => new(id, tab, block, label, EmulationSettingsEditor.Information, value);

    private static EmulationSettingsField Toggle(string id, EmulationMachineTab tab, string block,
        string label, bool value, bool refreshSettingsOnChange = false) =>
        new(id, tab, block, label, EmulationSettingsEditor.Toggle,
            value ? "enabled" : "disabled", RefreshSettingsOnChange: refreshSettingsOnChange);

    private static EmulationSettingsField Number(string id, EmulationMachineTab tab, string block,
        string label, string value) => new(id, tab, block, label, EmulationSettingsEditor.Number, value);

    private static EmulationSettingsField Path(string id, string label, string? value) =>
        new(id, EmulationMachineTab.Rom, "firmware", label, EmulationSettingsEditor.Path, value,
            DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware);

    private static EmulationSettingsField AudioOutput(string? value) => new(
        AmigaSettingsConstants.AudioOutput, EmulationMachineTab.Audio, "audio", "Emulation.Audio.Device",
        EmulationSettingsEditor.Selection, value ?? string.Empty,
        [new EmulationSettingsChoice(string.Empty, "Emulation.Audio.DefaultOutput")],
        ChoiceSource: EmulationSettingsChoiceSource.AudioOutputDevices);

    private static string Value(IReadOnlyDictionary<string, string> values, string key, string fallback) =>
        values.GetValueOrDefault(key) ?? fallback;

    private static string DefaultFpu(string cpu) => cpu is "68040" or "68060" ? "cpu" : "0";
    private static IReadOnlyList<string> FpuValues(string cpu) => cpu switch
    {
        "68000" or "68010" => ["0"],
        "68020" or "68030" => ["0", "68881", "68882"],
        _ => ["cpu", "0", "68881", "68882"]
    };
    private static IReadOnlyList<string> ChipMemoryValues(AmigaModel model) => model.Id switch
    {
        "A1000" => ["1"], "A500" => ["1", "2", "3", "4"],
        "A500PLUS" or "A600" => ["2", "4"], "A2000" => ["1", "2", "4"], _ => ["4"]
    };
    private static IReadOnlyList<string> SlowMemoryValues(AmigaModel model) => model.Id switch
    {
        "A1000" or "A500" or "A500PLUS" or "A2000" => ["0", "2", "4", "6", "7"], _ => ["0"]
    };
    private static string ChipMemoryValue(int kib) => Math.Clamp(kib / 512, 1, 4).ToString();
    private static string SlowMemoryValue(int kib) => kib switch
    {
        512 => "2", 1024 => "4", 1536 => "6", 1792 => "7", _ => "0"
    };

    private static EmulationSettingsChoice CpuChoice(string value) =>
        new(value, string.Empty, value == "68020" ? "Motorola 68EC020" : $"Motorola {value}");

    private static EmulationSettingsChoice FpuChoice(string value) => value switch
    {
        "0" => new(value, "Emulation.Memory.None"),
        "cpu" => new(value, string.Empty, "CPU"),
        _ => new(value, string.Empty, value)
    };

    private static IReadOnlyList<EmulationSettingsChoice> CompatibilityChoices() =>
    [
        new("normal", "Emulation.Cpu.Compatibility.Normal"),
        new("compatible", "Emulation.Cpu.Compatibility.Compatible"),
        new("memory", "Emulation.Cpu.Compatibility.Memory"),
        new("exact", "Emulation.Cpu.Compatibility.Exact")
    ];

    private static EmulationSettingsChoice ChipMemoryChoice(string value)
    {
        var kib = int.TryParse(value, out var units) ? units * 512 : 0;
        return MemoryChoice(value, kib * 1024L);
    }

    private static EmulationSettingsChoice SlowMemoryChoice(string value)
    {
        var kib = value switch { "2" => 512, "4" => 1024, "6" => 1536, "7" => 1792, _ => 0 };
        return MemoryChoice(value, kib * 1024L);
    }

    private static EmulationSettingsChoice MemoryMibChoice(string value)
    {
        var mib = int.TryParse(value, out var parsed) ? parsed : 0;
        return MemoryChoice(value, mib * 1024L * 1024L);
    }

    private static EmulationSettingsChoice MemoryChoice(string id, long bytes) => bytes == 0
        ? new EmulationSettingsChoice(id, "Emulation.Memory.None", NumericValue: 0)
        : bytes < 1024L * 1024L
            ? new EmulationSettingsChoice(id, string.Empty, $"{bytes / 1024L} KiB", bytes)
            : new EmulationSettingsChoice(id, string.Empty, $"{bytes / (1024L * 1024L)} MiB", bytes);

    private static IReadOnlyList<EmulationSettingsChoice> CpuFrequencyChoices(AmigaModel model,
        string compatibility, bool ntsc)
    {
        var nominal = NominalCpuFrequencyMhz(model, ntsc);
        if (compatibility is "memory" or "exact")
        {
            var halfA500Clock = ntsc ? 3.579545d : 3.546895d;
            var choices = new[] { 1, 2, 4, 8, 16 }.Select(multiplier =>
            {
                var frequency = halfA500Clock * multiplier;
                var ratio = frequency / nominal;
                return FrequencyChoice(ratio, "0.0", multiplier.ToString(), frequency);
            }).Where(choice => !Approximately(choice.NumericValue.GetValueOrDefault() / 1_000_000d, nominal))
                .ToList();
            choices.Add(FrequencyChoice(1d, "0.0", "0", nominal));
            return choices.OrderBy(choice => choice.NumericValue).ToArray();
        }

        return new[]
        {
            (Ratio: 0.5d, Throttle: "-500.0"), (Ratio: 1d, Throttle: "0.0"),
            (Ratio: 2d, Throttle: "1000.0"), (Ratio: 4d, Throttle: "3000.0"),
            (Ratio: 8d, Throttle: "7000.0")
        }.Select(item => FrequencyChoice(item.Ratio, item.Throttle, "0", nominal * item.Ratio)).ToArray();
    }

    private static EmulationSettingsChoice FrequencyChoice(double ratio, string throttle, string multiplier,
        double frequency)
    {
        var prefix = Approximately(ratio, 1d) ? "100 %" : $"{Math.Round(ratio * 100d):0} %";
        return new EmulationSettingsChoice($"{throttle}|{multiplier}", string.Empty,
            $"{prefix} — {FormatMhz(frequency)}", (long)Math.Round(frequency * 1_000_000d));
    }

    private static string CpuFrequencyValue(IReadOnlyDictionary<string, string> options, string compatibility,
        IReadOnlyList<EmulationSettingsChoice> choices)
    {
        var throttle = Value(options, "puae_cpu_throttle", "0.0");
        var multiplier = Value(options, "puae_cpu_multiplier", "0");
        var expected = compatibility is "memory" or "exact" ? $"0.0|{multiplier}" : $"{throttle}|0";
        return choices.Any(choice => choice.Id == expected)
            ? expected : choices.FirstOrDefault(choice => choice.Id == "0.0|0")?.Id ?? choices[0].Id;
    }

    private static double NominalCpuFrequencyMhz(AmigaModel model, bool ntsc) => model.Id switch
    {
        "A1200" or "CD32" => ntsc ? 14.31818d : 14.18758d,
        "A3000" or "A4000" => 25d,
        _ => ntsc ? 7.15909d : 7.09379d
    };

    private static string FormatMhz(double frequency) => $"{frequency:0.00} MHz";
    private static bool Approximately(double left, double right) => Math.Abs(left - right) < 0.0001d;

    private static EmulationSettingsChoice Invariant(string id, string text, long? numericValue = null) =>
        new(id, string.Empty, text, numericValue);

    private static IEnumerable<EmulationSettingsChoice> InvariantChoices(params string[] values) =>
        values.Select(value => Invariant(value, value));

    private static IReadOnlyList<EmulationSettingsChoice> VideoStandardChoices() =>
    [Invariant("PAL auto", "PAL auto"), Invariant("NTSC auto", "NTSC auto"),
        Invariant("PAL", "PAL"), Invariant("NTSC", "NTSC")];

    private static IReadOnlyList<EmulationSettingsChoice> VideoResolutionChoices() =>
    [new("auto", "Visual.Automatic"), new("auto-lores", "Emulation.Video.Resolution.AutoLow"),
        new("auto-superhires", "Emulation.Video.Resolution.AutoSuperHigh"),
        new("lores", "Emulation.Video.Resolution.Low"), new("hires", "Emulation.Video.Resolution.High"),
        new("superhires", "Emulation.Video.Resolution.SuperHigh")];

    private static IReadOnlyList<EmulationSettingsChoice> VideoAspectChoices() =>
    [new("auto", "Visual.Automatic"), Invariant("PAL", "PAL"), Invariant("NTSC", "NTSC"),
        Invariant("1:1", "1:1")];

    private static IReadOnlyList<EmulationSettingsChoice> CropChoices() =>
    [new("disabled", "Emulation.Value.Disabled"), new("minimum", "Emulation.Value.Minimum"),
        new("smaller", "Emulation.Value.VerySmall"), new("small", "Emulation.Value.Small"),
        new("medium", "Emulation.Value.Medium"), new("large", "Emulation.Value.Large"),
        new("larger", "Emulation.Value.VeryLarge"), new("maximum", "Emulation.Value.Maximum"),
        new("auto", "Visual.Automatic")];

    private static IReadOnlyList<EmulationSettingsChoice> LineModeChoices() =>
    [new("auto", "Visual.Automatic"), new("single", "Emulation.Video.LineMode.Single"),
        new("double", "Emulation.Video.LineMode.Double")];

    private static IReadOnlyList<EmulationSettingsChoice> HzChangeChoices() =>
    [new("disabled", "Emulation.Value.Disabled"), new("enabled", "Emulation.Value.Enabled"),
        new("locked", "Emulation.State.Locked")];

    private static IReadOnlyList<EmulationSettingsChoice> FrameSkipChoices() =>
    [new("disabled", "Emulation.Value.Disabled"), Invariant("1", "1"), Invariant("2", "2")];

    private static IReadOnlyList<EmulationSettingsChoice> ImmediateBlitChoices() =>
    [new("false", "Emulation.Value.Disabled"), new("immediate", "Emulation.State.Immediate"),
        new("waiting", "Emulation.State.Waiting")];

    private static IReadOnlyList<EmulationSettingsChoice> CollisionChoices() =>
    [new("none", "HostTools.None"), new("sprites", "Emulation.Video.Collision.Sprites"),
        new("playfields", "Emulation.Video.Collision.Playfields"),
        new("full", "Emulation.Video.Collision.Full")];

    private static IEnumerable<EmulationSettingsChoice> RendererChoices() =>
        Enum.GetNames<EmulationVideoRenderer>().Select(value => Invariant(value, value));

    private static IReadOnlyList<EmulationSettingsChoice> AudioInterpolationChoices() =>
    [new("none", "HostTools.None"), new("anti", "Emulation.Audio.Interpolation.Anti"),
        Invariant("sinc", "Sinc"), Invariant("rh", "RH"), Invariant("crux", "Crux")];

    private static IReadOnlyList<EmulationSettingsChoice> AudioFilterChoices() =>
    [new("emulated", "Emulation.Audio.Filter.Emulated"), new("off", "Emulation.Value.Disabled"),
        new("on", "Emulation.Value.Enabled")];

    private static IReadOnlyList<EmulationSettingsChoice> FilterTypeChoices() =>
    [new("auto", "Visual.Automatic"), new("standard", "Emulation.Value.Standard"),
        new("enhanced", "Emulation.Value.Enhanced")];

    private static IEnumerable<EmulationSettingsChoice> PercentageChoices(int minimum, int maximum, int step) =>
        Enumerable.Range(0, (maximum - minimum) / step + 1)
            .Select(index => minimum + index * step).Select(value => Invariant(value.ToString(), $"{value} %", value));

    private static IReadOnlyList<EmulationSettingsChoice> AnalogMouseChoices() =>
    [new("disabled", "Emulation.Value.Disabled"), new("left", "Emulation.Controller.Stick.Left"),
        new("right", "Emulation.Controller.Stick.Right"), new("both", "Emulation.Controller.Stick.Both")];

    private static IEnumerable<EmulationSettingsChoice> RatioChoices() => Enumerable.Range(1, 30)
        .Select(value => value / 10d).Select(value => Invariant(value.ToString("0.0",
            System.Globalization.CultureInfo.InvariantCulture), $"{value:0.0}×"));
}
