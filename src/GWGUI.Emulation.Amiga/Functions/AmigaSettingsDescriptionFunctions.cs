using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga;

internal static class AmigaSettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(AmigaModel model,
        AmigaMachineConfiguration configuration)
    {
        var options = configuration.Options ?? new Dictionary<string, string>();
        return
        [
            Block("cpu", EmulationMachineTab.Cpu, "Emulation.Cpu.Processor", "\uE950", 2,
                Select("puae_cpu_model", EmulationMachineTab.Cpu, "cpu", "Emulation.Cpu.Model",
                    Value(options, "puae_cpu_model", model.DefaultCpu), model.CpuModels),
                Select("puae_fpu_model", EmulationMachineTab.Cpu, "cpu", "Emulation.Fpu.Model",
                    Value(options, "puae_fpu_model", DefaultFpu(model.DefaultCpu)), FpuValues(model.DefaultCpu)),
                Select("puae_cpu_compatibility", EmulationMachineTab.Cpu, "cpu", "Emulation.Cpu.Precision",
                    Value(options, "puae_cpu_compatibility", "exact"), ["normal", "compatible", "memory", "exact"])),
            Block("main-memory", EmulationMachineTab.Ram, "Emulation.Memory.Main", "\uE964", 2,
                Select("puae_chipmem_size", EmulationMachineTab.Ram, "main-memory", "Emulation.Memory.Chip",
                    Value(options, "puae_chipmem_size", ChipMemoryValue(model.ChipMemoryKib)), ChipMemoryValues(model)),
                Select("puae_bogomem_size", EmulationMachineTab.Ram, "main-memory", "Emulation.Memory.Slow",
                    Value(options, "puae_bogomem_size", SlowMemoryValue(model.SlowMemoryKib)), SlowMemoryValues(model))),
            Block("extension-memory", EmulationMachineTab.Ram, "Emulation.Memory.Extensions", "\uE964", 2,
                Select("puae_fastmem_size", EmulationMachineTab.Ram, "extension-memory", "Emulation.Memory.Fast",
                    Value(options, "puae_fastmem_size", model.FastMemoryMib.ToString()), ["0", "1", "2", "4", "8"]),
                Select("puae_z3mem_size", EmulationMachineTab.Ram, "extension-memory", "Emulation.Memory.Z3",
                    Value(options, "puae_z3mem_size", "0"), model.Id is "A3000" or "A4000"
                        ? ["0", "1", "2", "4", "8", "16", "32", "64", "128", "256", "512"] : ["0"])),
            Block("firmware", EmulationMachineTab.Rom, "Emulation.Firmware.Rom.System", "\uE8B7", 1,
                Path(AmigaSettingsConstants.KickstartPath, "Kickstart", configuration.KickstartPath),
                Path(AmigaSettingsConstants.ExtendedRomPath, "Emulation.Firmware.Rom.Extended", configuration.ExtendedRomPath),
                Path(AmigaSettingsConstants.RomKeyPath, "Emulation.Firmware.Rom.Key", configuration.RomKeyPath)),
            Block("display", EmulationMachineTab.Video, "Emulation.Video.Display", "\uE7F4", 2,
                Select("puae_video_standard", EmulationMachineTab.Video, "display", "Emulation.Video.Standard",
                    Value(options, "puae_video_standard", "PAL"), ["PAL auto", "NTSC auto", "PAL", "NTSC"]),
                Select("puae_video_resolution", EmulationMachineTab.Video, "display", "Emulation.Video.Resolution",
                    Value(options, "puae_video_resolution", "auto"), ["auto", "auto-lores", "auto-superhires", "lores", "hires", "superhires"]),
                Select("puae_video_aspect", EmulationMachineTab.Video, "display", "Emulation.Video.AspectRatio",
                    Value(options, "puae_video_aspect", "auto"), ["auto", "PAL", "NTSC", "1:1"]),
                Select("puae_crop", EmulationMachineTab.Video, "display", "Emulation.Video.Crop",
                    Value(options, "puae_crop", "disabled"), ["disabled", "minimum", "smaller", "small", "medium", "large", "larger", "maximum", "auto"]),
                Select(AmigaSettingsConstants.VideoRenderer, EmulationMachineTab.Video, "display", "Emulation.Video.Settings.Rendering",
                    configuration.VideoRenderer.ToString(), Enum.GetNames<EmulationVideoRenderer>())),
            Block("audio", EmulationMachineTab.Audio, "Emulation.Audio", "\uE767", 2,
                Toggle(AmigaSettingsConstants.AudioEnabled, EmulationMachineTab.Audio, "audio",
                    "Emulation.Audio.Enabled", configuration.AudioEnabled),
                Select("puae_sound_interpol", EmulationMachineTab.Audio, "audio", "Emulation.Audio.Interpolation",
                    Value(options, "puae_sound_interpol", "anti"), ["none", "anti", "sinc", "rh", "crux"]),
                Select("puae_sound_filter", EmulationMachineTab.Audio, "audio", "Emulation.Audio.Filter",
                    Value(options, "puae_sound_filter", "emulated"), ["emulated", "off", "on"])),
            Block("mouse", EmulationMachineTab.Mouse, "Emulation.Tab.Mouse", "\uE962", 2,
                Number("puae_mouse_speed", EmulationMachineTab.Mouse, "mouse", "Emulation.Mouse.Speed",
                    Value(options, "puae_mouse_speed", "100")),
                Select("puae_analogmouse", EmulationMachineTab.Mouse, "mouse", "Emulation.Mouse.Analog",
                    Value(options, "puae_analogmouse", "both"), ["none", "left", "right", "both"]))
        ];
    }

    private static EmulationSettingsBlock Block(string id, EmulationMachineTab tab, string title,
        string icon, int columns, params EmulationSettingsField[] fields) =>
        new(id, tab, title, fields, icon, columns);

    private static EmulationSettingsField Select(string id, EmulationMachineTab tab, string block,
        string label, string value, IEnumerable<string> choices) =>
        new(id, tab, block, label, EmulationSettingsEditor.Selection, value,
            choices.Select(choice => new EmulationSettingsChoice(choice, choice, choice)).ToArray());

    private static EmulationSettingsField Toggle(string id, EmulationMachineTab tab, string block,
        string label, bool value) => new(id, tab, block, label, EmulationSettingsEditor.Toggle,
            value ? "enabled" : "disabled");

    private static EmulationSettingsField Number(string id, EmulationMachineTab tab, string block,
        string label, string value) => new(id, tab, block, label, EmulationSettingsEditor.Number, value);

    private static EmulationSettingsField Path(string id, string label, string? value) =>
        new(id, EmulationMachineTab.Rom, "firmware", label, EmulationSettingsEditor.Path, value);

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
}
