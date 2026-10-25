namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Constants;

internal static class KeyboardConstants
{
    internal const string PetBusinessStop = "STOP";
    internal static readonly IReadOnlyDictionary<EmulationKey, string> SpecialKeys = new Dictionary<EmulationKey, string>
    {
        [EmulationKey.CommodoreKey] = "C=",
        [EmulationKey.CommodoreRunStop] = "RUN/STOP",
        [EmulationKey.CommodoreShiftLock] = "SHIFT LOCK",
        [EmulationKey.CommodoreClearHome] = "CLR/HOME",
        [EmulationKey.CommodoreRestore] = "RESTORE",
        [EmulationKey.CommodorePound] = "£",
        [EmulationKey.CommodoreArrowLeft] = "←",
        [EmulationKey.CommodoreArrowUp] = "↑",
        [EmulationKey.Help] = "HELP",
        [EmulationKey.CommodoreLineFeed] = "LINE FEED",
        [EmulationKey.Commodore40_80Display] = "40/80 DISPLAY",
        [EmulationKey.CommodoreNoScroll] = "NO SCROLL",
        [EmulationKey.CommodoreAsciiDinCaps] = "CAPS (ASCII/DIN)",
        [EmulationKey.CommodoreOffReverse] = "OFF/RVS",
        [EmulationKey.CommodoreNormalGraphic] = "NORM/GRAPH",
        [EmulationKey.CommodoreKeypadClearEntry] = "CE",
        [EmulationKey.CommodoreRepeat] = "RPT"
    };
}
