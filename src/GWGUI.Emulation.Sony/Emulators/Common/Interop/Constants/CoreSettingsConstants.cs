namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

internal static class CoreSettingsConstants
{
    internal const string OptionsBlock = "emulatorOptions";
    internal const string AccelerationBlock = "acceleration";
    internal const string AccelerationTitle = "Emulation.Cpu.Acceleration";
    internal const string CompatibilityGroup = "Compatibility";
    internal const string CompatibilityTitle = "Emulation.Option.Sony.Group.Compatibility";
    internal const string TimingGroup = "Timing";
    internal const string TimingTitle = "Emulation.Option.Sony.Group.Timing";
    internal const string CpuScaleLabel = "Emulation.Option.Sony.Cpu.ClockScale";
    internal const string CompatibilityKeyword = "compatib";
    internal const string FrameKeyword = "frame";
    internal const string TimingKeyword = "timing";
    internal const string DynarecKeyword = "dynarec";
    internal static bool IsCpuClock(string key) =>
        key == Emulators.BeetlePsx.Constants.OptionConstants.CpuFreqScaleKey
        || key == Emulators.BeetlePsxHw.Constants.OptionConstants.CpuFreqScaleKey
        || key == Emulators.SwanStation.Constants.OptionConstants.CPUOverclockKey
        || key == Emulators.PcsxRearmed.Constants.OptionConstants.PsxclockKey;
    internal static bool IsCdAudio(string key) => key == Emulators.SwanStation.Constants.OptionConstants.CDROMMuteCDAudioKey;
    internal const string OptionResourcePrefix = "Emulation.Option.";
    internal const string NumericFormat = "0.########";
    internal const string StorageTitleResource = "Emulation.Tab.Storage";
    internal const string MouseTitleResource = "Emulation.Tab.Mouse";
    internal const string ControllersTitleResource = "Emulation.Tab.Controller";
    internal const string StorageIcon = "\uEDA2";
    internal const string MouseIcon = "\uE962";
    internal const string ControllersIcon = "\uE7FC";
    internal const string GeometryGroup = "PGXP";
    internal const string TextureGroup = "Texture";
    internal const string RenderingTitle = "Emulation.Video.Settings.Rendering";
    internal const string GeometryTitle = "Emulation.Option.Sony.Group.Geometry";
    internal const string TexturesTitle = "Emulation.Option.Sony.Group.Textures";
    internal const string AudioOptionsBlock = SettingsDescriptionFunctionsConstants.EmulatorAudioOptionsBlock;
    internal const string AudioOptionsTitle = SettingsDescriptionFunctionsConstants.ResourceEmulator;
    internal const string DisplayGroup = "Display";
    internal const string ScreenGroup = "Screen";
    internal static bool IsBasicVideo(string key) => key is
        Emulators.SwanStation.Constants.OptionConstants.GPURendererKey
        or Emulators.SwanStation.Constants.OptionConstants.GPUResolutionScaleKey
        or Emulators.BeetlePsx.Constants.OptionConstants.InternalResolutionKey
        or Emulators.BeetlePsxHw.Constants.OptionConstants.InternalResolutionKey
        or Emulators.BeetlePsxHw.Constants.OptionConstants.RendererKey
        or Emulators.Pcee2.Constants.OptionConstants.RendererKey
        or Emulators.Pcsx2.Constants.OptionConstants.UpscaleMultiplierKey
        or Emulators.Ppsspp.Constants.OptionConstants.BackendKey
        or Emulators.Ppsspp.Constants.OptionConstants.InternalResolutionKey
        or Emulators.Rpcs3.Constants.OptionConstants.RendererKey
        or Emulators.Rpcs3.Constants.OptionConstants.ResolutionScaleKey;
    internal static bool IsScreenOption(string key) => new[] { "crop", "overscan", "aspect", "widescreen", "offset", "scanline", "interlac" }.Any(part => key.Contains(part, StringComparison.OrdinalIgnoreCase));
    internal const long DevelopmentRamExtensionBytes = 6L * 1024 * 1024;
    internal static IReadOnlySet<string> EnabledLabels { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        OptionValueResourceConstants.EnabledLabelResource,
        OptionValueResourceConstants.EnabledRecommendedLabelResource,
    };
    internal static IReadOnlySet<string> DisabledLabels { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        OptionValueResourceConstants.DisabledLabelResource,
        OptionValueResourceConstants.DisabledAllowOverridesLabelResource,
    };
    internal static IReadOnlySet<string> HiddenOptions { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        Emulators.SwanStation.Constants.OptionConstants.GPUTextureFilterKey,
        Emulators.BeetlePsxHw.Constants.OptionConstants.FilterKey,
        Emulators.BeetlePsxHw.Constants.OptionConstants.FilterExcludeSpriteKey,
        Emulators.BeetlePsxHw.Constants.OptionConstants.FilterExclude2dPolygonKey,
        Emulators.SwanStation.Constants.OptionConstants.LoggingLogLevelKey,
        Emulators.SwanStation.Constants.OptionConstants.DisplayShowOSDMessagesKey,
        Emulators.BeetlePsx.Constants.OptionConstants.DisplayInternalFpsKey,
        Emulators.BeetlePsx.Constants.OptionConstants.DisplayOsdKey,
        Emulators.BeetlePsxHw.Constants.OptionConstants.DisplayInternalFpsKey,
        Emulators.BeetlePsxHw.Constants.OptionConstants.DisplayOsdKey,
        Emulators.Rpcs3.Constants.OptionConstants.SilenceAllLogsKey,
        Emulators.PcsxRearmed.Constants.OptionConstants.ShowInputSettingsKey,
        Emulators.PcsxRearmed.Constants.OptionConstants.DisplayFpsV2Key,
        Emulators.PcsxRearmed.Constants.OptionConstants.DisplayInfoKey,
        Emulators.PcsxRearmed.Constants.OptionConstants.BiosKey,
        Emulators.BeetlePsx.Constants.OptionConstants.OverrideBiosKey,
        Emulators.BeetlePsxHw.Constants.OptionConstants.OverrideBiosKey,
    };
    internal static IReadOnlyDictionary<EmulationMachineTab, (string Title, string Icon)> Sections { get; } =
        new Dictionary<EmulationMachineTab, (string, string)>
        {
            [EmulationMachineTab.General] = (SettingsDescriptionFunctionsConstants.ResourceGeneral, SettingsDescriptionFunctionsConstants.IconGeneral),
            [EmulationMachineTab.Cpu] = (SettingsDescriptionFunctionsConstants.ResourceCpuProcessor, SettingsDescriptionFunctionsConstants.IconCpu),
            [EmulationMachineTab.Ram] = (SettingsDescriptionFunctionsConstants.ResourceMemoryMain, SettingsDescriptionFunctionsConstants.IconMemory),
            [EmulationMachineTab.Rom] = (SettingsDescriptionFunctionsConstants.ResourceRom, SettingsDescriptionFunctionsConstants.IconFirmware),
            [EmulationMachineTab.Video] = (SettingsDescriptionFunctionsConstants.ResourceVideo, SettingsDescriptionFunctionsConstants.IconVideo),
            [EmulationMachineTab.Audio] = (SettingsDescriptionFunctionsConstants.ResourceAudio, SettingsDescriptionFunctionsConstants.IconAudio),
            [EmulationMachineTab.Storage] = (StorageTitleResource, StorageIcon),
            [EmulationMachineTab.Mouse] = (MouseTitleResource, MouseIcon),
            [EmulationMachineTab.Controllers] = (ControllersTitleResource, ControllersIcon),
            [EmulationMachineTab.Cheats] = ("Emulation.Tab.Cheats", SettingsDescriptionFunctionsConstants.IconGeneral),
            [EmulationMachineTab.Ethernet] = ("Emulation.Tab.Ethernet", SettingsDescriptionFunctionsConstants.IconGeneral)
        };
}
