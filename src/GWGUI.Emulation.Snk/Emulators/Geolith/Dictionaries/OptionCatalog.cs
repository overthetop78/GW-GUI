using GWGUI.Emulation.Snk.Emulators.Geolith.Constants;

namespace GWGUI.Emulation.Snk.Emulators.Geolith.Dictionaries;

internal static class OptionCatalog
{
    internal static IReadOnlyList<CoreOption> Options { get; } = Array.AsReadOnly(new CoreOption[] { new(
        RuntimeOptionConstants.SystemType,
        "System Type",
        "Specify the System Type: AES, MVS, or Universe BIOS System",
        EmulationMachineTab.General.ToString(),
        "aes",
        "aes",
        [
        new("aes", "Neo Geo AES (Home Console)", ResourceKey: SettingsChoiceConstants.ValueNeoGeoAESHomeConsole),
        new("mvs", "Neo Geo MVS (Arcade)", ResourceKey: SettingsChoiceConstants.ValueNeoGeoMVSArcade),
        new("uni", "Universe BIOS (Community-enhanced BIOS)", ResourceKey: SettingsChoiceConstants.ValueUniverseBIOSCommunityEnhancedBIOS)
    ],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingSystemType,
        ExplanationResourceKey: SettingsTextConstants.HelpSystemType
    ), new(
        RuntimeOptionConstants.UnibiosHw,
        "Universe BIOS Hardware",
        "Specify the hardware the Universe BIOS should detect",
        EmulationMachineTab.General.ToString(),
        "mvs",
        "mvs",
        [new("aes", "Neo Geo AES (Home Console)"), new("mvs", "Neo Geo MVS (Arcade)")],
        IsVisible: false
    ), new(
        RuntimeOptionConstants.CdSystemType,
        "CD System Type",
        "Specify the CD System Type when loading Neo Geo CD disc images",
        EmulationMachineTab.General.ToString(),
        "cdz",
        "cdz",
        [
        new("cd_front", "Neo Geo CD (Front Loader)", ResourceKey: SettingsChoiceConstants.ValueNeoGeoCDFrontLoader),
        new("cd_top", "Neo Geo CD (Top Loader)", ResourceKey: SettingsChoiceConstants.ValueNeoGeoCDTopLoader),
        new("cdz", "Neo Geo CDZ", ResourceKey: SettingsChoiceConstants.ValueNeoGeoCDZ),
        new("cdz_unibios", "Neo Geo CDZ (Universe BIOS)", ResourceKey: SettingsChoiceConstants.ValueNeoGeoCDZUniverseBIOS)
    ],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingCDSystemType,
        ExplanationResourceKey: SettingsTextConstants.HelpCDSystemType
    ), new(
        RuntimeOptionConstants.Region,
        "Region",
        "Specify the Region: USA, Japan, Asia, Europe",
        EmulationMachineTab.General.ToString(),
        "us",
        "us",
        [
        new("us", "USA", ResourceKey: SettingsChoiceConstants.ValueUSA),
        new("jp", "Japan", ResourceKey: SettingsChoiceConstants.ValueJapan),
        new("as", "Asia", ResourceKey: SettingsChoiceConstants.ValueAsia),
        new("eu", "Europe", ResourceKey: SettingsChoiceConstants.ValueEurope)
    ],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingRegion,
        ExplanationResourceKey: SettingsTextConstants.HelpRegion
    ), new(
        RuntimeOptionConstants.Memcard,
        "Memory Card",
        "Enable or Disable the Memory Card",
        EmulationMachineTab.Storage.ToString(),
        "on",
        "on",
        [new("off", "Off"), new("on", "On")],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingMemoryCard,
        ExplanationResourceKey: SettingsTextConstants.HelpMemoryCard
    ), new(
        RuntimeOptionConstants.MemcardWp,
        "Memory Card Write Protect",
        "Enable or Disable the Memory Card Write Protect pin",
        EmulationMachineTab.Storage.ToString(),
        "off",
        "off",
        [new("off", "Off"), new("on", "On")],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingMemoryCardWriteProtect,
        ExplanationResourceKey: SettingsTextConstants.HelpMemoryCardWriteProtect
    ), new(
        RuntimeOptionConstants.Settingmode,
        "Setting Mode",
        "Bring up the System ROM menu at boot on arcade systems",
        EmulationMachineTab.General.ToString(),
        "off",
        "off",
        [new("off", "Off"), new("on", "On")],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingSettingMode,
        ExplanationResourceKey: SettingsTextConstants.HelpSettingMode
    ), new(
        RuntimeOptionConstants.FourPlayer,
        "Four Player Mode",
        "Set Four Player (dual MVS cabinet) mode for Asia/Japan MVS systems",
        EmulationMachineTab.Controllers.ToString(),
        "off",
        "off",
        [new("off", "Off"), new("on", "On")],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingFourPlayerMode,
        ExplanationResourceKey: SettingsTextConstants.HelpFourPlayerMode
    ), new(
        RuntimeOptionConstants.Freeplay,
        "Freeplay",
        "Play MVS games without the need to insert coins",
        EmulationMachineTab.General.ToString(),
        "off",
        "off",
        [new("off", "Off"), new("on", "On")],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingFreeplay,
        ExplanationResourceKey: SettingsTextConstants.HelpFreeplay
    ), new(
        RuntimeOptionConstants.OverscanT,
        "Mask Overscan (Top)",
        "Mask off pixels hidden by a bezel or border on original CRTs (top)",
        EmulationMachineTab.Video.ToString(),
        "8",
        "8",
        [
        new("16", "16", ResourceKey: SettingsChoiceConstants.Value16),
        new("12", "12", ResourceKey: SettingsChoiceConstants.Value12),
        new("8", "8", ResourceKey: SettingsChoiceConstants.Value8),
        new("4", "4", ResourceKey: SettingsChoiceConstants.Value4),
        new("0", "0", ResourceKey: SettingsChoiceConstants.Value0)
    ],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingMaskOverscanTop,
        ExplanationResourceKey: SettingsTextConstants.HelpMaskOverscanTop
    ), new(
        RuntimeOptionConstants.OverscanB,
        "Mask Overscan (Bottom)",
        "Mask off pixels hidden by a bezel or border on original CRTs (bottom)",
        EmulationMachineTab.Video.ToString(),
        "8",
        "8",
        [
        new("16", "16", ResourceKey: SettingsChoiceConstants.Value16),
        new("12", "12", ResourceKey: SettingsChoiceConstants.Value12),
        new("8", "8", ResourceKey: SettingsChoiceConstants.Value8),
        new("4", "4", ResourceKey: SettingsChoiceConstants.Value4),
        new("0", "0", ResourceKey: SettingsChoiceConstants.Value0)
    ],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingMaskOverscanBottom,
        ExplanationResourceKey: SettingsTextConstants.HelpMaskOverscanBottom
    ), new(
        RuntimeOptionConstants.OverscanL,
        "Mask Overscan (Left)",
        "Mask off pixels hidden by a bezel or border on original CRTs (left)",
        EmulationMachineTab.Video.ToString(),
        "8",
        "8",
        [
        new("16", "16", ResourceKey: SettingsChoiceConstants.Value16),
        new("12", "12", ResourceKey: SettingsChoiceConstants.Value12),
        new("8", "8", ResourceKey: SettingsChoiceConstants.Value8),
        new("4", "4", ResourceKey: SettingsChoiceConstants.Value4),
        new("0", "0", ResourceKey: SettingsChoiceConstants.Value0)
    ],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingMaskOverscanLeft,
        ExplanationResourceKey: SettingsTextConstants.HelpMaskOverscanLeft
    ), new(
        RuntimeOptionConstants.OverscanR,
        "Mask Overscan (Right)",
        "Mask off pixels hidden by a bezel or border on original CRTs (right)",
        EmulationMachineTab.Video.ToString(),
        "8",
        "8",
        [
        new("16", "16", ResourceKey: SettingsChoiceConstants.Value16),
        new("12", "12", ResourceKey: SettingsChoiceConstants.Value12),
        new("8", "8", ResourceKey: SettingsChoiceConstants.Value8),
        new("4", "4", ResourceKey: SettingsChoiceConstants.Value4),
        new("0", "0", ResourceKey: SettingsChoiceConstants.Value0)
    ],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingMaskOverscanRight,
        ExplanationResourceKey: SettingsTextConstants.HelpMaskOverscanRight
    ), new(
        RuntimeOptionConstants.Palette,
        "Palette",
        "Set the Palette",
        EmulationMachineTab.Video.ToString(),
        "resnet",
        "resnet",
        [new("resnet", "Resistor Network", ResourceKey: SettingsChoiceConstants.ValueResistorNetwork), new("raw", "Raw", ResourceKey: SettingsChoiceConstants.ValueRaw)],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingPalette,
        ExplanationResourceKey: SettingsTextConstants.HelpPalette
    ), new(
        RuntimeOptionConstants.Aspect,
        "Aspect Ratio",
        "Set the Aspect Ratio",
        EmulationMachineTab.Video.ToString(),
        "1:1",
        "1:1",
        [
        new("1:1", "Perfectly Square Pixels (1:1 PAR)", ResourceKey: SettingsChoiceConstants.ValuePerfectlySquarePixels11PAR),
        new("45:44", "Ostensibly Accurate NTSC Aspect Ratio (45:44 PAR)", ResourceKey: SettingsChoiceConstants.ValueOstensiblyAccurateNTSCAspectRatio4544PAR),
        new("4:3", "Very Traditional NTSC Aspect Ratio (4:3 DAR)", ResourceKey: SettingsChoiceConstants.ValueVeryTraditionalNTSCAspectRatio43DAR)
    ],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingAspectRatio,
        ExplanationResourceKey: SettingsTextConstants.HelpAspectRatio
    ), new(
        RuntimeOptionConstants.Sprlimit,
        "Sprites-per-line Limit",
        "Set the sprites-per-line limit - increasing causes glitches in some games",
        EmulationMachineTab.Video.ToString(),
        "96",
        "96",
        [
        new("96", "Hardware Accurate (96)", ResourceKey: SettingsChoiceConstants.ValueHardwareAccurate96),
        new("192", "Double (192)", ResourceKey: SettingsChoiceConstants.ValueDouble192),
        new("288", "Triple (288)", ResourceKey: SettingsChoiceConstants.ValueTriple288),
        new("381", "MAX 381 MEGA PRO-GEAR SPEC", ResourceKey: SettingsChoiceConstants.ValueMAX381MEGAPROGEARSPEC)
    ],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingSpritesPerLineLimit,
        ExplanationResourceKey: SettingsTextConstants.HelpSpritesPerLineLimit
    ), new(
        RuntimeOptionConstants.Oc,
        "Overclocking",
        "Annihilate your accuracy with The 24MHz Shock - expect glitches",
        EmulationMachineTab.Cpu.ToString(),
        "off",
        "off",
        [new("off", "Off"), new("on", "On")],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingProcessorSpeed,
        ExplanationResourceKey: SettingsTextConstants.HelpProcessorSpeed
    ), new(
        RuntimeOptionConstants.DisableAdpcmWrap,
        "Disable ADPCM Accumulator Wrap",
        "ADPCM Accumulator Wrap may be disabled to fix sound effects in buggy games, for example Ganryu and Nightmare in the Dark. This is a hack, and should remain Off for most games.",
        EmulationMachineTab.Audio.ToString(),
        "off",
        "off",
        [new("off", "Off"), new("on", "On")],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingDisableADPCMAccumulatorWrap,
        ExplanationResourceKey: SettingsTextConstants.HelpDisableADPCMAccumulatorWrap
    ), new(
        RuntimeOptionConstants.CdSpeedHack,
        "CD Speed Hack",
        "Patch BIOS busy-wait loops to reduce loading times (Does not work for Universe BIOS)",
        EmulationMachineTab.Storage.ToString(),
        "disabled",
        "disabled",
        [new("enabled", "Enabled"), new("disabled", "Disabled")],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingCDSpeedHack,
        ExplanationResourceKey: SettingsTextConstants.HelpCDSpeedHack
    ), new(
        RuntimeOptionConstants.CdDmaLenLimit,
        "CD DMA Length Limit",
        "Correct the BIOS upload pointers when a CD buffer DMA is larger than one sector, fixing corrupted sound in Art of Fighting's bonus stage. This is a hack, and real hardware behaviour is unconfirmed.",
        EmulationMachineTab.Storage.ToString(),
        "disabled",
        "disabled",
        [new("enabled", "Enabled"), new("disabled", "Disabled")],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingCDDMALengthLimit,
        ExplanationResourceKey: SettingsTextConstants.HelpCDDMALengthLimit
    ), new(
        RuntimeOptionConstants.CdSkipLoading,
        "Skip CD Loading",
        "Fast-forward through CD loading screens",
        EmulationMachineTab.Storage.ToString(),
        "disabled",
        "disabled",
        [new("enabled", "Enabled"), new("disabled", "Disabled")],
        IsVisible: true,
        ResourceKey: SettingsTextConstants.SettingSkipCDLoading,
        ExplanationResourceKey: SettingsTextConstants.HelpSkipCDLoading
    ), });
}
