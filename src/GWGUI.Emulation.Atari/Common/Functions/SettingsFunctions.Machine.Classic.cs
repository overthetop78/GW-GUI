using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    private static IReadOnlyList<EmulationSettingsBlock> CreateClassic(MachineConfiguration configuration)
    {
        var model = ClassicModelCatalog.Get(configuration.Model);
        var blocks = new List<EmulationSettingsBlock>
        {
            Block(SettingsDescriptionFunctionsConstants.Processor, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.ResourceCpuProcessor, SettingsDescriptionFunctionsConstants.Value3, 2,
                Select(SettingsConstants.Cpu, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Processor, SettingsDescriptionFunctionsConstants.ResourceCpuModel,
                    Value(configuration, SettingsConstants.Cpu, model.DefaultCpu.ToString()), model.Cpus.Select(value => value.ToString()),
                    isEnabled: model.Cpus.Count > 1),
                Information(SettingsConstants.CpuOriginalFrequency, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Processor, SettingsDescriptionFunctionsConstants.ResourceCpuSpeedOriginal,
                    $"{model.DefaultCpuFrequencyHz / 1_000_000d:0.00} MHz")),
            Block(SettingsDescriptionFunctionsConstants.MainMemory, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.ResourceMemoryMain, SettingsDescriptionFunctionsConstants.Value4, 1,
                ClassicMemory(configuration, model)),
            Block(SettingsDescriptionFunctionsConstants.Video, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.ResourceVideoSettingsDisplay, SettingsDescriptionFunctionsConstants.Value7, 2,
                Select(ConfigurationOptionConstants.VideoStandard, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Video,
                    SettingsDescriptionFunctionsConstants.ResourceVideoStandard, Value(configuration, ConfigurationOptionConstants.VideoStandard,
                        model.DefaultRegion.ToString()), model.Regions.Select(HardwareSettingsFunctions.ClassicRegionChoice),
                    isEnabled: model.Regions.Count > 1),
                Select(VideoAudioSettingsConstants.ResolutionOption, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Video,
                    SettingsDescriptionFunctionsConstants.ResourceVideoResolution, Value(configuration, VideoAudioSettingsConstants.ResolutionOption,
                        DefaultResolution(configuration.Model)), Resolutions(configuration.Model))),
            Audio(configuration, false)
        };
        if (model.Firmware.Count > 0)
        {
            var firmwareFields = ClassicFirmwareFields(configuration);
            blocks.Add(Block(SettingsDescriptionFunctionsConstants.Firmware, EmulationMachineTab.Rom,
                SettingsDescriptionFunctionsConstants.ResourceFirmwareRomSystem,
                SettingsDescriptionFunctionsConstants.Value5, firmwareFields.Length > 1 ? 2 : 1,
                firmwareFields));
        }
        AddEightBitMemory(configuration, blocks);
        AddEightBitOptions(configuration, blocks);
        AddEightBitControllerOptions(configuration, blocks);
        return blocks;
    }

    private static EmulationSettingsField[] ClassicFirmwareFields(MachineConfiguration configuration)
    {
        var categories = ClassicModelCatalog.Get(configuration.Model).Firmware;
        var fields = new List<EmulationSettingsField>();
        if (categories.Any(category => FirmwareSelectionFunctions.IsSystemRom(configuration.Model, category)))
            fields.Add(FirmwarePath(configuration, SettingsConstants.SystemFirmware,
                SettingsDescriptionFunctionsConstants.ResourceFirmwareRomSystem));
        if (configuration.Model is MachineModel.Atari800Xl or MachineModel.Atari130Xe
            or MachineModel.XlXe or MachineModel.Xegs)
            fields.Add(FirmwarePath(configuration, SettingsConstants.BasicFirmware,
                SettingsDescriptionFunctionsConstants.ResourceFirmwareRomBasic));
        if (categories.Contains(FirmwareCategory.AtariXegsBios))
            fields.Add(FirmwarePath(configuration, SettingsConstants.XegsFirmware,
                SettingsDescriptionFunctionsConstants.ResourceFirmwareRomXegs));
        return fields.ToArray();
    }

    private static EmulationSettingsField FirmwarePath(MachineConfiguration configuration,
        string fieldId, string label) =>
        Path(fieldId, EmulationMachineTab.Rom, SettingsDescriptionFunctionsConstants.Firmware, label,
            configuration.Firmwares.FirstOrDefault(item => string.Equals(
                FirmwareSelectionFunctions.FieldId(configuration.Model, item.Category), fieldId,
                StringComparison.Ordinal))?.Path);

    private static void AddEightBitControllerOptions(MachineConfiguration configuration,
        ICollection<EmulationSettingsBlock> blocks)
    {
        if (!EightBitSettingsCatalog.SupportsComputerOptions(configuration.Model)) return;
        blocks.Add(Block(SettingsDescriptionFunctionsConstants.ControllerOptions, EmulationMachineTab.Controllers,
            SettingsDescriptionFunctionsConstants.ResourceControllerTab, SettingsDescriptionFunctionsConstants.Value8, 2,
            Select(EightBitSettingsConstants.PaddleMovementSpeedOptionKey,
                EmulationMachineTab.Controllers, SettingsDescriptionFunctionsConstants.ControllerOptions, SettingsDescriptionFunctionsConstants.ResourceAtariControllerPaddleSpeed,
                Value(configuration, EightBitSettingsConstants.PaddleMovementSpeedOptionKey,
                    EightBitSettingsConstants.DefaultPaddleMovementSpeed),
                EightBitSettingsCatalog.PaddleMovementSpeeds),
            Select(EightBitSettingsConstants.AutofireOptionKey,
                EmulationMachineTab.Controllers, SettingsDescriptionFunctionsConstants.ControllerOptions, SettingsDescriptionFunctionsConstants.ResourceAtariControllerAutofire,
                Value(configuration, EightBitSettingsConstants.AutofireOptionKey,
                    EightBitSettingsConstants.Disabled), EightBitSettingsCatalog.AutofireModes),
            Select(EightBitSettingsConstants.ControllerCompatibilityOptionKey,
                EmulationMachineTab.Controllers, SettingsDescriptionFunctionsConstants.ControllerOptions, SettingsDescriptionFunctionsConstants.ResourceAtariControllerCompatibility,
                Value(configuration, EightBitSettingsConstants.ControllerCompatibilityOptionKey,
                    EightBitSettingsConstants.None), EightBitSettingsCatalog.ControllerCompatibilityModes),
            Select(EightBitSettingsConstants.DigitalSensitivityOptionKey,
                EmulationMachineTab.Controllers, SettingsDescriptionFunctionsConstants.ControllerOptions, SettingsDescriptionFunctionsConstants.ResourceAtariControllerDigitalSensitivity,
                Value(configuration, EightBitSettingsConstants.DigitalSensitivityOptionKey,
                    EightBitSettingsConstants.DefaultSensitivity), EightBitSettingsCatalog.Sensitivities),
            Select(EightBitSettingsConstants.AnalogSensitivityOptionKey,
                EmulationMachineTab.Controllers, SettingsDescriptionFunctionsConstants.ControllerOptions, SettingsDescriptionFunctionsConstants.ResourceAtariControllerAnalogSensitivity,
                Value(configuration, EightBitSettingsConstants.AnalogSensitivityOptionKey,
                    EightBitSettingsConstants.DefaultSensitivity), EightBitSettingsCatalog.Sensitivities)));
    }

    private static void AddEightBitOptions(MachineConfiguration configuration,
        ICollection<EmulationSettingsBlock> blocks)
    {
        if (!EightBitSettingsCatalog.SupportsComputerOptions(configuration.Model)) return;
        blocks.Add(Block(SettingsDescriptionFunctionsConstants.VideoColors, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.ResourceVideoSettingsDisplay, SettingsDescriptionFunctionsConstants.Value7, 2,
            Select(EightBitSettingsConstants.ArtifactingModeOptionKey, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.VideoColors, SettingsDescriptionFunctionsConstants.ResourceAtariVideoArtifacting,
                Value(configuration, EightBitSettingsConstants.ArtifactingModeOptionKey,
                    EightBitSettingsConstants.None), EightBitSettingsCatalog.ArtifactingModes),
            Select(EightBitSettingsConstants.ColorHueOptionKey, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.VideoColors,
                SettingsDescriptionFunctionsConstants.ResourceAtariVideoHue, Value(configuration, EightBitSettingsConstants.ColorHueOptionKey,
                    EightBitSettingsConstants.DefaultColorAdjustment), EightBitSettingsCatalog.ColorAdjustments),
            Select(EightBitSettingsConstants.ColorSaturationOptionKey, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.VideoColors, SettingsDescriptionFunctionsConstants.ResourceAtariVideoSaturation,
                Value(configuration, EightBitSettingsConstants.ColorSaturationOptionKey,
                    EightBitSettingsConstants.DefaultColorAdjustment), EightBitSettingsCatalog.ColorAdjustments),
            Select(EightBitSettingsConstants.ColorContrastOptionKey, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.VideoColors, SettingsDescriptionFunctionsConstants.ResourceAtariVideoContrast,
                Value(configuration, EightBitSettingsConstants.ColorContrastOptionKey,
                    EightBitSettingsConstants.DefaultColorAdjustment), EightBitSettingsCatalog.ContrastAndBrightness),
            Select(EightBitSettingsConstants.ColorBrightnessOptionKey, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.VideoColors, SettingsDescriptionFunctionsConstants.ResourceAtariVideoBrightness,
                Value(configuration, EightBitSettingsConstants.ColorBrightnessOptionKey,
                    EightBitSettingsConstants.DefaultColorAdjustment), EightBitSettingsCatalog.ContrastAndBrightness),
            Select(EightBitSettingsConstants.ColorGammaOptionKey, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.VideoColors, SettingsDescriptionFunctionsConstants.ResourceVideoGamma,
                Value(configuration, EightBitSettingsConstants.ColorGammaOptionKey,
                    EightBitSettingsConstants.DefaultGamma), EightBitSettingsCatalog.GammaValues),
            Select(EightBitSettingsConstants.ColorDelayOptionKey, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.VideoColors, SettingsDescriptionFunctionsConstants.ResourceAtariVideoColorDelay,
                Value(configuration, EightBitSettingsConstants.ColorDelayOptionKey,
                    EightBitSettingsConstants.DefaultColorDelay), EightBitSettingsCatalog.ColorDelayValues),
            Select(EightBitSettingsConstants.ExternalPaletteOptionKey, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.VideoColors, SettingsDescriptionFunctionsConstants.ResourceAtariVideoExternalPalette,
                Value(configuration, EightBitSettingsConstants.ExternalPaletteOptionKey,
                    EightBitSettingsConstants.None), EightBitSettingsCatalog.ExternalPalettes)));
        blocks.Add(Block(SettingsDescriptionFunctionsConstants.Pokey, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.ResourceAudio, SettingsDescriptionFunctionsConstants.Value9, 1,
            Toggle(EightBitSettingsConstants.PokeyStereoOptionKey, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Pokey,
                SettingsDescriptionFunctionsConstants.ResourceAtariAudioPokeyStereo, Enabled(configuration,
                    EightBitSettingsConstants.PokeyStereoOptionKey))));
        blocks.Add(Block(SettingsDescriptionFunctionsConstants.StorageOptions, EmulationMachineTab.Storage, SettingsDescriptionFunctionsConstants.ResourceStorageDeviceList, SettingsDescriptionFunctionsConstants.Value6, 2,
            Toggle(EightBitSettingsConstants.ShowActivityOptionKey, EmulationMachineTab.Storage,
                SettingsDescriptionFunctionsConstants.StorageOptions, SettingsDescriptionFunctionsConstants.ResourceStorageActivityOsd, Enabled(configuration,
                    EightBitSettingsConstants.ShowActivityOptionKey)),
            Toggle(EightBitSettingsConstants.ShowSpeedOptionKey, EmulationMachineTab.Storage,
                SettingsDescriptionFunctionsConstants.StorageOptions, SettingsDescriptionFunctionsConstants.ResourceAtariStorageSpeedOsd, Enabled(configuration,
                    EightBitSettingsConstants.ShowSpeedOptionKey)),
            Toggle(EightBitSettingsConstants.ShowSectorOptionKey, EmulationMachineTab.Storage,
                SettingsDescriptionFunctionsConstants.StorageOptions, SettingsDescriptionFunctionsConstants.ResourceAtariStorageSectorOsd, Enabled(configuration,
                    EightBitSettingsConstants.ShowSectorOptionKey)),
            Toggle(EightBitSettingsConstants.CassetteBootOptionKey, EmulationMachineTab.Storage,
                SettingsDescriptionFunctionsConstants.StorageOptions, SettingsDescriptionFunctionsConstants.ResourceAtariStorageCassetteBoot, Enabled(configuration,
                    EightBitSettingsConstants.CassetteBootOptionKey)),
            Toggle(EightBitSettingsConstants.RealTimeClockOptionKey, EmulationMachineTab.Storage,
                SettingsDescriptionFunctionsConstants.StorageOptions, SettingsDescriptionFunctionsConstants.ResourceAtariStorageRealTimeClock, Enabled(configuration,
                    EightBitSettingsConstants.RealTimeClockOptionKey)),
            Toggle(EightBitSettingsConstants.PrinterDeviceOptionKey, EmulationMachineTab.Storage,
                SettingsDescriptionFunctionsConstants.StorageOptions, SettingsDescriptionFunctionsConstants.ResourceAtariStoragePrinterDevice, Enabled(configuration,
                    EightBitSettingsConstants.PrinterDeviceOptionKey)),
            Toggle(EightBitSettingsConstants.SerialDeviceOptionKey, EmulationMachineTab.Storage,
                SettingsDescriptionFunctionsConstants.StorageOptions, SettingsDescriptionFunctionsConstants.ResourceAtariStorageSerialDevice, Enabled(configuration,
                    EightBitSettingsConstants.SerialDeviceOptionKey))));
    }

    private static void AddEightBitMemory(MachineConfiguration configuration,
        ICollection<EmulationSettingsBlock> blocks)
    {
        var mosaic = EightBitSettingsCatalog.Mosaic(configuration.Model);
        var axlon = EightBitSettingsCatalog.Axlon(configuration.Model);
        if (mosaic.Count == 0 && axlon.Count == 0) return;
        blocks.Add(Block(SettingsDescriptionFunctionsConstants.ExtensionMemory, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.ResourceMemoryExtensions, SettingsDescriptionFunctionsConstants.Value4, 2,
            Select(EightBitSettingsConstants.MosaicMemoryOptionKey, EmulationMachineTab.Ram,
                SettingsDescriptionFunctionsConstants.ExtensionMemory, SettingsDescriptionFunctionsConstants.ResourceAtariMemoryMosaic,
                Value(configuration, EightBitSettingsConstants.MosaicMemoryOptionKey,
                    mosaic.FirstOrDefault()?.Value ?? EightBitSettingsConstants.Disabled),
                mosaic.Select(HardwareSettingsFunctions.Expansion)),
            Select(EightBitSettingsConstants.AxlonMemoryOptionKey, EmulationMachineTab.Ram,
                SettingsDescriptionFunctionsConstants.ExtensionMemory, SettingsDescriptionFunctionsConstants.ResourceAtariMemoryAxlon,
                Value(configuration, EightBitSettingsConstants.AxlonMemoryOptionKey,
                    axlon.FirstOrDefault()?.Value ?? EightBitSettingsConstants.Disabled),
                axlon.Select(HardwareSettingsFunctions.Expansion)),
            Toggle(EightBitSettingsConstants.AxlonShadowOptionKey, EmulationMachineTab.Ram,
                SettingsDescriptionFunctionsConstants.ExtensionMemory, SettingsDescriptionFunctionsConstants.ResourceAtariMemoryAxlonShadow,
                Enabled(configuration, EightBitSettingsConstants.AxlonShadowOptionKey)),
            Toggle(EightBitSettingsConstants.MapRamOptionKey, EmulationMachineTab.Ram,
                SettingsDescriptionFunctionsConstants.ExtensionMemory, SettingsDescriptionFunctionsConstants.ResourceAtariMemoryMapRam,
                Enabled(configuration, EightBitSettingsConstants.MapRamOptionKey))));
    }

}
