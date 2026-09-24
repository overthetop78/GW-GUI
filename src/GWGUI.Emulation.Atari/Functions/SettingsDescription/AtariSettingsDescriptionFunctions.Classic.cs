using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Functions;

internal static partial class AtariSettingsDescriptionFunctions
{
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
        if (model.Firmware.Count > 0)
        {
            var firmwareFields = ClassicFirmwareFields(configuration);
            blocks.Add(Block(AtariSettingsDescriptionFunctionsConstants.Firmware, EmulationMachineTab.Rom,
                AtariSettingsDescriptionFunctionsConstants.ResourceFirmwareRomSystem,
                AtariSettingsDescriptionFunctionsConstants.Value5, firmwareFields.Length > 1 ? 2 : 1,
                firmwareFields));
        }
        AddEightBitMemory(configuration, blocks);
        AddEightBitOptions(configuration, blocks);
        AddEightBitControllerOptions(configuration, blocks);
        return blocks;
    }

    private static EmulationSettingsField[] ClassicFirmwareFields(AtariMachineConfiguration configuration)
    {
        var categories = AtariClassicModelCatalog.Get(configuration.Model).Firmware;
        var fields = new List<EmulationSettingsField>();
        if (categories.Any(category => AtariFirmwareSelectionFunctions.IsSystemRom(configuration.Model, category)))
            fields.Add(FirmwarePath(configuration, AtariSettingsConstants.SystemFirmware,
                AtariSettingsDescriptionFunctionsConstants.ResourceFirmwareRomSystem));
        if (configuration.Model is AtariMachineModel.Atari800Xl or AtariMachineModel.Atari130Xe
            or AtariMachineModel.XlXe or AtariMachineModel.Xegs)
            fields.Add(FirmwarePath(configuration, AtariSettingsConstants.BasicFirmware,
                AtariSettingsDescriptionFunctionsConstants.ResourceFirmwareRomBasic));
        if (categories.Contains(AtariFirmwareCategory.AtariXegsBios))
            fields.Add(FirmwarePath(configuration, AtariSettingsConstants.XegsFirmware,
                AtariSettingsDescriptionFunctionsConstants.ResourceFirmwareRomXegs));
        return fields.ToArray();
    }

    private static EmulationSettingsField FirmwarePath(AtariMachineConfiguration configuration,
        string fieldId, string label) =>
        Path(fieldId, EmulationMachineTab.Rom, AtariSettingsDescriptionFunctionsConstants.Firmware, label,
            configuration.Firmwares.FirstOrDefault(item => string.Equals(
                AtariFirmwareSelectionFunctions.FieldId(configuration.Model, item.Category), fieldId,
                StringComparison.Ordinal))?.Path);

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

}
