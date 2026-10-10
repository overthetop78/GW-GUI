using GWGUI.Emulation.Coleco.Emulators.JollyCV.Constants;

namespace GWGUI.Emulation.Coleco.Emulators.JollyCV.Dictionaries;

internal static class JollyCVOptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new(OptionConstants.OptionTmspalette, "TMS9918 Palette",
            "Set the Palette", null,
            "TeaTime", "TeaTime",
            [new("TeaTime", "TeaTime (Default)"), new("SYoung", "SYoung"), new("GCDatasheet", "GCDatasheet")]),
        new(OptionConstants.OptionAspect, "Aspect Ratio",
            "Set the Aspect Ratio", null,
            "Auto", "Auto",
            [new("Auto", "Region-based Pixel Aspect Ratio (Default)"), new("1:1", "Perfectly Square Pixels (1:1 PAR)"), new("NTSC", "Very Traditional NTSC Aspect Ratio (4:3 DAR)"), new("PAL", "Ostensibly Accurate PAL Aspect Ratio")]),
        new(OptionConstants.OptionOverscanT, "Mask Overscan (Top)",
            "Mask off pixels hidden by a bezel or border on original CRTs (top)", null,
            "0", "0",
            [new("0", "0 (Default)"), new("2", "2"), new("4", "4"), new("6", "6"), new("8", "8"), new("10", "10"), new("12", "12"), new("14", "14"), new("16", "16")]),
        new(OptionConstants.OptionOverscanB, "Mask Overscan (Bottom)",
            "Mask off pixels hidden by a bezel or border on original CRTs (bottom)", null,
            "0", "0",
            [new("0", "0 (Default)"), new("2", "2"), new("4", "4"), new("6", "6"), new("8", "8"), new("10", "10"), new("12", "12"), new("14", "14"), new("16", "16")]),
        new(OptionConstants.OptionOverscanL, "Mask Overscan (Left)",
            "Mask off pixels hidden by a bezel or border on original CRTs (left)", null,
            "6", "6",
            [new("0", "0"), new("2", "2"), new("4", "4"), new("6", "6 (Default)"), new("8", "8")]),
        new(OptionConstants.OptionOverscanR, "Mask Overscan (Right)",
            "Mask off pixels hidden by a bezel or border on original CRTs (right)", null,
            "6", "6",
            [new("0", "0"), new("2", "2"), new("4", "4"), new("6", "6 (Default)"), new("8", "8")]),
        new(OptionConstants.OptionNosprlimit, "No Sprite per Line Limit (Hack)",
            "Remove the sprites per line limit - can cause glitches", null,
            "off", "off",
            [new("off", "OFF (Default)"), new("on", "ON")]),
        new(OptionConstants.OptionAutoreset, "CreatiVision Auto-Reset (Hack)",
            "Automatically reset CreatiVision games after starting to immediately gain control without having to manually reset", null,
            "off", "off",
            [new("off", "OFF (Default)"), new("on", "ON")]),
    ];
}
