using GWGUI.Emulation.Coleco.Emulators.JollyCV.Constants;

namespace GWGUI.Emulation.Coleco.Emulators.JollyCV.Dictionaries;
internal static class JollyCVOptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } = [
        new(
            OptionConstants.OptionTmspalette,
            "TMS9918 Palette",
            "Set the Palette",
            null,
            "TeaTime",
            "TeaTime",
            [
                new("TeaTime", "TeaTime (Default)", ResourceKey: SettingsChoiceConstants.ValueTeaTime),
                new("SYoung", "SYoung", ResourceKey: SettingsChoiceConstants.ValueSYoung),
                new("GCDatasheet", "GCDatasheet", ResourceKey: SettingsChoiceConstants.ValueGCDatasheet)
            ],
            ResourceKey: SettingsTextConstants.ColorPalette,
            ExplanationResourceKey: SettingsTextConstants.ColorPaletteHelp
        ),
        new(
            OptionConstants.OptionAspect,
            "Aspect Ratio",
            "Set the Aspect Ratio",
            null,
            "Auto",
            "Auto",
            [
                new("Auto", "Region-based Pixel Aspect Ratio (Default)", ResourceKey: "Visual.Automatic"),
                new("1:1", "Perfectly Square Pixels (1:1 PAR)", ResourceKey: SettingsChoiceConstants.ValueAspect1By1PAR),
                new("NTSC", "Very Traditional NTSC Aspect Ratio (4:3 DAR)", ResourceKey: SettingsChoiceConstants.ValueAspect4By3NTSC),
                new("PAL", "Ostensibly Accurate PAL Aspect Ratio", ResourceKey: SettingsChoiceConstants.ValuePAL)
            ],
            ResourceKey: SettingsTextConstants.AspectRatio,
            ExplanationResourceKey: SettingsTextConstants.AspectRatioHelp
        ),
        new(
            OptionConstants.OptionOverscanT,
            "Mask Overscan (Top)",
            "Mask off pixels hidden by a bezel or border on original CRTs (top)",
            null,
            "0",
            "0",
            [
                new("0", "0 (Default)", InvariantDisplayValue: "0"),
                new("2", "2", InvariantDisplayValue: "2"),
                new("4", "4", InvariantDisplayValue: "4"),
                new("6", "6", InvariantDisplayValue: "6"),
                new("8", "8", InvariantDisplayValue: "8"),
                new("10", "10", InvariantDisplayValue: "10"),
                new("12", "12", InvariantDisplayValue: "12"),
                new("14", "14", InvariantDisplayValue: "14"),
                new("16", "16", InvariantDisplayValue: "16")
            ],
            ResourceKey: SettingsTextConstants.CropTopBorder,
            ExplanationResourceKey: SettingsTextConstants.CropTopBorderHelp
        ),
        new(
            OptionConstants.OptionOverscanB,
            "Mask Overscan (Bottom)",
            "Mask off pixels hidden by a bezel or border on original CRTs (bottom)",
            null,
            "0",
            "0",
            [
                new("0", "0 (Default)", InvariantDisplayValue: "0"),
                new("2", "2", InvariantDisplayValue: "2"),
                new("4", "4", InvariantDisplayValue: "4"),
                new("6", "6", InvariantDisplayValue: "6"),
                new("8", "8", InvariantDisplayValue: "8"),
                new("10", "10", InvariantDisplayValue: "10"),
                new("12", "12", InvariantDisplayValue: "12"),
                new("14", "14", InvariantDisplayValue: "14"),
                new("16", "16", InvariantDisplayValue: "16")
            ],
            ResourceKey: SettingsTextConstants.CropBottomBorder,
            ExplanationResourceKey: SettingsTextConstants.CropBottomBorderHelp
        ),
        new(
            OptionConstants.OptionOverscanL,
            "Mask Overscan (Left)",
            "Mask off pixels hidden by a bezel or border on original CRTs (left)",
            null,
            "6",
            "6",
            [
                new("0", "0", InvariantDisplayValue: "0"),
                new("2", "2", InvariantDisplayValue: "2"),
                new("4", "4", InvariantDisplayValue: "4"),
                new("6", "6 (Default)", InvariantDisplayValue: "6"),
                new("8", "8", InvariantDisplayValue: "8")
            ],
            ResourceKey: SettingsTextConstants.CropLeftBorder,
            ExplanationResourceKey: SettingsTextConstants.CropLeftBorderHelp
        ),
        new(
            OptionConstants.OptionOverscanR,
            "Mask Overscan (Right)",
            "Mask off pixels hidden by a bezel or border on original CRTs (right)",
            null,
            "6",
            "6",
            [
                new("0", "0", InvariantDisplayValue: "0"),
                new("2", "2", InvariantDisplayValue: "2"),
                new("4", "4", InvariantDisplayValue: "4"),
                new("6", "6 (Default)", InvariantDisplayValue: "6"),
                new("8", "8", InvariantDisplayValue: "8")
            ],
            ResourceKey: SettingsTextConstants.CropRightBorder,
            ExplanationResourceKey: SettingsTextConstants.CropRightBorderHelp
        ),
        new(
            OptionConstants.OptionNosprlimit,
            "No Sprite per Line Limit (Hack)",
            "Remove the sprites per line limit - can cause glitches",
            null,
            "off",
            "off",
            [new("off", "OFF (Default)", ResourceKey: "Emulation.Value.Disabled"), new("on", "ON", ResourceKey: "Emulation.Value.Enabled")],
            ResourceKey: SettingsTextConstants.RemoveSpriteLimit,
            ExplanationResourceKey: SettingsTextConstants.RemoveSpriteLimitHelp
        ),
        new(
            OptionConstants.OptionAutoreset,
            "CreatiVision Auto-Reset (Hack)",
            "Automatically reset CreatiVision games after starting to immediately gain control without having to manually reset",
            null,
            "off",
            "off",
            [new("off", "OFF (Default)"), new("on", "ON")]
        ),

    ];
}