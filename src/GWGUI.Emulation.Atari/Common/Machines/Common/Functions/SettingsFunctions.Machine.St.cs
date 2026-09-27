using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    private static void AddMouseSettings(MachineConfiguration configuration,
        CompatibilityDefinition compatibility, ICollection<EmulationSettingsBlock> blocks)
    {
        if (!compatibility.VisibleTabs.Contains(SettingsTab.Mouse)) return;
        var choices = Enumerable.Range(MouseSettingsConstants.MinimumSpeedPercent
                / MouseSettingsConstants.SpeedStepPercent,
                (MouseSettingsConstants.MaximumSpeedPercent - MouseSettingsConstants.MinimumSpeedPercent)
                / MouseSettingsConstants.SpeedStepPercent + 1)
            .Select(value => value * MouseSettingsConstants.SpeedStepPercent)
            .Select(value => new EmulationSettingsChoice(value.ToString(), string.Empty, $"{value} %", value));
        blocks.Add(Block(SettingsDescriptionFunctionsConstants.Mouse, EmulationMachineTab.Mouse, SettingsDescriptionFunctionsConstants.ResourceTabMouse, SettingsDescriptionFunctionsConstants.Value, 1,
            Select(MouseSettingsConstants.SpeedOptionKey, EmulationMachineTab.Mouse, SettingsDescriptionFunctionsConstants.Mouse,
                SettingsDescriptionFunctionsConstants.ResourceMouseSpeed, Value(configuration, MouseSettingsConstants.SpeedOptionKey,
                    MouseSettingsConstants.DefaultSpeedPercent.ToString()), choices)));
    }

    private static void AddGeneralFolders(MachineConfiguration configuration,
        CompatibilityDefinition compatibility, ICollection<EmulationSettingsBlock> blocks)
    {
        var supportsHardDisk = EightBitSettingsCatalog.SupportsComputerOptions(configuration.Model)
            || compatibility.Media.Any(rule => rule.Availability == MediaAvailability.Available
                && rule.Category is MediaCategory.HardDisk or MediaCategory.Directory);
        if (!supportsHardDisk) return;
        blocks.Add(Block(SettingsDescriptionFunctionsConstants.DefaultFolders, EmulationMachineTab.General, SettingsDescriptionFunctionsConstants.ResourceFolderDefault, SettingsDescriptionFunctionsConstants.Value2, 1,
            new EmulationSettingsField(SettingsConstants.HardDiskFolder, EmulationMachineTab.General,
                SettingsDescriptionFunctionsConstants.DefaultFolders, SettingsDescriptionFunctionsConstants.ResourceStorageHardDiskList, EmulationSettingsEditor.DirectoryPath,
                configuration.Folders.HardDisks,
                ExplanationResourceKey: ShortHelp(SettingsConstants.HardDiskFolder),
                DetailedExplanationResourceKey: DetailedHelp(SettingsConstants.HardDiskFolder),
                DefaultFolderCategory: EmulationDefaultFolderCategory.HardDisk)));
    }

    private static IReadOnlyList<EmulationSettingsBlock> CreateSt(MachineConfiguration configuration)
    {
        var model = StModelCatalog.Get(configuration.Model);
        return
        [
            Block(SettingsDescriptionFunctionsConstants.Processor, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.ResourceCpuProcessor, SettingsDescriptionFunctionsConstants.Value3, 2,
                Select(SettingsConstants.Cpu, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Processor, SettingsDescriptionFunctionsConstants.ResourceCpuModel,
                    Value(configuration, SettingsConstants.Cpu, model.DefaultCpu.ToString()),
                    model.Cpus.Select(value => HardwareSettingsFunctions.Invariant(value.ToString(), value.ToString()))),
                Select(SettingsConstants.CpuPrecision, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Processor, SettingsDescriptionFunctionsConstants.ResourceCpuPrecision,
                    Value(configuration, SettingsConstants.CpuPrecision, model.DefaultCpuPrecision.ToString()),
                    model.CpuPrecisions.Select(HardwareSettingsFunctions.CpuPrecision)),
                Select(SettingsConstants.Fpu, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Processor, SettingsDescriptionFunctionsConstants.ResourceFpuModel,
                    Value(configuration, SettingsConstants.Fpu, model.DefaultFpu.ToString()),
                    model.Fpus.Select(HardwareSettingsFunctions.Fpu)),
                Information(SettingsConstants.CpuOriginalFrequency, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Processor,
                    SettingsDescriptionFunctionsConstants.ResourceCpuSpeedOriginal,
                    HardwareSettingsFunctions.FrequencyMhz(model.DefaultCpuFrequencyMhz).InvariantDisplayValue!),
                Select(SettingsConstants.CpuFrequency, EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Processor, SettingsDescriptionFunctionsConstants.ResourceCpuSpeed,
                    Value(configuration, SettingsConstants.CpuFrequency, model.DefaultCpuFrequencyMhz.ToString()),
                    model.CpuFrequenciesMhz.Select(HardwareSettingsFunctions.FrequencyMhz))),
            Block(SettingsDescriptionFunctionsConstants.MainMemory, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.ResourceMemoryMain, SettingsDescriptionFunctionsConstants.Value4, 2,
                Select(SettingsConstants.MainMemory, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.MainMemory, SettingsDescriptionFunctionsConstants.ResourceMemoryMain,
                    Value(configuration, SettingsConstants.MainMemory,
                        ((long)model.DefaultMainMemoryKib * HardwareSettingsConstants.BytesPerKibibyte).ToString()),
                    model.MainMemoryKib.Select(HardwareSettingsFunctions.MemoryKib))),
            Block(SettingsDescriptionFunctionsConstants.ExtensionMemory, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.ResourceMemoryExtensions, SettingsDescriptionFunctionsConstants.Value4, 1,
                Select(SettingsConstants.AlternateMemory, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.ExtensionMemory, SettingsDescriptionFunctionsConstants.ResourceMemoryExtensions,
                    Value(configuration, SettingsConstants.AlternateMemory,
                        ((long)model.DefaultAlternateMemoryMib * HardwareSettingsConstants.BytesPerMebibyte).ToString()),
                    model.AlternateMemoryMib.Select(HardwareSettingsFunctions.MemoryMib))),
            Block(SettingsDescriptionFunctionsConstants.Firmware, EmulationMachineTab.Rom, SettingsDescriptionFunctionsConstants.ResourceFirmwareRomSystem, SettingsDescriptionFunctionsConstants.Value5, 1,
                Path(SettingsConstants.SystemFirmware, EmulationMachineTab.Rom, SettingsDescriptionFunctionsConstants.Firmware,
                    SettingsDescriptionFunctionsConstants.ResourceFirmwareRomSystem,
                    configuration.Firmwares.FirstOrDefault(item => item.Category == FirmwareCategory.Tos)?.Path),
                Toggle(SettingsDescriptionFunctionsConstants.FastBoot, EmulationMachineTab.Rom, SettingsDescriptionFunctionsConstants.Firmware, SettingsDescriptionFunctionsConstants.ResourceAtariFastBoot,
                    Value(configuration, SettingsDescriptionFunctionsConstants.FastBoot, SettingsDescriptionFunctionsConstants.False) == SettingsDescriptionFunctionsConstants.True, SettingsDescriptionFunctionsConstants.True, SettingsDescriptionFunctionsConstants.False)),
            Block(SettingsDescriptionFunctionsConstants.StorageOptions, EmulationMachineTab.Storage, SettingsDescriptionFunctionsConstants.ResourceStorageDeviceList, SettingsDescriptionFunctionsConstants.Value6, 1,
                Toggle(MachineOptionConstants.DriveActivity, EmulationMachineTab.Storage,
                    SettingsDescriptionFunctionsConstants.StorageOptions, SettingsDescriptionFunctionsConstants.ResourceStorageActivityOsd,
                    Value(configuration, MachineOptionConstants.DriveActivity, SettingsDescriptionFunctionsConstants.False) == SettingsDescriptionFunctionsConstants.True,
                    SettingsDescriptionFunctionsConstants.True, SettingsDescriptionFunctionsConstants.False)),
            Block(SettingsDescriptionFunctionsConstants.Video, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.ResourceVideoSettingsDisplay, SettingsDescriptionFunctionsConstants.Value7, 2,
                Select(VideoAudioSettingsConstants.StandardOption, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Video,
                    SettingsDescriptionFunctionsConstants.ResourceVideoStandard, Value(configuration, VideoAudioSettingsConstants.StandardOption,
                        VideoAudioSettingsConstants.Automatic), StStandards(model)),
                Select(SettingsConstants.Region, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Video, SettingsDescriptionFunctionsConstants.ResourceAtariVideoRegion,
                    Value(configuration, SettingsConstants.Region, model.DefaultRegion.ToString()),
                    model.Regions.Select(HardwareSettingsFunctions.StRegionChoice)),
                Select(VideoAudioSettingsConstants.ResolutionOption, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Video,
                    SettingsDescriptionFunctionsConstants.ResourceVideoResolution, Value(configuration, VideoAudioSettingsConstants.ResolutionOption,
                        VideoAudioSettingsConstants.Automatic), AutomaticAndNative()),
                Select(VideoAudioSettingsConstants.AspectRatioOption, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Video,
                    SettingsDescriptionFunctionsConstants.ResourceVideoAspectRatio, Value(configuration, VideoAudioSettingsConstants.AspectRatioOption,
                        VideoAudioSettingsConstants.Automatic), AspectRatios()),
                Toggle(VideoAudioSettingsConstants.CropOption, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Video,
                    SettingsDescriptionFunctionsConstants.ResourceVideoCrop, Value(configuration, VideoAudioSettingsConstants.CropOption,
                        VideoAudioSettingsConstants.Disabled) == VideoAudioSettingsConstants.Enabled),
                Select(VideoAudioSettingsConstants.FrameSkipOption, EmulationMachineTab.Video, SettingsDescriptionFunctionsConstants.Video,
                    SettingsDescriptionFunctionsConstants.ResourceVideoFrameSkip, Value(configuration, VideoAudioSettingsConstants.FrameSkipOption,
                        VideoAudioSettingsConstants.MinimumFrameSkip.ToString()), FrameSkips())),
            Audio(configuration, true)
        ];
    }

}
