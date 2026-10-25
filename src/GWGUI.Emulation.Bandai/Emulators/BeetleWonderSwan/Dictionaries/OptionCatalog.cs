using System.Globalization;
using GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Functions;
using GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Constants;

namespace GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Dictionaries;
internal static class BeetleWonderSwanOptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } = [
        new(
            OptionConstants.OptionLanguage,
            "Language",
            "Console language saved in the internal EEPROM. Applied when a game is loaded.",
            null,
            "english",
            "english",
            [new("english", "English"), new("japanese", "Japanese"), ],
            IsVisible: false,
            RequiresRuntimeReload: true
        ),
        new(
            OptionConstants.OptionNameSource,
            "Owner Name Source",
            "Select the owner name stored in the internal EEPROM. Applied when a game is loaded.",
            null,
            "frontend",
            "frontend",
            [new("frontend", "Frontend Username"), new("default", "Default (Mednafen)"), ],
            IsVisible: false,
            RequiresRuntimeReload: true
        ),
        new(
            OptionConstants.OptionBirthYear,
            "Owner Birth Year",
            "Birth year stored in the internal EEPROM. Applied when a game is loaded.",
            null,
            OptionConstants.DefaultBirthYear.ToString(CultureInfo.InvariantCulture),
            OptionConstants.DefaultBirthYear.ToString(CultureInfo.InvariantCulture),
            NumericOptionValueFunctions.IntegerRange(OptionConstants.BirthYears, false),
            IsVisible: false,
            RequiresRuntimeReload: true
        ),
        new(
            OptionConstants.OptionBirthMonth,
            "Owner Birth Month",
            "Birth month stored in the internal EEPROM. Applied when a game is loaded.",
            null,
            "6",
            "6",
            [
                new("1", "January"),
                new("2", "February"),
                new("3", "March"),
                new("4", "April"),
                new("5", "May"),
                new("6", "June"),
                new("7", "July"),
                new("8", "August"),
                new("9", "September"),
                new("10", "October"),
                new("11", "November"),
                new("12", "December"),

            ],
            IsVisible: false,
            RequiresRuntimeReload: true
        ),
        new(
            OptionConstants.OptionBirthDay,
            "Owner Birth Day",
            "Birth day stored in the internal EEPROM. Applied when a game is loaded.",
            null,
            OptionConstants.DefaultBirthDay.ToString(CultureInfo.InvariantCulture),
            OptionConstants.DefaultBirthDay.ToString(CultureInfo.InvariantCulture),
            NumericOptionValueFunctions.IntegerRange(OptionConstants.BirthDays, false),
            IsVisible: false,
            RequiresRuntimeReload: true
        ),
        new(
            OptionConstants.OptionSex,
            "Owner Sex",
            "Owner sex stored in the internal EEPROM. Applied when a game is loaded.",
            null,
            "female",
            "female",
            [new("female", "Female"), new("male", "Male"), ],
            IsVisible: false,
            RequiresRuntimeReload: true
        ),
        new(
            OptionConstants.OptionBlood,
            "Owner Blood Type",
            "Blood type stored in the internal EEPROM. Applied when a game is loaded.",
            null,
            "o",
            "o",
            [new("o", "O"), new("a", "A"), new("b", "B"), new("ab", "AB"), ],
            IsVisible: false,
            RequiresRuntimeReload: true
        ),
        new(
            OptionConstants.OptionRotateDisplay,
            "Display Rotation",
            "Select landscape or portrait orientation. Manual mode uses the assigned rotation button.",
            null,
            "manual",
            "manual",
            [
                new("manual", "Manual", ResourceKey: SettingsChoiceConstants.Manual),
                new("landscape", "Landscape", ResourceKey: SettingsChoiceConstants.Landscape),
                new("portrait", "Portrait", ResourceKey: SettingsChoiceConstants.Portrait),

            ],
            ResourceKey: SettingsTextConstants.DisplayRotation,
            ExplanationResourceKey: SettingsTextConstants.DisplayRotationHelp
        ),
        new(
            OptionConstants.OptionRotateKeymap,
            "Rotate Button Mappings",
            "Automatic mode follows the display orientation. Other modes keep the controls in the selected orientation.",
            null,
            "auto",
            "auto",
            [
                new("auto", "Auto", ResourceKey: SettingsChoiceConstants.Automatic),
                new("disabled", "Landscape", ResourceKey: SettingsChoiceConstants.Landscape),
                new("enabled", "Portrait", ResourceKey: SettingsChoiceConstants.Portrait),

            ],
            ResourceKey: SettingsTextConstants.ControlOrientation,
            ExplanationResourceKey: SettingsTextConstants.ControlOrientationHelp
        ),
        new(
            OptionConstants.OptionMonoPalette,
            "Color Palette",
            "Select the colors used for monochrome games.",
            null,
            "default",
            "default",
            [
                new("default", "Grayscale", ResourceKey: SettingsChoiceConstants.Grayscale),
                new("wonderswan", "WS - WonderSwan", InvariantDisplayValue: "WS - WonderSwan"),
                new("wondeswan_color", "WS - WonderSwan Color", InvariantDisplayValue: "WS - WonderSwan Color"),
                new("swancrystal", "WS - SwanCrystal", InvariantDisplayValue: "WS - SwanCrystal"),
                new("gb_dmg", "Game Boy DMG", InvariantDisplayValue: "Game Boy DMG"),
                new("gb_pocket", "Game Boy Pocket", InvariantDisplayValue: "Game Boy Pocket"),
                new("gb_light", "Game Boy Light", InvariantDisplayValue: "Game Boy Light"),
                new("blossom_pink", "Blossom Pink", InvariantDisplayValue: "Blossom Pink"),
                new("bubbles_blue", "Bubbles Blue", InvariantDisplayValue: "Bubbles Blue"),
                new("buttercup_green", "Buttercup Green", InvariantDisplayValue: "Buttercup Green"),
                new("digivice", "Digivice", InvariantDisplayValue: "Digivice"),
                new("game_com", "Game.com", InvariantDisplayValue: "Game.com"),
                new("gameking", "GameKing", InvariantDisplayValue: "GameKing"),
                new("game_master", "Game Master", InvariantDisplayValue: "Game Master"),
                new("golden_wild", "Golden Wild", InvariantDisplayValue: "Golden Wild"),
                new("greenscale", "Greenscale", InvariantDisplayValue: "Greenscale"),
                new("hokage_orange", "Hokage Orange", InvariantDisplayValue: "Hokage Orange"),
                new("labo_fawn", "Labo Fawn", InvariantDisplayValue: "Labo Fawn"),
                new("legendary_super_saiyan", "Legendary Super Saiyan", InvariantDisplayValue: "Legendary Super Saiyan"),
                new("microvision", "Microvision", InvariantDisplayValue: "Microvision"),
                new("million_live_gold", "Million Live Gold", InvariantDisplayValue: "Million Live Gold"),
                new("odyssey_gold", "Odyssey Gold", InvariantDisplayValue: "Odyssey Gold"),
                new("shiny_sky_blue", "Shiny Sky Blue", InvariantDisplayValue: "Shiny Sky Blue"),
                new("slime_blue", "Slime Blue", InvariantDisplayValue: "Slime Blue"),
                new("ti_83", "TI-83", InvariantDisplayValue: "TI-83"),
                new("travel_wood", "Travel Wood", InvariantDisplayValue: "Travel Wood"),
                new("virtual_boy", "Virtual Boy", InvariantDisplayValue: "Virtual Boy"),

            ],
            ResourceKey: SettingsTextConstants.MonochromePalette,
            ExplanationResourceKey: SettingsTextConstants.MonochromePaletteHelp
        ),
        new(
            OptionConstants.OptionGfxColors,
            "Color Depth",
            "Select the color depth. The 24-bit mode requires more processing power.",
            null,
            "16bit",
            "16bit",
            [new("16bit", "Thousands (16-bit)", InvariantDisplayValue: "16 bits"), new("24bit", "Millions (24-bit)", InvariantDisplayValue: "24 bits"), ],
            ResourceKey: SettingsTextConstants.ColorDepth,
            ExplanationResourceKey: SettingsTextConstants.ColorDepthHelp,
            RequiresRuntimeReload: true
        ),
        new(
            OptionConstants.OptionFrameskip,
            "Frameskip",
            "Skip video frames to reduce audio crackling. Automatic mode follows audio buffer status. Manual mode uses the selected threshold.",
            null,
            "disabled",
            "disabled",
            [new("disabled", "disabled", ResourceKey: SettingsChoiceConstants.Disabled), new("auto", "Auto", ResourceKey: SettingsChoiceConstants.Automatic), new("manual", "Manual", ResourceKey: SettingsChoiceConstants.Manual), ],
            ResourceKey: SettingsTextConstants.FrameSkipping,
            ExplanationResourceKey: SettingsTextConstants.FrameSkippingHelp
        ),
        new(
            OptionConstants.OptionFrameskipThreshold,
            "Frameskip Threshold (%)",
            "In manual mode, skip frames when the audio buffer falls below this percentage.",
            null,
            OptionConstants.DefaultFrameSkipThresholdPercent.ToString(CultureInfo.InvariantCulture),
            OptionConstants.DefaultFrameSkipThresholdPercent.ToString(CultureInfo.InvariantCulture),
            NumericOptionValueFunctions.IntegerRange(OptionConstants.FrameSkipThresholds, true),
            ResourceKey: SettingsTextConstants.FrameSkipThreshold,
            ExplanationResourceKey: SettingsTextConstants.FrameSkipThresholdHelp
        ),
        new(
            OptionConstants.Option60hzMode,
            "60Hz Mode",
            "Skip every fifth frame to display at 60 Hz. This reduces motion smoothness.",
            null,
            "enabled",
            "enabled",
            [new("disabled", "disabled", ResourceKey: SettingsChoiceConstants.Disabled), new("enabled", "enabled", ResourceKey: SettingsChoiceConstants.Enabled), ],
            ResourceKey: SettingsTextConstants.FixedRefreshRate,
            ExplanationResourceKey: SettingsTextConstants.FixedRefreshRateHelp
        ),
        new(
            OptionConstants.OptionSoundSampleRate,
            "Sound Output Sample Rate",
            "Higher sample rates slightly improve audio quality and require more processing power.",
            null,
            OptionConstants.DefaultAudioSampleRateHz.ToString(CultureInfo.InvariantCulture),
            OptionConstants.DefaultAudioSampleRateHz.ToString(CultureInfo.InvariantCulture),
            NumericOptionValueFunctions.AudioSampleRates(),
            ResourceKey: SettingsTextConstants.AudioSampleRate,
            ExplanationResourceKey: SettingsTextConstants.AudioSampleRateHelp
        ),
        new(
            OptionConstants.OptionSoundLowPass,
            "Audio Filter",
            "Apply a low-pass filter to soften the sound.",
            null,
            "disabled",
            "disabled",
            [new("disabled", "disabled", ResourceKey: SettingsChoiceConstants.Disabled), new("enabled", "enabled", ResourceKey: SettingsChoiceConstants.Enabled), ],
            ResourceKey: SettingsTextConstants.AudioFilter,
            ExplanationResourceKey: SettingsTextConstants.AudioFilterHelp
        ),

    ];
}