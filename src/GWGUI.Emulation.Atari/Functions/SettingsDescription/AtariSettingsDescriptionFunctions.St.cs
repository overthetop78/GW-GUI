using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Functions;

internal static partial class AtariSettingsDescriptionFunctions
{
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

}
