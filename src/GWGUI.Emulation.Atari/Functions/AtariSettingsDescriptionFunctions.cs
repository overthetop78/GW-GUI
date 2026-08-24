using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

internal static class AtariSettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(AtariMachineConfiguration configuration)
    {
        var compatibility = AtariCompatibilityCatalog.Get(configuration.Model);
        var blocks = (compatibility.Core == AtariEmulator.Hatari
            ? CreateSt(configuration)
            : CreateClassic(configuration)).ToList();
        AddGeneralFolders(configuration, compatibility, blocks);
        AddMouseSettings(configuration, compatibility, blocks);
        return blocks;
    }

    private static void AddMouseSettings(AtariMachineConfiguration configuration,
        AtariCompatibilityDefinition compatibility, ICollection<EmulationSettingsBlock> blocks)
    {
        if (!compatibility.VisibleTabs.Contains(AtariSettingsTab.Mouse)) return;
        var choices = Enumerable.Range(AtariMouseSettingsConstants.MinimumSpeedPercent
                / AtariMouseSettingsConstants.SpeedStepPercent,
                (AtariMouseSettingsConstants.MaximumSpeedPercent - AtariMouseSettingsConstants.MinimumSpeedPercent)
                / AtariMouseSettingsConstants.SpeedStepPercent + 1)
            .Select(value => value * AtariMouseSettingsConstants.SpeedStepPercent)
            .Select(value => new EmulationSettingsChoice(value.ToString(), string.Empty, $"{value} %", value));
        blocks.Add(Block("mouse", EmulationMachineTab.Mouse, "Emulation.Tab.Mouse", "\uE962", 1,
            Select(AtariMouseSettingsConstants.SpeedOptionKey, EmulationMachineTab.Mouse, "mouse",
                "Emulation.Mouse.Speed", Value(configuration, AtariMouseSettingsConstants.SpeedOptionKey,
                    AtariMouseSettingsConstants.DefaultSpeedPercent.ToString()), choices)));
    }

    private static void AddGeneralFolders(AtariMachineConfiguration configuration,
        AtariCompatibilityDefinition compatibility, ICollection<EmulationSettingsBlock> blocks)
    {
        var supportsHardDisk = AtariEightBitSettingsCatalog.SupportsComputerOptions(configuration.Model)
            || compatibility.Media.Any(rule => rule.Availability == AtariMediaAvailability.Available
                && rule.Category is AtariMediaCategory.HardDisk or AtariMediaCategory.Directory);
        if (!supportsHardDisk) return;
        blocks.Add(Block("default-folders", EmulationMachineTab.General, "Emulation.Folder.Default", "\uEDA2", 1,
            new EmulationSettingsField(AtariSettingsConstants.HardDiskFolder, EmulationMachineTab.General,
                "default-folders", "Emulation.Storage.HardDisk.List", EmulationSettingsEditor.DirectoryPath,
                configuration.Folders.HardDisks,
                DefaultFolderCategory: EmulationDefaultFolderCategory.HardDisk)));
    }

    private static IReadOnlyList<EmulationSettingsBlock> CreateSt(AtariMachineConfiguration configuration)
    {
        var model = AtariStModelCatalog.Get(configuration.Model);
        return
        [
            Block("processor", EmulationMachineTab.Cpu, "Emulation.Cpu.Processor", "\uE950", 2,
                Select(AtariSettingsConstants.Cpu, EmulationMachineTab.Cpu, "processor", "Emulation.Cpu.Model",
                    Value(configuration, AtariSettingsConstants.Cpu, model.DefaultCpu.ToString()),
                    model.Cpus.Select(value => AtariHardwareSettingsFunctions.Invariant(value.ToString(), value.ToString()))),
                Select(AtariSettingsConstants.CpuPrecision, EmulationMachineTab.Cpu, "processor", "Emulation.Cpu.Precision",
                    Value(configuration, AtariSettingsConstants.CpuPrecision, model.DefaultCpuPrecision.ToString()),
                    model.CpuPrecisions.Select(AtariHardwareSettingsFunctions.CpuPrecision)),
                Select(AtariSettingsConstants.Fpu, EmulationMachineTab.Cpu, "processor", "Emulation.Fpu.Model",
                    Value(configuration, AtariSettingsConstants.Fpu, model.DefaultFpu.ToString()),
                    model.Fpus.Select(AtariHardwareSettingsFunctions.Fpu)),
                Information(AtariSettingsConstants.CpuOriginalFrequency, EmulationMachineTab.Cpu, "processor",
                    "Emulation.Cpu.SpeedOriginal",
                    AtariHardwareSettingsFunctions.FrequencyMhz(model.DefaultCpuFrequencyMhz).InvariantDisplayValue!),
                Select(AtariSettingsConstants.CpuFrequency, EmulationMachineTab.Cpu, "processor", "Emulation.Cpu.Speed",
                    Value(configuration, AtariSettingsConstants.CpuFrequency, model.DefaultCpuFrequencyMhz.ToString()),
                    model.CpuFrequenciesMhz.Select(AtariHardwareSettingsFunctions.FrequencyMhz))),
            Block("main-memory", EmulationMachineTab.Ram, "Emulation.Memory.Main", "\uE964", 2,
                Select(AtariConfigurationOptionConstants.MainMemory, EmulationMachineTab.Ram, "main-memory", "Emulation.Memory.Main",
                    Value(configuration, AtariConfigurationOptionConstants.MainMemory,
                        ((long)model.DefaultMainMemoryKib * AtariHardwareSettingsConstants.BytesPerKibibyte).ToString()),
                    model.MainMemoryKib.Select(AtariHardwareSettingsFunctions.MemoryKib))),
            Block("extension-memory", EmulationMachineTab.Ram, "Emulation.Memory.Extensions", "\uE964", 1,
                Select(AtariSettingsConstants.AlternateMemory, EmulationMachineTab.Ram, "extension-memory", "Emulation.Memory.Extensions",
                    Value(configuration, AtariSettingsConstants.AlternateMemory,
                        ((long)model.DefaultAlternateMemoryMib * AtariHardwareSettingsConstants.BytesPerMebibyte).ToString()),
                    model.AlternateMemoryMib.Select(AtariHardwareSettingsFunctions.MemoryMib))),
            Block("firmware", EmulationMachineTab.Rom, "Emulation.Firmware.Rom.System", "\uE8B7", 1,
                Path(AtariSettingsConstants.SystemFirmware, EmulationMachineTab.Rom, "firmware",
                    "Emulation.Firmware.Rom.System",
                    configuration.Firmwares.FirstOrDefault(item => item.Category == AtariFirmwareCategory.Tos)?.Path),
                Toggle("hatari_fastboot", EmulationMachineTab.Rom, "firmware", "Emulation.Atari.FastBoot",
                    Value(configuration, "hatari_fastboot", "false") == "true", "true", "false")),
            Block("storage-options", EmulationMachineTab.Storage, "Emulation.Storage.Device.List", "\uE7C3", 1,
                Toggle(AtariMachineOptionConstants.DriveActivity, EmulationMachineTab.Storage,
                    "storage-options", "Emulation.Storage.ActivityOsd",
                    Value(configuration, AtariMachineOptionConstants.DriveActivity, "false") == "true",
                    "true", "false")),
            Block("video", EmulationMachineTab.Video, "Emulation.Video.Settings.Display", "\uE7F4", 2,
                Select(AtariVideoAudioSettingsConstants.StandardOption, EmulationMachineTab.Video, "video",
                    "Emulation.Video.Standard", Value(configuration, AtariVideoAudioSettingsConstants.StandardOption,
                        AtariVideoAudioSettingsConstants.Automatic), StStandards(model)),
                Select(AtariSettingsConstants.Region, EmulationMachineTab.Video, "video", "Emulation.Atari.Video.Region",
                    Value(configuration, AtariSettingsConstants.Region, model.DefaultRegion.ToString()),
                    model.Regions.Select(AtariHardwareSettingsFunctions.StRegion)),
                Select(AtariVideoAudioSettingsConstants.ResolutionOption, EmulationMachineTab.Video, "video",
                    "Emulation.Video.Resolution", Value(configuration, AtariVideoAudioSettingsConstants.ResolutionOption,
                        AtariVideoAudioSettingsConstants.Automatic), AutomaticAndNative()),
                Select(AtariVideoAudioSettingsConstants.AspectRatioOption, EmulationMachineTab.Video, "video",
                    "Emulation.Video.AspectRatio", Value(configuration, AtariVideoAudioSettingsConstants.AspectRatioOption,
                        AtariVideoAudioSettingsConstants.Automatic), AspectRatios()),
                Toggle(AtariVideoAudioSettingsConstants.CropOption, EmulationMachineTab.Video, "video",
                    "Emulation.Video.Crop", Value(configuration, AtariVideoAudioSettingsConstants.CropOption,
                        AtariVideoAudioSettingsConstants.Disabled) == AtariVideoAudioSettingsConstants.Enabled),
                Select(AtariVideoAudioSettingsConstants.FrameSkipOption, EmulationMachineTab.Video, "video",
                    "Emulation.Video.FrameSkip", Value(configuration, AtariVideoAudioSettingsConstants.FrameSkipOption,
                        AtariVideoAudioSettingsConstants.MinimumFrameSkip.ToString()), FrameSkips()),
                Renderer(configuration)),
            Audio(configuration, true)
        ];
    }

    private static IReadOnlyList<EmulationSettingsBlock> CreateClassic(AtariMachineConfiguration configuration)
    {
        var model = AtariClassicModelCatalog.Get(configuration.Model);
        var blocks = new List<EmulationSettingsBlock>
        {
            Block("processor", EmulationMachineTab.Cpu, "Emulation.Cpu.Processor", "\uE950", 2,
                Select(AtariSettingsConstants.Cpu, EmulationMachineTab.Cpu, "processor", "Emulation.Cpu.Model",
                    Value(configuration, AtariSettingsConstants.Cpu, model.DefaultCpu.ToString()), model.Cpus.Select(value => value.ToString()),
                    isEnabled: model.Cpus.Count > 1),
                Information(AtariSettingsConstants.CpuOriginalFrequency, EmulationMachineTab.Cpu, "processor", "Emulation.Cpu.SpeedOriginal",
                    $"{model.DefaultCpuFrequencyHz / 1_000_000d:0.00} MHz")),
            Block("main-memory", EmulationMachineTab.Ram, "Emulation.Memory.Main", "\uE964", 1,
                ClassicMemory(configuration, model)),
            Block("video", EmulationMachineTab.Video, "Emulation.Video.Settings.Display", "\uE7F4", 2,
                Select(AtariConfigurationOptionConstants.VideoStandard, EmulationMachineTab.Video, "video",
                    "Emulation.Video.Standard", Value(configuration, AtariConfigurationOptionConstants.VideoStandard,
                        model.DefaultRegion.ToString()), model.Regions.Select(AtariHardwareSettingsFunctions.ClassicRegion),
                    isEnabled: model.Regions.Count > 1),
                Select(AtariVideoAudioSettingsConstants.ResolutionOption, EmulationMachineTab.Video, "video",
                    "Emulation.Video.Resolution", Value(configuration, AtariVideoAudioSettingsConstants.ResolutionOption,
                        DefaultResolution(configuration.Model)), Resolutions(configuration.Model)),
                Renderer(configuration)),
            Audio(configuration, false)
        };
        if (configuration.Model == AtariMachineModel.Atari400)
        {
            blocks.Add(Block("firmware", EmulationMachineTab.Rom, "Emulation.Firmware.Rom.System", "\uE8B7", 1,
                Path(AtariSettingsConstants.SystemFirmware, EmulationMachineTab.Rom, "firmware",
                    "Emulation.Firmware.Rom.System", configuration.Firmwares.FirstOrDefault()?.Path)));
        }
        AddEightBitMemory(configuration, blocks);
        AddEightBitOptions(configuration, blocks);
        AddEightBitControllerOptions(configuration, blocks);
        return blocks;
    }

    private static void AddEightBitControllerOptions(AtariMachineConfiguration configuration,
        ICollection<EmulationSettingsBlock> blocks)
    {
        if (!AtariEightBitSettingsCatalog.SupportsComputerOptions(configuration.Model)) return;
        blocks.Add(Block("controller-options", EmulationMachineTab.Controllers,
            "Emulation.Controller.Tab", "\uE7FC", 2,
            Select(AtariEightBitSettingsConstants.PaddleMovementSpeedOptionKey,
                EmulationMachineTab.Controllers, "controller-options", "Emulation.Atari.Controller.PaddleSpeed",
                Value(configuration, AtariEightBitSettingsConstants.PaddleMovementSpeedOptionKey,
                    AtariEightBitSettingsConstants.DefaultPaddleMovementSpeed),
                AtariEightBitSettingsCatalog.PaddleMovementSpeeds),
            Select(AtariEightBitSettingsConstants.AutofireOptionKey,
                EmulationMachineTab.Controllers, "controller-options", "Emulation.Atari.Controller.Autofire",
                Value(configuration, AtariEightBitSettingsConstants.AutofireOptionKey,
                    AtariEightBitSettingsConstants.Disabled), AtariEightBitSettingsCatalog.AutofireModes),
            Select(AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey,
                EmulationMachineTab.Controllers, "controller-options", "Emulation.Atari.Controller.Compatibility",
                Value(configuration, AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey,
                    AtariEightBitSettingsConstants.None), AtariEightBitSettingsCatalog.ControllerCompatibilityModes),
            Select(AtariEightBitSettingsConstants.DigitalSensitivityOptionKey,
                EmulationMachineTab.Controllers, "controller-options", "Emulation.Atari.Controller.DigitalSensitivity",
                Value(configuration, AtariEightBitSettingsConstants.DigitalSensitivityOptionKey,
                    AtariEightBitSettingsConstants.DefaultSensitivity), AtariEightBitSettingsCatalog.Sensitivities),
            Select(AtariEightBitSettingsConstants.AnalogSensitivityOptionKey,
                EmulationMachineTab.Controllers, "controller-options", "Emulation.Atari.Controller.AnalogSensitivity",
                Value(configuration, AtariEightBitSettingsConstants.AnalogSensitivityOptionKey,
                    AtariEightBitSettingsConstants.DefaultSensitivity), AtariEightBitSettingsCatalog.Sensitivities)));
    }

    private static void AddEightBitOptions(AtariMachineConfiguration configuration,
        ICollection<EmulationSettingsBlock> blocks)
    {
        if (!AtariEightBitSettingsCatalog.SupportsComputerOptions(configuration.Model)) return;
        blocks.Add(Block("video-colors", EmulationMachineTab.Video, "Emulation.Video.Settings.Display", "\uE7F4", 2,
            Select(AtariEightBitSettingsConstants.ArtifactingModeOptionKey, EmulationMachineTab.Video,
                "video-colors", "Emulation.Atari.Video.Artifacting",
                Value(configuration, AtariEightBitSettingsConstants.ArtifactingModeOptionKey,
                    AtariEightBitSettingsConstants.None), AtariEightBitSettingsCatalog.ArtifactingModes),
            Select(AtariEightBitSettingsConstants.ColorHueOptionKey, EmulationMachineTab.Video, "video-colors",
                "Emulation.Atari.Video.Hue", Value(configuration, AtariEightBitSettingsConstants.ColorHueOptionKey,
                    AtariEightBitSettingsConstants.DefaultColorAdjustment), AtariEightBitSettingsCatalog.ColorAdjustments),
            Select(AtariEightBitSettingsConstants.ColorSaturationOptionKey, EmulationMachineTab.Video,
                "video-colors", "Emulation.Atari.Video.Saturation",
                Value(configuration, AtariEightBitSettingsConstants.ColorSaturationOptionKey,
                    AtariEightBitSettingsConstants.DefaultColorAdjustment), AtariEightBitSettingsCatalog.ColorAdjustments),
            Select(AtariEightBitSettingsConstants.ColorContrastOptionKey, EmulationMachineTab.Video,
                "video-colors", "Emulation.Atari.Video.Contrast",
                Value(configuration, AtariEightBitSettingsConstants.ColorContrastOptionKey,
                    AtariEightBitSettingsConstants.DefaultColorAdjustment), AtariEightBitSettingsCatalog.ContrastAndBrightness),
            Select(AtariEightBitSettingsConstants.ColorBrightnessOptionKey, EmulationMachineTab.Video,
                "video-colors", "Emulation.Atari.Video.Brightness",
                Value(configuration, AtariEightBitSettingsConstants.ColorBrightnessOptionKey,
                    AtariEightBitSettingsConstants.DefaultColorAdjustment), AtariEightBitSettingsCatalog.ContrastAndBrightness),
            Select(AtariEightBitSettingsConstants.ColorGammaOptionKey, EmulationMachineTab.Video,
                "video-colors", "Emulation.Video.Gamma",
                Value(configuration, AtariEightBitSettingsConstants.ColorGammaOptionKey,
                    AtariEightBitSettingsConstants.DefaultGamma), AtariEightBitSettingsCatalog.GammaValues),
            Select(AtariEightBitSettingsConstants.ColorDelayOptionKey, EmulationMachineTab.Video,
                "video-colors", "Emulation.Atari.Video.ColorDelay",
                Value(configuration, AtariEightBitSettingsConstants.ColorDelayOptionKey,
                    AtariEightBitSettingsConstants.DefaultColorDelay), AtariEightBitSettingsCatalog.ColorDelayValues),
            Select(AtariEightBitSettingsConstants.ExternalPaletteOptionKey, EmulationMachineTab.Video,
                "video-colors", "Emulation.Atari.Video.ExternalPalette",
                Value(configuration, AtariEightBitSettingsConstants.ExternalPaletteOptionKey,
                    AtariEightBitSettingsConstants.None), AtariEightBitSettingsCatalog.ExternalPalettes)));
        blocks.Add(Block("pokey", EmulationMachineTab.Audio, "Emulation.Audio", "\uE767", 1,
            Toggle(AtariEightBitSettingsConstants.PokeyStereoOptionKey, EmulationMachineTab.Audio, "pokey",
                "Emulation.Atari.Audio.PokeyStereo", Enabled(configuration,
                    AtariEightBitSettingsConstants.PokeyStereoOptionKey))));
        blocks.Add(Block("storage-options", EmulationMachineTab.Storage, "Emulation.Storage.Device.List", "\uE7C3", 2,
            Toggle(AtariEightBitSettingsConstants.ShowActivityOptionKey, EmulationMachineTab.Storage,
                "storage-options", "Emulation.Storage.ActivityOsd", Enabled(configuration,
                    AtariEightBitSettingsConstants.ShowActivityOptionKey)),
            Toggle(AtariEightBitSettingsConstants.ShowSpeedOptionKey, EmulationMachineTab.Storage,
                "storage-options", "Emulation.Atari.Storage.SpeedOsd", Enabled(configuration,
                    AtariEightBitSettingsConstants.ShowSpeedOptionKey)),
            Toggle(AtariEightBitSettingsConstants.ShowSectorOptionKey, EmulationMachineTab.Storage,
                "storage-options", "Emulation.Atari.Storage.SectorOsd", Enabled(configuration,
                    AtariEightBitSettingsConstants.ShowSectorOptionKey)),
            Toggle(AtariEightBitSettingsConstants.SioAccelerationOptionKey, EmulationMachineTab.Storage,
                "storage-options", "Emulation.Atari.Storage.SioAcceleration", Enabled(configuration,
                    AtariEightBitSettingsConstants.SioAccelerationOptionKey)),
            Toggle(AtariEightBitSettingsConstants.CassetteBootOptionKey, EmulationMachineTab.Storage,
                "storage-options", "Emulation.Atari.Storage.CassetteBoot", Enabled(configuration,
                    AtariEightBitSettingsConstants.CassetteBootOptionKey)),
            Toggle(AtariEightBitSettingsConstants.RealTimeClockOptionKey, EmulationMachineTab.Storage,
                "storage-options", "Emulation.Atari.Storage.RealTimeClock", Enabled(configuration,
                    AtariEightBitSettingsConstants.RealTimeClockOptionKey)),
            Toggle(AtariEightBitSettingsConstants.PrinterDeviceOptionKey, EmulationMachineTab.Storage,
                "storage-options", "Emulation.Atari.Storage.PrinterDevice", Enabled(configuration,
                    AtariEightBitSettingsConstants.PrinterDeviceOptionKey)),
            Toggle(AtariEightBitSettingsConstants.SerialDeviceOptionKey, EmulationMachineTab.Storage,
                "storage-options", "Emulation.Atari.Storage.SerialDevice", Enabled(configuration,
                    AtariEightBitSettingsConstants.SerialDeviceOptionKey))));
    }

    private static void AddEightBitMemory(AtariMachineConfiguration configuration,
        ICollection<EmulationSettingsBlock> blocks)
    {
        var mosaic = AtariEightBitSettingsCatalog.Mosaic(configuration.Model);
        var axlon = AtariEightBitSettingsCatalog.Axlon(configuration.Model);
        if (mosaic.Count == 0 && axlon.Count == 0) return;
        blocks.Add(Block("extension-memory", EmulationMachineTab.Ram, "Emulation.Memory.Extensions", "\uE964", 2,
            Select(AtariEightBitSettingsConstants.MosaicMemoryOptionKey, EmulationMachineTab.Ram,
                "extension-memory", "Emulation.Atari.Memory.Mosaic",
                Value(configuration, AtariEightBitSettingsConstants.MosaicMemoryOptionKey,
                    mosaic.FirstOrDefault()?.Value ?? AtariEightBitSettingsConstants.Disabled),
                mosaic.Select(AtariHardwareSettingsFunctions.Expansion)),
            Select(AtariEightBitSettingsConstants.AxlonMemoryOptionKey, EmulationMachineTab.Ram,
                "extension-memory", "Emulation.Atari.Memory.Axlon",
                Value(configuration, AtariEightBitSettingsConstants.AxlonMemoryOptionKey,
                    axlon.FirstOrDefault()?.Value ?? AtariEightBitSettingsConstants.Disabled),
                axlon.Select(AtariHardwareSettingsFunctions.Expansion)),
            Toggle(AtariEightBitSettingsConstants.AxlonShadowOptionKey, EmulationMachineTab.Ram,
                "extension-memory", "Emulation.Atari.Memory.AxlonShadow",
                Enabled(configuration, AtariEightBitSettingsConstants.AxlonShadowOptionKey)),
            Toggle(AtariEightBitSettingsConstants.MapRamOptionKey, EmulationMachineTab.Ram,
                "extension-memory", "Emulation.Atari.Memory.MapRam",
                Enabled(configuration, AtariEightBitSettingsConstants.MapRamOptionKey))));
    }

    private static EmulationSettingsBlock Audio(AtariMachineConfiguration configuration, bool isHatari) =>
        Block("audio", EmulationMachineTab.Audio, "Emulation.Audio", "\uE767", 2,
            Toggle(AtariSettingsConstants.AudioEnabled, EmulationMachineTab.Audio, "audio",
                "Emulation.Audio.Enabled", configuration.AudioEnabled),
            Select(AtariVideoAudioSettingsConstants.AudioOutputOption, EmulationMachineTab.Audio, "audio",
                "Emulation.Audio.Output", Value(configuration, AtariVideoAudioSettingsConstants.AudioOutputOption,
                    AtariConfigurationOptionConstants.DefaultAudioOutput), DefaultAudioOutput()) with
            { ChoiceSource = EmulationSettingsChoiceSource.AudioOutputDevices },
            Select(AtariVideoAudioSettingsConstants.AudioLatencyOption, EmulationMachineTab.Audio, "audio",
                "Emulation.Audio.Latency", Value(configuration, AtariVideoAudioSettingsConstants.AudioLatencyOption,
                    AtariConfigurationOptionConstants.DefaultAudioLatencyMilliseconds.ToString()),
                AtariVideoAudioSettingsConstants.AudioLatenciesMilliseconds.Select(Milliseconds)),
            Select(AtariVideoAudioSettingsConstants.AudioVolumeOption, EmulationMachineTab.Audio, "audio",
                "Explorer.Volume", Value(configuration, AtariVideoAudioSettingsConstants.AudioVolumeOption,
                    AtariConfigurationOptionConstants.DefaultAudioVolumePercent.ToString()), Percentages(
                        AtariVideoAudioSettingsConstants.MinimumVolumePercent,
                        AtariVideoAudioSettingsConstants.MaximumVolumePercent,
                        AtariVideoAudioSettingsConstants.VolumeStepPercent)),
            Toggle(AtariVideoAudioSettingsConstants.FloppySoundOption, EmulationMachineTab.Audio, "audio",
                "Emulation.Audio.Floppy.Enabled", Value(configuration,
                    AtariVideoAudioSettingsConstants.FloppySoundOption, "true") == "true",
                "true", "false") with
            { IsVisible = isHatari },
            Select(AtariVideoAudioSettingsConstants.FloppySoundVolumeOption, EmulationMachineTab.Audio, "audio",
                "Emulation.Audio.Floppy.Sound", Value(configuration,
                    AtariVideoAudioSettingsConstants.FloppySoundVolumeOption, "75"),
                AtariVideoAudioSettingsConstants.FloppySoundVolumesPercent.Select(Percentage))
                with
            { IsVisible = isHatari },
            Toggle(AtariVideoAudioSettingsConstants.PolarizedFilterOption, EmulationMachineTab.Audio, "audio",
                "Emulation.Audio.PolarizedFilter", Value(configuration,
                    AtariVideoAudioSettingsConstants.PolarizedFilterOption, "false") == "true",
                "true", "false") with
            { IsVisible = isHatari });

    private static IReadOnlyList<EmulationSettingsChoice> StStandards(AtariStModelDefinition model)
    {
        var choices = new List<EmulationSettingsChoice>
        {
            new(AtariVideoAudioSettingsConstants.Automatic, "Visual.Automatic")
        };
        if (model.Video.Contains(AtariStVideoCapability.Pal)) choices.Add(Invariant("PAL"));
        if (model.Video.Contains(AtariStVideoCapability.Ntsc)) choices.Add(Invariant("NTSC"));
        if (model.Video.Contains(AtariStVideoCapability.Monochrome)) choices.Add(Invariant("Monochrome"));
        return choices;
    }

    private static IReadOnlyList<EmulationSettingsChoice> AutomaticAndNative() =>
    [
        new(AtariVideoAudioSettingsConstants.Automatic, "Visual.Automatic"),
        Invariant(AtariVideoAudioSettingsConstants.Native)
    ];

    private static IReadOnlyList<EmulationSettingsChoice> AspectRatios() =>
    [
        new(AtariVideoAudioSettingsConstants.Automatic, "Visual.Automatic"),
        Invariant(AtariVideoAudioSettingsConstants.FourByThree),
        Invariant(AtariVideoAudioSettingsConstants.PixelAspect)
    ];

    private static IReadOnlyList<EmulationSettingsChoice> FrameSkips() =>
        Enumerable.Range(AtariVideoAudioSettingsConstants.MinimumFrameSkip,
                AtariVideoAudioSettingsConstants.MaximumFrameSkip
                - AtariVideoAudioSettingsConstants.MinimumFrameSkip + 1)
            .Select(value => Invariant(value.ToString())).Append(Invariant("10")).ToArray();

    private static IReadOnlyList<EmulationSettingsChoice> Resolutions(AtariMachineModel model) =>
        AtariEightBitSettingsCatalog.SupportsOriginalComputerOptions(model)
            ? AtariEightBitSettingsCatalog.OriginalComputerResolutions.Select(value =>
                new EmulationSettingsChoice(value, string.Empty,
                    value.Replace("x", " × ", StringComparison.Ordinal))).ToArray()
            : AutomaticAndNative();

    private static string DefaultResolution(AtariMachineModel model) =>
        AtariEightBitSettingsCatalog.SupportsOriginalComputerOptions(model)
            ? AtariEightBitSettingsCatalog.OriginalComputerResolutions[0]
            : AtariVideoAudioSettingsConstants.Automatic;

    private static IReadOnlyList<EmulationSettingsChoice> DefaultAudioOutput() =>
    [
        new(AtariConfigurationOptionConstants.DefaultAudioOutput, "Emulation.Audio.DefaultOutput")
    ];

    private static EmulationSettingsChoice Milliseconds(int value) =>
        new(value.ToString(), string.Empty, $"{value} ms", value);

    private static EmulationSettingsChoice Percentage(int value) =>
        new(value.ToString(), string.Empty, $"{value} %", value);

    private static IReadOnlyList<EmulationSettingsChoice> Percentages(int minimum, int maximum, int step) =>
        Enumerable.Range(0, (maximum - minimum) / step + 1)
            .Select(index => Percentage(minimum + index * step)).ToArray();

    private static EmulationSettingsChoice Invariant(string value) => new(value, string.Empty, value);

    private static EmulationSettingsField Renderer(AtariMachineConfiguration configuration) =>
        Select(AtariSettingsConstants.VideoRenderer, EmulationMachineTab.Video, "video",
            "Emulation.Video.Settings.Rendering", configuration.VideoRenderer.ToString(),
            Enum.GetNames<EmulationVideoRenderer>());

    private static EmulationSettingsBlock Block(string id, EmulationMachineTab tab, string title,
        string icon, int columns, params EmulationSettingsField[] fields) =>
        new(id, tab, title, fields, icon, columns);

    private static EmulationSettingsField Select(string id, EmulationMachineTab tab, string block,
        string label, string value, IEnumerable<string> choices, bool isEnabled = true) =>
        new(id, tab, block, label, EmulationSettingsEditor.Selection, value,
            choices.Select(choice => new EmulationSettingsChoice(choice, choice, choice)).ToArray(), isEnabled);

    private static EmulationSettingsField Select(string id, EmulationMachineTab tab, string block,
        string label, string value, IEnumerable<EmulationSettingsChoice> choices, bool isEnabled = true) =>
        new(id, tab, block, label, EmulationSettingsEditor.Selection, value, choices.ToArray(), isEnabled);

    private static EmulationSettingsField Toggle(string id, EmulationMachineTab tab, string block,
        string label, bool value, string enabledValue = "enabled", string disabledValue = "disabled") =>
        new(id, tab, block, label, EmulationSettingsEditor.Toggle,
            value ? enabledValue : disabledValue, EnabledValue: enabledValue, DisabledValue: disabledValue);

    private static EmulationSettingsField Path(string id, EmulationMachineTab tab, string block,
        string label, string? value) => new(id, tab, block, label, EmulationSettingsEditor.Path, value,
            DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware);

    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block,
        string label, string value, long? numericValue = null) => new(id, tab, block, label,
            EmulationSettingsEditor.Information, value, IsEnabled: false, NumericValue: numericValue);

    private static string Value(AtariMachineConfiguration configuration, string key, string fallback) =>
        configuration.Options.GetValueOrDefault(key) ?? fallback;

    private static bool Enabled(AtariMachineConfiguration configuration, string key) =>
        Value(configuration, key, AtariEightBitSettingsConstants.Disabled) == AtariEightBitSettingsConstants.Enabled;

    private static EmulationSettingsField ClassicMemory(AtariMachineConfiguration configuration,
        AtariClassicModelDefinition model)
    {
        if (configuration.Model != AtariMachineModel.XlXe)
            return Information(AtariConfigurationOptionConstants.MainMemory, EmulationMachineTab.Ram,
                "main-memory", "Emulation.Memory.Main", AtariHardwareSettingsFunctions.FormatBytes(model.MainMemoryBytes),
                model.MainMemoryBytes);
        var choices = new[] { 320L, 576L, 1088L }.Select(value =>
            AtariHardwareSettingsFunctions.MemoryKib((int)value)).ToArray();
        return Select(AtariConfigurationOptionConstants.MainMemory, EmulationMachineTab.Ram, "main-memory",
            "Emulation.Memory.Main", Value(configuration, AtariConfigurationOptionConstants.MainMemory,
                model.MainMemoryBytes.ToString()), choices);
    }
}
