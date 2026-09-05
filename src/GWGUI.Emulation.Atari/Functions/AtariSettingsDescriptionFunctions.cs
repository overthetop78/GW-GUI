using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariSettingsDescriptionFunctions
{
    private static readonly IReadOnlyDictionary<string, string> FieldHelpResources =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [AtariSettingsConstants.Fpu] = "Emulation.Help.Cpu.FpuModel",
            [AtariSettingsConstants.CpuPrecision] = "Emulation.Help.Cpu.Precision",
            [AtariSettingsConstants.CpuFrequency] = "Emulation.Help.Cpu.Speed",
            [AtariSettingsConstants.AlternateMemory] = "Emulation.Help.Memory.Extensions",
            [AtariEightBitSettingsConstants.MosaicMemoryOptionKey] = "Emulation.Help.Memory.Mosaic",
            [AtariEightBitSettingsConstants.AxlonMemoryOptionKey] = "Emulation.Help.Memory.Axlon",
            [AtariEightBitSettingsConstants.AxlonShadowOptionKey] = "Emulation.Help.Memory.AxlonShadow",
            [AtariEightBitSettingsConstants.MapRamOptionKey] = "Emulation.Help.Memory.MapRam",
            [AtariSettingsDescriptionFunctionsConstants.HatariFastboot] = "Emulation.Help.Firmware.FastBoot",
            [AtariVideoAudioSettingsConstants.StandardOption] = "Emulation.Help.Video.Standard",
            [AtariConfigurationOptionConstants.VideoStandard] = "Emulation.Help.Video.Standard",
            [AtariVideoAudioSettingsConstants.AspectRatioOption] = "Emulation.Help.Video.AspectRatio",
            [AtariVideoAudioSettingsConstants.FrameSkipOption] = "Emulation.Help.Video.FrameSkip",
            [AtariSettingsConstants.Region] = "Emulation.Help.Video.Region",
            [AtariEightBitSettingsConstants.ArtifactingModeOptionKey] = "Emulation.Help.Video.Artifacting",
            [AtariEightBitSettingsConstants.ColorGammaOptionKey] = "Emulation.Help.Video.Gamma",
            [AtariEightBitSettingsConstants.ColorDelayOptionKey] = "Emulation.Help.Video.ColorDelay",
            [AtariEightBitSettingsConstants.ExternalPaletteOptionKey] = "Emulation.Help.Video.ExternalPalette",
            [AtariVideoAudioSettingsConstants.AudioLatencyOption] = "Emulation.Help.Audio.Latency",
            [AtariVideoAudioSettingsConstants.PolarizedFilterOption] = "Emulation.Help.Audio.PolarizedFilter",
            [AtariEightBitSettingsConstants.PokeyStereoOptionKey] = "Emulation.Help.Audio.PokeyStereo",
            [AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey] = "Emulation.Help.Controller.Compatibility",
            [AtariEightBitSettingsConstants.DigitalSensitivityOptionKey] = "Emulation.Help.Controller.DigitalSensitivity",
            [AtariEightBitSettingsConstants.AnalogSensitivityOptionKey] = "Emulation.Help.Controller.AnalogSensitivity",
            [AtariEightBitSettingsConstants.AutofireOptionKey] = "Emulation.Help.Controller.Autofire",
            [AtariEightBitSettingsConstants.PaddleMovementSpeedOptionKey] = "Emulation.Help.Controller.PaddleSpeed",
            [AtariEightBitSettingsConstants.SioAccelerationOptionKey] = "Emulation.Help.Storage.SioAcceleration",
            [AtariEightBitSettingsConstants.CassetteBootOptionKey] = "Emulation.Help.Storage.CassetteBoot",
            [AtariEightBitSettingsConstants.RealTimeClockOptionKey] = "Emulation.Help.Storage.RealTimeClock",
            [AtariEightBitSettingsConstants.PrinterDeviceOptionKey] = "Emulation.Help.Storage.PrinterDevice",
            [AtariEightBitSettingsConstants.SerialDeviceOptionKey] = "Emulation.Help.Storage.SerialDevice"
        };

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
        blocks.Add(Block(AtariSettingsDescriptionFunctionsConstants.Mouse, EmulationMachineTab.Mouse, AtariSettingsDescriptionFunctionsConstants.ResourceTabMouse, AtariSettingsDescriptionFunctionsConstants.Value, 1,
            Select(AtariMouseSettingsConstants.SpeedOptionKey, EmulationMachineTab.Mouse, AtariSettingsDescriptionFunctionsConstants.Mouse,
                AtariSettingsDescriptionFunctionsConstants.ResourceMouseSpeed, Value(configuration, AtariMouseSettingsConstants.SpeedOptionKey,
                    AtariMouseSettingsConstants.DefaultSpeedPercent.ToString()), choices)));
    }

    private static void AddGeneralFolders(AtariMachineConfiguration configuration,
        AtariCompatibilityDefinition compatibility, ICollection<EmulationSettingsBlock> blocks)
    {
        var supportsHardDisk = AtariEightBitSettingsCatalog.SupportsComputerOptions(configuration.Model)
            || compatibility.Media.Any(rule => rule.Availability == AtariMediaAvailability.Available
                && rule.Category is AtariMediaCategory.HardDisk or AtariMediaCategory.Directory);
        if (!supportsHardDisk) return;
        blocks.Add(Block(AtariSettingsDescriptionFunctionsConstants.DefaultFolders, EmulationMachineTab.General, AtariSettingsDescriptionFunctionsConstants.ResourceFolderDefault, AtariSettingsDescriptionFunctionsConstants.Value2, 1,
            new EmulationSettingsField(AtariSettingsConstants.HardDiskFolder, EmulationMachineTab.General,
                AtariSettingsDescriptionFunctionsConstants.DefaultFolders, AtariSettingsDescriptionFunctionsConstants.ResourceStorageHardDiskList, EmulationSettingsEditor.DirectoryPath,
                configuration.Folders.HardDisks,
                DefaultFolderCategory: EmulationDefaultFolderCategory.HardDisk)));
    }

    private static IReadOnlyList<EmulationSettingsBlock> CreateSt(AtariMachineConfiguration configuration)
    {
        var model = AtariStModelCatalog.Get(configuration.Model);
        return
        [
            Block(AtariSettingsDescriptionFunctionsConstants.Processor, EmulationMachineTab.Cpu, AtariSettingsDescriptionFunctionsConstants.ResourceCpuProcessor, AtariSettingsDescriptionFunctionsConstants.Value3, 2,
                Select(AtariSettingsConstants.Cpu, EmulationMachineTab.Cpu, AtariSettingsDescriptionFunctionsConstants.Processor, AtariSettingsDescriptionFunctionsConstants.ResourceCpuModel,
                    Value(configuration, AtariSettingsConstants.Cpu, model.DefaultCpu.ToString()),
                    model.Cpus.Select(value => AtariHardwareSettingsFunctions.Invariant(value.ToString(), value.ToString()))),
                Select(AtariSettingsConstants.CpuPrecision, EmulationMachineTab.Cpu, AtariSettingsDescriptionFunctionsConstants.Processor, AtariSettingsDescriptionFunctionsConstants.ResourceCpuPrecision,
                    Value(configuration, AtariSettingsConstants.CpuPrecision, model.DefaultCpuPrecision.ToString()),
                    model.CpuPrecisions.Select(AtariHardwareSettingsFunctions.CpuPrecision)),
                Select(AtariSettingsConstants.Fpu, EmulationMachineTab.Cpu, AtariSettingsDescriptionFunctionsConstants.Processor, AtariSettingsDescriptionFunctionsConstants.ResourceFpuModel,
                    Value(configuration, AtariSettingsConstants.Fpu, model.DefaultFpu.ToString()),
                    model.Fpus.Select(AtariHardwareSettingsFunctions.Fpu)),
                Information(AtariSettingsConstants.CpuOriginalFrequency, EmulationMachineTab.Cpu, AtariSettingsDescriptionFunctionsConstants.Processor,
                    AtariSettingsDescriptionFunctionsConstants.ResourceCpuSpeedOriginal,
                    AtariHardwareSettingsFunctions.FrequencyMhz(model.DefaultCpuFrequencyMhz).InvariantDisplayValue!),
                Select(AtariSettingsConstants.CpuFrequency, EmulationMachineTab.Cpu, AtariSettingsDescriptionFunctionsConstants.Processor, AtariSettingsDescriptionFunctionsConstants.ResourceCpuSpeed,
                    Value(configuration, AtariSettingsConstants.CpuFrequency, model.DefaultCpuFrequencyMhz.ToString()),
                    model.CpuFrequenciesMhz.Select(AtariHardwareSettingsFunctions.FrequencyMhz))),
            Block(AtariSettingsDescriptionFunctionsConstants.MainMemory, EmulationMachineTab.Ram, AtariSettingsDescriptionFunctionsConstants.ResourceMemoryMain, AtariSettingsDescriptionFunctionsConstants.Value4, 2,
                Select(AtariConfigurationOptionConstants.MainMemory, EmulationMachineTab.Ram, AtariSettingsDescriptionFunctionsConstants.MainMemory, AtariSettingsDescriptionFunctionsConstants.ResourceMemoryMain,
                    Value(configuration, AtariConfigurationOptionConstants.MainMemory,
                        ((long)model.DefaultMainMemoryKib * AtariHardwareSettingsConstants.BytesPerKibibyte).ToString()),
                    model.MainMemoryKib.Select(AtariHardwareSettingsFunctions.MemoryKib))),
            Block(AtariSettingsDescriptionFunctionsConstants.ExtensionMemory, EmulationMachineTab.Ram, AtariSettingsDescriptionFunctionsConstants.ResourceMemoryExtensions, AtariSettingsDescriptionFunctionsConstants.Value4, 1,
                Select(AtariSettingsConstants.AlternateMemory, EmulationMachineTab.Ram, AtariSettingsDescriptionFunctionsConstants.ExtensionMemory, AtariSettingsDescriptionFunctionsConstants.ResourceMemoryExtensions,
                    Value(configuration, AtariSettingsConstants.AlternateMemory,
                        ((long)model.DefaultAlternateMemoryMib * AtariHardwareSettingsConstants.BytesPerMebibyte).ToString()),
                    model.AlternateMemoryMib.Select(AtariHardwareSettingsFunctions.MemoryMib))),
            Block(AtariSettingsDescriptionFunctionsConstants.Firmware, EmulationMachineTab.Rom, AtariSettingsDescriptionFunctionsConstants.ResourceFirmwareRomSystem, AtariSettingsDescriptionFunctionsConstants.Value5, 1,
                Path(AtariSettingsConstants.SystemFirmware, EmulationMachineTab.Rom, AtariSettingsDescriptionFunctionsConstants.Firmware,
                    AtariSettingsDescriptionFunctionsConstants.ResourceFirmwareRomSystem,
                    configuration.Firmwares.FirstOrDefault(item => item.Category == AtariFirmwareCategory.Tos)?.Path),
                Toggle(AtariSettingsDescriptionFunctionsConstants.HatariFastboot, EmulationMachineTab.Rom, AtariSettingsDescriptionFunctionsConstants.Firmware, AtariSettingsDescriptionFunctionsConstants.ResourceAtariFastBoot,
                    Value(configuration, AtariSettingsDescriptionFunctionsConstants.HatariFastboot, AtariSettingsDescriptionFunctionsConstants.False) == AtariSettingsDescriptionFunctionsConstants.True, AtariSettingsDescriptionFunctionsConstants.True, AtariSettingsDescriptionFunctionsConstants.False)),
            Block(AtariSettingsDescriptionFunctionsConstants.StorageOptions, EmulationMachineTab.Storage, AtariSettingsDescriptionFunctionsConstants.ResourceStorageDeviceList, AtariSettingsDescriptionFunctionsConstants.Value6, 1,
                Toggle(AtariMachineOptionConstants.DriveActivity, EmulationMachineTab.Storage,
                    AtariSettingsDescriptionFunctionsConstants.StorageOptions, AtariSettingsDescriptionFunctionsConstants.ResourceStorageActivityOsd,
                    Value(configuration, AtariMachineOptionConstants.DriveActivity, AtariSettingsDescriptionFunctionsConstants.False) == AtariSettingsDescriptionFunctionsConstants.True,
                    AtariSettingsDescriptionFunctionsConstants.True, AtariSettingsDescriptionFunctionsConstants.False)),
            Block(AtariSettingsDescriptionFunctionsConstants.Video, EmulationMachineTab.Video, AtariSettingsDescriptionFunctionsConstants.ResourceVideoSettingsDisplay, AtariSettingsDescriptionFunctionsConstants.Value7, 2,
                Select(AtariVideoAudioSettingsConstants.StandardOption, EmulationMachineTab.Video, AtariSettingsDescriptionFunctionsConstants.Video,
                    AtariSettingsDescriptionFunctionsConstants.ResourceVideoStandard, Value(configuration, AtariVideoAudioSettingsConstants.StandardOption,
                        AtariVideoAudioSettingsConstants.Automatic), StStandards(model)),
                Select(AtariSettingsConstants.Region, EmulationMachineTab.Video, AtariSettingsDescriptionFunctionsConstants.Video, AtariSettingsDescriptionFunctionsConstants.ResourceAtariVideoRegion,
                    Value(configuration, AtariSettingsConstants.Region, model.DefaultRegion.ToString()),
                    model.Regions.Select(AtariHardwareSettingsFunctions.StRegion)),
                Select(AtariVideoAudioSettingsConstants.ResolutionOption, EmulationMachineTab.Video, AtariSettingsDescriptionFunctionsConstants.Video,
                    AtariSettingsDescriptionFunctionsConstants.ResourceVideoResolution, Value(configuration, AtariVideoAudioSettingsConstants.ResolutionOption,
                        AtariVideoAudioSettingsConstants.Automatic), AutomaticAndNative()),
                Select(AtariVideoAudioSettingsConstants.AspectRatioOption, EmulationMachineTab.Video, AtariSettingsDescriptionFunctionsConstants.Video,
                    AtariSettingsDescriptionFunctionsConstants.ResourceVideoAspectRatio, Value(configuration, AtariVideoAudioSettingsConstants.AspectRatioOption,
                        AtariVideoAudioSettingsConstants.Automatic), AspectRatios()),
                Toggle(AtariVideoAudioSettingsConstants.CropOption, EmulationMachineTab.Video, AtariSettingsDescriptionFunctionsConstants.Video,
                    AtariSettingsDescriptionFunctionsConstants.ResourceVideoCrop, Value(configuration, AtariVideoAudioSettingsConstants.CropOption,
                        AtariVideoAudioSettingsConstants.Disabled) == AtariVideoAudioSettingsConstants.Enabled),
                Select(AtariVideoAudioSettingsConstants.FrameSkipOption, EmulationMachineTab.Video, AtariSettingsDescriptionFunctionsConstants.Video,
                    AtariSettingsDescriptionFunctionsConstants.ResourceVideoFrameSkip, Value(configuration, AtariVideoAudioSettingsConstants.FrameSkipOption,
                        AtariVideoAudioSettingsConstants.MinimumFrameSkip.ToString()), FrameSkips())),
            Audio(configuration, true)
        ];
    }

    private static IReadOnlyList<EmulationSettingsBlock> CreateClassic(AtariMachineConfiguration configuration)
    {
        var model = AtariClassicModelCatalog.Get(configuration.Model);
        var blocks = new List<EmulationSettingsBlock>
        {
            Block(AtariSettingsDescriptionFunctionsConstants.Processor, EmulationMachineTab.Cpu, AtariSettingsDescriptionFunctionsConstants.ResourceCpuProcessor, AtariSettingsDescriptionFunctionsConstants.Value3, 2,
                Select(AtariSettingsConstants.Cpu, EmulationMachineTab.Cpu, AtariSettingsDescriptionFunctionsConstants.Processor, AtariSettingsDescriptionFunctionsConstants.ResourceCpuModel,
                    Value(configuration, AtariSettingsConstants.Cpu, model.DefaultCpu.ToString()), model.Cpus.Select(value => value.ToString()),
                    isEnabled: model.Cpus.Count > 1),
                Information(AtariSettingsConstants.CpuOriginalFrequency, EmulationMachineTab.Cpu, AtariSettingsDescriptionFunctionsConstants.Processor, AtariSettingsDescriptionFunctionsConstants.ResourceCpuSpeedOriginal,
                    $"{model.DefaultCpuFrequencyHz / 1_000_000d:0.00} MHz")),
            Block(AtariSettingsDescriptionFunctionsConstants.MainMemory, EmulationMachineTab.Ram, AtariSettingsDescriptionFunctionsConstants.ResourceMemoryMain, AtariSettingsDescriptionFunctionsConstants.Value4, 1,
                ClassicMemory(configuration, model)),
            Block(AtariSettingsDescriptionFunctionsConstants.Video, EmulationMachineTab.Video, AtariSettingsDescriptionFunctionsConstants.ResourceVideoSettingsDisplay, AtariSettingsDescriptionFunctionsConstants.Value7, 2,
                Select(AtariConfigurationOptionConstants.VideoStandard, EmulationMachineTab.Video, AtariSettingsDescriptionFunctionsConstants.Video,
                    AtariSettingsDescriptionFunctionsConstants.ResourceVideoStandard, Value(configuration, AtariConfigurationOptionConstants.VideoStandard,
                        model.DefaultRegion.ToString()), model.Regions.Select(AtariHardwareSettingsFunctions.ClassicRegion),
                    isEnabled: model.Regions.Count > 1),
                Select(AtariVideoAudioSettingsConstants.ResolutionOption, EmulationMachineTab.Video, AtariSettingsDescriptionFunctionsConstants.Video,
                    AtariSettingsDescriptionFunctionsConstants.ResourceVideoResolution, Value(configuration, AtariVideoAudioSettingsConstants.ResolutionOption,
                        DefaultResolution(configuration.Model)), Resolutions(configuration.Model))),
            Audio(configuration, false)
        };
        if (configuration.Model == AtariMachineModel.Atari400)
        {
            blocks.Add(Block(AtariSettingsDescriptionFunctionsConstants.Firmware, EmulationMachineTab.Rom, AtariSettingsDescriptionFunctionsConstants.ResourceFirmwareRomSystem, AtariSettingsDescriptionFunctionsConstants.Value5, 1,
                Path(AtariSettingsConstants.SystemFirmware, EmulationMachineTab.Rom, AtariSettingsDescriptionFunctionsConstants.Firmware,
                    AtariSettingsDescriptionFunctionsConstants.ResourceFirmwareRomSystem, configuration.Firmwares.FirstOrDefault()?.Path)));
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
        blocks.Add(Block(AtariSettingsDescriptionFunctionsConstants.ControllerOptions, EmulationMachineTab.Controllers,
            AtariSettingsDescriptionFunctionsConstants.ResourceControllerTab, AtariSettingsDescriptionFunctionsConstants.Value8, 2,
            Select(AtariEightBitSettingsConstants.PaddleMovementSpeedOptionKey,
                EmulationMachineTab.Controllers, AtariSettingsDescriptionFunctionsConstants.ControllerOptions, AtariSettingsDescriptionFunctionsConstants.ResourceAtariControllerPaddleSpeed,
                Value(configuration, AtariEightBitSettingsConstants.PaddleMovementSpeedOptionKey,
                    AtariEightBitSettingsConstants.DefaultPaddleMovementSpeed),
                AtariEightBitSettingsCatalog.PaddleMovementSpeeds),
            Select(AtariEightBitSettingsConstants.AutofireOptionKey,
                EmulationMachineTab.Controllers, AtariSettingsDescriptionFunctionsConstants.ControllerOptions, AtariSettingsDescriptionFunctionsConstants.ResourceAtariControllerAutofire,
                Value(configuration, AtariEightBitSettingsConstants.AutofireOptionKey,
                    AtariEightBitSettingsConstants.Disabled), AtariEightBitSettingsCatalog.AutofireModes),
            Select(AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey,
                EmulationMachineTab.Controllers, AtariSettingsDescriptionFunctionsConstants.ControllerOptions, AtariSettingsDescriptionFunctionsConstants.ResourceAtariControllerCompatibility,
                Value(configuration, AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey,
                    AtariEightBitSettingsConstants.None), AtariEightBitSettingsCatalog.ControllerCompatibilityModes),
            Select(AtariEightBitSettingsConstants.DigitalSensitivityOptionKey,
                EmulationMachineTab.Controllers, AtariSettingsDescriptionFunctionsConstants.ControllerOptions, AtariSettingsDescriptionFunctionsConstants.ResourceAtariControllerDigitalSensitivity,
                Value(configuration, AtariEightBitSettingsConstants.DigitalSensitivityOptionKey,
                    AtariEightBitSettingsConstants.DefaultSensitivity), AtariEightBitSettingsCatalog.Sensitivities),
            Select(AtariEightBitSettingsConstants.AnalogSensitivityOptionKey,
                EmulationMachineTab.Controllers, AtariSettingsDescriptionFunctionsConstants.ControllerOptions, AtariSettingsDescriptionFunctionsConstants.ResourceAtariControllerAnalogSensitivity,
                Value(configuration, AtariEightBitSettingsConstants.AnalogSensitivityOptionKey,
                    AtariEightBitSettingsConstants.DefaultSensitivity), AtariEightBitSettingsCatalog.Sensitivities)));
    }

    private static void AddEightBitOptions(AtariMachineConfiguration configuration,
        ICollection<EmulationSettingsBlock> blocks)
    {
        if (!AtariEightBitSettingsCatalog.SupportsComputerOptions(configuration.Model)) return;
        blocks.Add(Block(AtariSettingsDescriptionFunctionsConstants.VideoColors, EmulationMachineTab.Video, AtariSettingsDescriptionFunctionsConstants.ResourceVideoSettingsDisplay, AtariSettingsDescriptionFunctionsConstants.Value7, 2,
            Select(AtariEightBitSettingsConstants.ArtifactingModeOptionKey, EmulationMachineTab.Video,
                AtariSettingsDescriptionFunctionsConstants.VideoColors, AtariSettingsDescriptionFunctionsConstants.ResourceAtariVideoArtifacting,
                Value(configuration, AtariEightBitSettingsConstants.ArtifactingModeOptionKey,
                    AtariEightBitSettingsConstants.None), AtariEightBitSettingsCatalog.ArtifactingModes),
            Select(AtariEightBitSettingsConstants.ColorHueOptionKey, EmulationMachineTab.Video, AtariSettingsDescriptionFunctionsConstants.VideoColors,
                AtariSettingsDescriptionFunctionsConstants.ResourceAtariVideoHue, Value(configuration, AtariEightBitSettingsConstants.ColorHueOptionKey,
                    AtariEightBitSettingsConstants.DefaultColorAdjustment), AtariEightBitSettingsCatalog.ColorAdjustments),
            Select(AtariEightBitSettingsConstants.ColorSaturationOptionKey, EmulationMachineTab.Video,
                AtariSettingsDescriptionFunctionsConstants.VideoColors, AtariSettingsDescriptionFunctionsConstants.ResourceAtariVideoSaturation,
                Value(configuration, AtariEightBitSettingsConstants.ColorSaturationOptionKey,
                    AtariEightBitSettingsConstants.DefaultColorAdjustment), AtariEightBitSettingsCatalog.ColorAdjustments),
            Select(AtariEightBitSettingsConstants.ColorContrastOptionKey, EmulationMachineTab.Video,
                AtariSettingsDescriptionFunctionsConstants.VideoColors, AtariSettingsDescriptionFunctionsConstants.ResourceAtariVideoContrast,
                Value(configuration, AtariEightBitSettingsConstants.ColorContrastOptionKey,
                    AtariEightBitSettingsConstants.DefaultColorAdjustment), AtariEightBitSettingsCatalog.ContrastAndBrightness),
            Select(AtariEightBitSettingsConstants.ColorBrightnessOptionKey, EmulationMachineTab.Video,
                AtariSettingsDescriptionFunctionsConstants.VideoColors, AtariSettingsDescriptionFunctionsConstants.ResourceAtariVideoBrightness,
                Value(configuration, AtariEightBitSettingsConstants.ColorBrightnessOptionKey,
                    AtariEightBitSettingsConstants.DefaultColorAdjustment), AtariEightBitSettingsCatalog.ContrastAndBrightness),
            Select(AtariEightBitSettingsConstants.ColorGammaOptionKey, EmulationMachineTab.Video,
                AtariSettingsDescriptionFunctionsConstants.VideoColors, AtariSettingsDescriptionFunctionsConstants.ResourceVideoGamma,
                Value(configuration, AtariEightBitSettingsConstants.ColorGammaOptionKey,
                    AtariEightBitSettingsConstants.DefaultGamma), AtariEightBitSettingsCatalog.GammaValues),
            Select(AtariEightBitSettingsConstants.ColorDelayOptionKey, EmulationMachineTab.Video,
                AtariSettingsDescriptionFunctionsConstants.VideoColors, AtariSettingsDescriptionFunctionsConstants.ResourceAtariVideoColorDelay,
                Value(configuration, AtariEightBitSettingsConstants.ColorDelayOptionKey,
                    AtariEightBitSettingsConstants.DefaultColorDelay), AtariEightBitSettingsCatalog.ColorDelayValues),
            Select(AtariEightBitSettingsConstants.ExternalPaletteOptionKey, EmulationMachineTab.Video,
                AtariSettingsDescriptionFunctionsConstants.VideoColors, AtariSettingsDescriptionFunctionsConstants.ResourceAtariVideoExternalPalette,
                Value(configuration, AtariEightBitSettingsConstants.ExternalPaletteOptionKey,
                    AtariEightBitSettingsConstants.None), AtariEightBitSettingsCatalog.ExternalPalettes)));
        blocks.Add(Block(AtariSettingsDescriptionFunctionsConstants.Pokey, EmulationMachineTab.Audio, AtariSettingsDescriptionFunctionsConstants.ResourceAudio, AtariSettingsDescriptionFunctionsConstants.Value9, 1,
            Toggle(AtariEightBitSettingsConstants.PokeyStereoOptionKey, EmulationMachineTab.Audio, AtariSettingsDescriptionFunctionsConstants.Pokey,
                AtariSettingsDescriptionFunctionsConstants.ResourceAtariAudioPokeyStereo, Enabled(configuration,
                    AtariEightBitSettingsConstants.PokeyStereoOptionKey))));
        blocks.Add(Block(AtariSettingsDescriptionFunctionsConstants.StorageOptions, EmulationMachineTab.Storage, AtariSettingsDescriptionFunctionsConstants.ResourceStorageDeviceList, AtariSettingsDescriptionFunctionsConstants.Value6, 2,
            Toggle(AtariEightBitSettingsConstants.ShowActivityOptionKey, EmulationMachineTab.Storage,
                AtariSettingsDescriptionFunctionsConstants.StorageOptions, AtariSettingsDescriptionFunctionsConstants.ResourceStorageActivityOsd, Enabled(configuration,
                    AtariEightBitSettingsConstants.ShowActivityOptionKey)),
            Toggle(AtariEightBitSettingsConstants.ShowSpeedOptionKey, EmulationMachineTab.Storage,
                AtariSettingsDescriptionFunctionsConstants.StorageOptions, AtariSettingsDescriptionFunctionsConstants.ResourceAtariStorageSpeedOsd, Enabled(configuration,
                    AtariEightBitSettingsConstants.ShowSpeedOptionKey)),
            Toggle(AtariEightBitSettingsConstants.ShowSectorOptionKey, EmulationMachineTab.Storage,
                AtariSettingsDescriptionFunctionsConstants.StorageOptions, AtariSettingsDescriptionFunctionsConstants.ResourceAtariStorageSectorOsd, Enabled(configuration,
                    AtariEightBitSettingsConstants.ShowSectorOptionKey)),
            Toggle(AtariEightBitSettingsConstants.SioAccelerationOptionKey, EmulationMachineTab.Storage,
                AtariSettingsDescriptionFunctionsConstants.StorageOptions, AtariSettingsDescriptionFunctionsConstants.ResourceAtariStorageSioAcceleration, Enabled(configuration,
                    AtariEightBitSettingsConstants.SioAccelerationOptionKey)),
            Toggle(AtariEightBitSettingsConstants.CassetteBootOptionKey, EmulationMachineTab.Storage,
                AtariSettingsDescriptionFunctionsConstants.StorageOptions, AtariSettingsDescriptionFunctionsConstants.ResourceAtariStorageCassetteBoot, Enabled(configuration,
                    AtariEightBitSettingsConstants.CassetteBootOptionKey)),
            Toggle(AtariEightBitSettingsConstants.RealTimeClockOptionKey, EmulationMachineTab.Storage,
                AtariSettingsDescriptionFunctionsConstants.StorageOptions, AtariSettingsDescriptionFunctionsConstants.ResourceAtariStorageRealTimeClock, Enabled(configuration,
                    AtariEightBitSettingsConstants.RealTimeClockOptionKey)),
            Toggle(AtariEightBitSettingsConstants.PrinterDeviceOptionKey, EmulationMachineTab.Storage,
                AtariSettingsDescriptionFunctionsConstants.StorageOptions, AtariSettingsDescriptionFunctionsConstants.ResourceAtariStoragePrinterDevice, Enabled(configuration,
                    AtariEightBitSettingsConstants.PrinterDeviceOptionKey)),
            Toggle(AtariEightBitSettingsConstants.SerialDeviceOptionKey, EmulationMachineTab.Storage,
                AtariSettingsDescriptionFunctionsConstants.StorageOptions, AtariSettingsDescriptionFunctionsConstants.ResourceAtariStorageSerialDevice, Enabled(configuration,
                    AtariEightBitSettingsConstants.SerialDeviceOptionKey))));
    }

    private static void AddEightBitMemory(AtariMachineConfiguration configuration,
        ICollection<EmulationSettingsBlock> blocks)
    {
        var mosaic = AtariEightBitSettingsCatalog.Mosaic(configuration.Model);
        var axlon = AtariEightBitSettingsCatalog.Axlon(configuration.Model);
        if (mosaic.Count == 0 && axlon.Count == 0) return;
        blocks.Add(Block(AtariSettingsDescriptionFunctionsConstants.ExtensionMemory, EmulationMachineTab.Ram, AtariSettingsDescriptionFunctionsConstants.ResourceMemoryExtensions, AtariSettingsDescriptionFunctionsConstants.Value4, 2,
            Select(AtariEightBitSettingsConstants.MosaicMemoryOptionKey, EmulationMachineTab.Ram,
                AtariSettingsDescriptionFunctionsConstants.ExtensionMemory, AtariSettingsDescriptionFunctionsConstants.ResourceAtariMemoryMosaic,
                Value(configuration, AtariEightBitSettingsConstants.MosaicMemoryOptionKey,
                    mosaic.FirstOrDefault()?.Value ?? AtariEightBitSettingsConstants.Disabled),
                mosaic.Select(AtariHardwareSettingsFunctions.Expansion)),
            Select(AtariEightBitSettingsConstants.AxlonMemoryOptionKey, EmulationMachineTab.Ram,
                AtariSettingsDescriptionFunctionsConstants.ExtensionMemory, AtariSettingsDescriptionFunctionsConstants.ResourceAtariMemoryAxlon,
                Value(configuration, AtariEightBitSettingsConstants.AxlonMemoryOptionKey,
                    axlon.FirstOrDefault()?.Value ?? AtariEightBitSettingsConstants.Disabled),
                axlon.Select(AtariHardwareSettingsFunctions.Expansion)),
            Toggle(AtariEightBitSettingsConstants.AxlonShadowOptionKey, EmulationMachineTab.Ram,
                AtariSettingsDescriptionFunctionsConstants.ExtensionMemory, AtariSettingsDescriptionFunctionsConstants.ResourceAtariMemoryAxlonShadow,
                Enabled(configuration, AtariEightBitSettingsConstants.AxlonShadowOptionKey)),
            Toggle(AtariEightBitSettingsConstants.MapRamOptionKey, EmulationMachineTab.Ram,
                AtariSettingsDescriptionFunctionsConstants.ExtensionMemory, AtariSettingsDescriptionFunctionsConstants.ResourceAtariMemoryMapRam,
                Enabled(configuration, AtariEightBitSettingsConstants.MapRamOptionKey))));
    }

    private static EmulationSettingsBlock Audio(AtariMachineConfiguration configuration, bool isHatari) =>
        Block(AtariSettingsDescriptionFunctionsConstants.Audio, EmulationMachineTab.Audio, AtariSettingsDescriptionFunctionsConstants.ResourceAudio, AtariSettingsDescriptionFunctionsConstants.Value9, 2,
            Toggle(AtariSettingsConstants.AudioEnabled, EmulationMachineTab.Audio, AtariSettingsDescriptionFunctionsConstants.Audio,
                AtariSettingsDescriptionFunctionsConstants.ResourceAudioEnabled, configuration.AudioEnabled),
            Select(AtariVideoAudioSettingsConstants.AudioOutputOption, EmulationMachineTab.Audio, AtariSettingsDescriptionFunctionsConstants.Audio,
                AtariSettingsDescriptionFunctionsConstants.ResourceAudioOutput, Value(configuration, AtariVideoAudioSettingsConstants.AudioOutputOption,
                    AtariConfigurationOptionConstants.DefaultAudioOutput), DefaultAudioOutput()) with
            { ChoiceSource = EmulationSettingsChoiceSource.AudioOutputDevices },
            Select(AtariVideoAudioSettingsConstants.AudioLatencyOption, EmulationMachineTab.Audio, AtariSettingsDescriptionFunctionsConstants.Audio,
                AtariSettingsDescriptionFunctionsConstants.ResourceAudioLatency, Value(configuration, AtariVideoAudioSettingsConstants.AudioLatencyOption,
                    AtariConfigurationOptionConstants.DefaultAudioLatencyMilliseconds.ToString()),
                AtariVideoAudioSettingsConstants.AudioLatenciesMilliseconds.Select(Milliseconds)),
            Select(AtariVideoAudioSettingsConstants.AudioVolumeOption, EmulationMachineTab.Audio, AtariSettingsDescriptionFunctionsConstants.Audio,
                AtariSettingsDescriptionFunctionsConstants.ExplorerVolume, Value(configuration, AtariVideoAudioSettingsConstants.AudioVolumeOption,
                    AtariConfigurationOptionConstants.DefaultAudioVolumePercent.ToString()), Percentages(
                        AtariVideoAudioSettingsConstants.MinimumVolumePercent,
                        AtariVideoAudioSettingsConstants.MaximumVolumePercent,
                        AtariVideoAudioSettingsConstants.VolumeStepPercent)),
            Toggle(AtariVideoAudioSettingsConstants.FloppySoundOption, EmulationMachineTab.Audio, AtariSettingsDescriptionFunctionsConstants.Audio,
                AtariSettingsDescriptionFunctionsConstants.ResourceAudioFloppyEnabled, Value(configuration,
                    AtariVideoAudioSettingsConstants.FloppySoundOption, AtariSettingsDescriptionFunctionsConstants.True) == AtariSettingsDescriptionFunctionsConstants.True,
                AtariSettingsDescriptionFunctionsConstants.True, AtariSettingsDescriptionFunctionsConstants.False) with
            { IsVisible = isHatari },
            Select(AtariVideoAudioSettingsConstants.FloppySoundVolumeOption, EmulationMachineTab.Audio, AtariSettingsDescriptionFunctionsConstants.Audio,
                AtariSettingsDescriptionFunctionsConstants.ResourceAudioFloppySound, Value(configuration,
                    AtariVideoAudioSettingsConstants.FloppySoundVolumeOption, AtariSettingsDescriptionFunctionsConstants.Value75),
                AtariVideoAudioSettingsConstants.FloppySoundVolumesPercent.Select(Percentage))
                with
            { IsVisible = isHatari },
            Toggle(AtariVideoAudioSettingsConstants.PolarizedFilterOption, EmulationMachineTab.Audio, AtariSettingsDescriptionFunctionsConstants.Audio,
                AtariSettingsDescriptionFunctionsConstants.ResourceAudioPolarizedFilter, Value(configuration,
                    AtariVideoAudioSettingsConstants.PolarizedFilterOption, AtariSettingsDescriptionFunctionsConstants.False) == AtariSettingsDescriptionFunctionsConstants.True,
                AtariSettingsDescriptionFunctionsConstants.True, AtariSettingsDescriptionFunctionsConstants.False) with
            { IsVisible = isHatari });

    private static IReadOnlyList<EmulationSettingsChoice> StStandards(AtariStModelDefinition model)
    {
        var choices = new List<EmulationSettingsChoice>
        {
            new(AtariVideoAudioSettingsConstants.Automatic, AtariSettingsDescriptionFunctionsConstants.VisualAutomatic)
        };
        if (model.Video.Contains(AtariStVideoCapability.Pal)) choices.Add(Invariant(AtariSettingsDescriptionFunctionsConstants.PAL));
        if (model.Video.Contains(AtariStVideoCapability.Ntsc)) choices.Add(Invariant(AtariSettingsDescriptionFunctionsConstants.NTSC));
        if (model.Video.Contains(AtariStVideoCapability.Monochrome)) choices.Add(Invariant(AtariSettingsDescriptionFunctionsConstants.Monochrome));
        return choices;
    }

    private static IReadOnlyList<EmulationSettingsChoice> AutomaticAndNative() =>
    [
        new(AtariVideoAudioSettingsConstants.Automatic, AtariSettingsDescriptionFunctionsConstants.VisualAutomatic),
        Invariant(AtariVideoAudioSettingsConstants.Native)
    ];

    private static IReadOnlyList<EmulationSettingsChoice> AspectRatios() =>
    [
        new(AtariVideoAudioSettingsConstants.Automatic, AtariSettingsDescriptionFunctionsConstants.VisualAutomatic),
        Invariant(AtariVideoAudioSettingsConstants.FourByThree),
        Invariant(AtariVideoAudioSettingsConstants.PixelAspect)
    ];

    private static IReadOnlyList<EmulationSettingsChoice> FrameSkips() =>
        Enumerable.Range(AtariVideoAudioSettingsConstants.MinimumFrameSkip,
                AtariVideoAudioSettingsConstants.MaximumFrameSkip
                - AtariVideoAudioSettingsConstants.MinimumFrameSkip + 1)
            .Select(value => Invariant(value.ToString())).Append(Invariant(AtariSettingsDescriptionFunctionsConstants.Value10)).ToArray();

    private static IReadOnlyList<EmulationSettingsChoice> Resolutions(AtariMachineModel model) =>
        AtariEightBitSettingsCatalog.SupportsComputerOptions(model)
            ? AtariEightBitSettingsCatalog.OriginalComputerResolutions.Select(value =>
                new EmulationSettingsChoice(value, string.Empty,
                    value.Replace(AtariSettingsDescriptionFunctionsConstants.X, AtariSettingsDescriptionFunctionsConstants.Value11, StringComparison.Ordinal))).ToArray()
            : AutomaticAndNative();

    private static string DefaultResolution(AtariMachineModel model) =>
        AtariEightBitSettingsCatalog.SupportsComputerOptions(model)
            ? AtariEightBitSettingsCatalog.OriginalComputerResolutions[0]
            : AtariVideoAudioSettingsConstants.Automatic;

    private static IReadOnlyList<EmulationSettingsChoice> DefaultAudioOutput() =>
    [
        new(AtariConfigurationOptionConstants.DefaultAudioOutput, AtariSettingsDescriptionFunctionsConstants.ResourceAudioDefaultOutput)
    ];

    private static EmulationSettingsChoice Milliseconds(int value) =>
        new(value.ToString(), string.Empty, $"{value} ms", value);

    private static EmulationSettingsChoice Percentage(int value) =>
        new(value.ToString(), string.Empty, $"{value} %", value);

    private static IReadOnlyList<EmulationSettingsChoice> Percentages(int minimum, int maximum, int step) =>
        Enumerable.Range(0, (maximum - minimum) / step + 1)
            .Select(index => Percentage(minimum + index * step)).ToArray();

    private static EmulationSettingsChoice Invariant(string value) => new(value, string.Empty, value);


    private static EmulationSettingsBlock Block(string id, EmulationMachineTab tab, string title,
        string icon, int columns, params EmulationSettingsField[] fields) =>
        new(id, tab, title, fields, icon, columns);

    private static EmulationSettingsField Select(string id, EmulationMachineTab tab, string block,
        string label, string value, IEnumerable<string> choices, bool isEnabled = true) =>
        new(id, tab, block, label, EmulationSettingsEditor.Selection, value,
            choices.Select(choice => new EmulationSettingsChoice(choice, choice, choice)).ToArray(),
            IsEnabled: isEnabled, ExplanationResourceKey: ShortHelp(id),
            DetailedExplanationResourceKey: DetailedHelp(id));

    private static EmulationSettingsField Select(string id, EmulationMachineTab tab, string block,
        string label, string value, IEnumerable<EmulationSettingsChoice> choices, bool isEnabled = true) =>
        new(id, tab, block, label, EmulationSettingsEditor.Selection, value, choices.ToArray(),
            IsEnabled: isEnabled, ExplanationResourceKey: ShortHelp(id),
            DetailedExplanationResourceKey: DetailedHelp(id));

    private static EmulationSettingsField Toggle(string id, EmulationMachineTab tab, string block,
        string label, bool value, string enabledValue = AtariSettingsDescriptionFunctionsConstants.Enabled, string disabledValue = AtariSettingsDescriptionFunctionsConstants.Disabled) =>
        new(id, tab, block, label, EmulationSettingsEditor.Toggle,
            value ? enabledValue : disabledValue, ExplanationResourceKey: ShortHelp(id),
            DetailedExplanationResourceKey: DetailedHelp(id), EnabledValue: enabledValue, DisabledValue: disabledValue);

    private static EmulationSettingsField Path(string id, EmulationMachineTab tab, string block,
        string label, string? value) => new(id, tab, block, label, EmulationSettingsEditor.Path, value,
            ExplanationResourceKey: ShortHelp(id), DetailedExplanationResourceKey: DetailedHelp(id),
            DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware);

    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block,
        string label, string value, long? numericValue = null) => new(id, tab, block, label,
            EmulationSettingsEditor.Information, value, IsEnabled: false,
            ExplanationResourceKey: ShortHelp(id), DetailedExplanationResourceKey: DetailedHelp(id),
            NumericValue: numericValue);

    private static string? ShortHelp(string id) => FieldHelpResources.TryGetValue(id, out var resource)
        ? resource + ".Short" : null;

    private static string? DetailedHelp(string id) => FieldHelpResources.TryGetValue(id, out var resource)
        ? resource + ".Detailed" : null;

    private static string Value(AtariMachineConfiguration configuration, string key, string fallback) =>
        configuration.Options.GetValueOrDefault(key) ?? fallback;

    private static bool Enabled(AtariMachineConfiguration configuration, string key) =>
        Value(configuration, key, AtariEightBitSettingsConstants.Disabled) == AtariEightBitSettingsConstants.Enabled;

    private static EmulationSettingsField ClassicMemory(AtariMachineConfiguration configuration,
        AtariClassicModelDefinition model)
    {
        if (configuration.Model != AtariMachineModel.XlXe)
            return Information(AtariConfigurationOptionConstants.MainMemory, EmulationMachineTab.Ram,
                AtariSettingsDescriptionFunctionsConstants.MainMemory, AtariSettingsDescriptionFunctionsConstants.ResourceMemoryMain, AtariHardwareSettingsFunctions.FormatBytes(model.MainMemoryBytes),
                model.MainMemoryBytes);
        var choices = new[] { 320L, 576L, 1088L }.Select(value =>
            AtariHardwareSettingsFunctions.MemoryKib((int)value)).ToArray();
        return Select(AtariConfigurationOptionConstants.MainMemory, EmulationMachineTab.Ram, AtariSettingsDescriptionFunctionsConstants.MainMemory,
            AtariSettingsDescriptionFunctionsConstants.ResourceMemoryMain, Value(configuration, AtariConfigurationOptionConstants.MainMemory,
                model.MainMemoryBytes.ToString()), choices);
    }
}
