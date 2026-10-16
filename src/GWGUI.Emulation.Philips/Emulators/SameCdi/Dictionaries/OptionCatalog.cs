using GWGUI.Emulation.Philips.Emulators.Common.Functions;
using GWGUI.Emulation.Philips.Emulators.SameCdi.Constants;

namespace GWGUI.Emulation.Philips.Emulators.SameCdi.Dictionaries;
internal static class SameCdiOptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } = [
        new(
            OptionConstants.VideoStandard,
            "Video standard (Restart)",
            "Select the Mono-I player's television standard. Restart content after changing this option.",
            "system",
            OptionConstants.Pal,
            OptionConstants.Pal,
            [new(OptionConstants.Pal, "PAL (50 Hz)", VideoStandard: EmulationVideoStandard.Pal, FrequencyHz: 50), new(OptionConstants.Ntsc, "NTSC (60 Hz)", VideoStandard: EmulationVideoStandard.Ntsc, FrequencyHz: 60)],
            ResourceKey: SettingsTextConstants.VideoStandard,
            ExplanationResourceKey: SettingsTextConstants.VideoStandardHelp
        ),
        new(
            OptionConstants.ReadConfig,
            "Read configuration",
            null,
            "system",
            OptionConstants.Disabled,
            OptionConstants.Disabled,
            [new(OptionConstants.Enabled, "enabled", ResourceKey: SettingsChoiceConstants.Enabled), new(OptionConstants.Disabled, "disabled", ResourceKey: SettingsChoiceConstants.Disabled)],
            IsVisible: false
        ),
        new(
            OptionConstants.WriteConfig,
            "Write configuration",
            null,
            "system",
            OptionConstants.Disabled,
            OptionConstants.Disabled,
            [new(OptionConstants.Enabled, "enabled", ResourceKey: SettingsChoiceConstants.Enabled), new(OptionConstants.Disabled, "disabled", ResourceKey: SettingsChoiceConstants.Disabled)],
            IsVisible: false
        ),
        new(
            OptionConstants.Saves,
            "Save state naming",
            null,
            "system",
            OptionConstants.Game,
            OptionConstants.Game,
            [new(OptionConstants.Game, "game", ResourceKey: SettingsChoiceConstants.Game), new(OptionConstants.System, "system", ResourceKey: SettingsChoiceConstants.System)],
            ResourceKey: SettingsTextConstants.StateNaming,
            ExplanationResourceKey: SettingsTextConstants.StateNamingHelp
        ),
        new(
            OptionConstants.NvramSaves,
            "NVRAM Saves per game",
            null,
            "system",
            OptionConstants.Enabled,
            OptionConstants.Enabled,
            [new(OptionConstants.Enabled, "enabled", ResourceKey: SettingsChoiceConstants.Enabled), new(OptionConstants.Disabled, "disabled", ResourceKey: SettingsChoiceConstants.Disabled)],
            ResourceKey: SettingsTextConstants.NvramPerGame,
            ExplanationResourceKey: SettingsTextConstants.NvramPerGameHelp
        ),
        new(
            OptionConstants.AutoSave,
            "Auto save/load states",
            null,
            "system",
            OptionConstants.Disabled,
            OptionConstants.Disabled,
            [new(OptionConstants.Enabled, "enabled", ResourceKey: SettingsChoiceConstants.Enabled), new(OptionConstants.Disabled, "disabled", ResourceKey: SettingsChoiceConstants.Disabled)],
            ResourceKey: SettingsTextConstants.AutoSave,
            ExplanationResourceKey: SettingsTextConstants.AutoSaveHelp
        ),
        new(
            OptionConstants.BootFromCli,
            "Boot from CLI",
            null,
            "system",
            OptionConstants.Disabled,
            OptionConstants.Disabled,
            [new(OptionConstants.Enabled, "enabled", ResourceKey: SettingsChoiceConstants.Enabled), new(OptionConstants.Disabled, "disabled", ResourceKey: SettingsChoiceConstants.Disabled)],
            IsVisible: false
        ),
        new(
            OptionConstants.MamePathsEnable,
            "MAME INI Paths",
            null,
            "system",
            OptionConstants.Disabled,
            OptionConstants.Disabled,
            [new(OptionConstants.Enabled, "enabled", ResourceKey: SettingsChoiceConstants.Enabled), new(OptionConstants.Disabled, "disabled", ResourceKey: SettingsChoiceConstants.Disabled)],
            IsVisible: false
        ),
        new(
            OptionConstants.AlternateRenderer,
            "Alternate render method",
            null,
            "video",
            OptionConstants.Disabled,
            OptionConstants.Disabled,
            [new(OptionConstants.Enabled, "enabled", ResourceKey: SettingsChoiceConstants.Enabled), new(OptionConstants.Disabled, "disabled", ResourceKey: SettingsChoiceConstants.Disabled)],
            ResourceKey: SettingsTextConstants.AlternateRenderer,
            ExplanationResourceKey: SettingsTextConstants.AlternateRendererHelp
        ),
        new(
            OptionConstants.Altres,
            "Resolution",
            null,
            "video",
            NumericOptionValueFunctions.Resolution(OptionConstants.DefaultResolutionWidth, OptionConstants.DefaultResolutionHeight),
            NumericOptionValueFunctions.Resolution(OptionConstants.DefaultResolutionWidth, OptionConstants.DefaultResolutionHeight),
            NumericOptionValueFunctions.Resolutions(OptionConstants.Resolutions),
            ResourceKey: SettingsTextConstants.Resolution,
            ExplanationResourceKey: SettingsTextConstants.ResolutionHelp
        ),
        new(
            OptionConstants.MouseEnable,
            "Enable in-game mouse",
            null,
            "input",
            OptionConstants.Disabled,
            OptionConstants.Disabled,
            [new(OptionConstants.Enabled, "enabled", ResourceKey: SettingsChoiceConstants.Enabled), new(OptionConstants.Disabled, "disabled", ResourceKey: SettingsChoiceConstants.Disabled)],
            IsVisible: false
        ),
        new(
            OptionConstants.LightgunMode,
            "Lightgun mode",
            null,
            "input",
            OptionConstants.None,
            OptionConstants.None,
            [new(OptionConstants.None, "none", InvariantDisplayValue: "none"), new(OptionConstants.Touchscreen, "touchscreen", InvariantDisplayValue: "touchscreen"), new(OptionConstants.Lightgun, "lightgun", InvariantDisplayValue: "lightgun")],
            IsVisible: false
        ),
        new(
            OptionConstants.ButtonsProfiles,
            "Profile Buttons according to games (Restart)",
            null,
            "input",
            OptionConstants.Enabled,
            OptionConstants.Enabled,
            [new(OptionConstants.Enabled, "enabled", ResourceKey: SettingsChoiceConstants.Enabled), new(OptionConstants.Disabled, "disabled", ResourceKey: SettingsChoiceConstants.Disabled)],
            IsVisible: false
        ),
        new(
            OptionConstants.Mame4wayEnable,
            "MAME Joystick 4-way simulation",
            null,
            "input",
            OptionConstants.Disabled,
            OptionConstants.Disabled,
            [new(OptionConstants.Disabled, "disabled", ResourceKey: SettingsChoiceConstants.Disabled),
                new(OptionConstants.Value4way, "4way", InvariantDisplayValue: "4way"),
                new(OptionConstants.Strict, "strict", InvariantDisplayValue: "strict"),
                new(OptionConstants.Qbert, "qbert", InvariantDisplayValue: "qbert")],
            IsVisible: false
        ),
        new(
            OptionConstants.Throttle,
            "Enable throttle",
            null,
            "advanced",
            OptionConstants.Disabled,
            OptionConstants.Disabled,
            [new(OptionConstants.Enabled, "enabled", ResourceKey: SettingsChoiceConstants.Enabled), new(OptionConstants.Disabled, "disabled", ResourceKey: SettingsChoiceConstants.Disabled)],
            ResourceKey: SettingsTextConstants.Throttle,
            ExplanationResourceKey: SettingsTextConstants.ThrottleHelp
        ),
        new(
            OptionConstants.CheatsEnable,
            "Enable cheats",
            null,
            "advanced",
            OptionConstants.Disabled,
            OptionConstants.Disabled,
            [new(OptionConstants.Enabled, "enabled", ResourceKey: SettingsChoiceConstants.Enabled), new(OptionConstants.Disabled, "disabled", ResourceKey: SettingsChoiceConstants.Disabled)],
            ResourceKey: SettingsTextConstants.Cheats,
            ExplanationResourceKey: SettingsTextConstants.CheatsHelp
        ),
        new(
            OptionConstants.CpuOverclock,
            "Main CPU Overclock",
            null,
            "advanced",
            OptionConstants.Default,
            OptionConstants.Default,
            [
                new(OptionConstants.Default, "default", ResourceKey: SettingsChoiceConstants.Default),
                .. NumericOptionValueFunctions.IntegerChoices(OptionConstants.CpuPercentRanges, false)
            ],
            ResourceKey: SettingsTextConstants.CpuOverclock,
            ExplanationResourceKey: SettingsTextConstants.CpuOverclockHelp
        )
    ];
}