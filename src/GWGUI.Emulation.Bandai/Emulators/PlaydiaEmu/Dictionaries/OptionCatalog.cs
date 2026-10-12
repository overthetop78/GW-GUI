using GWGUI.Emulation.Bandai.Common.Contracts;
using GWGUI.Emulation.Bandai.Emulators.PlaydiaEmu.Constants;

namespace GWGUI.Emulation.Bandai.Emulators.PlaydiaEmu.Dictionaries;
internal static class PlaydiaEmuOptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } = [
        new(
            OptionConstants.OptionVolume,
            "Audio Volume (%)",
            null,
            null,
            "100",
            "100",
            [new("100", "100"), new("90", "90"), new("80", "80"), new("70", "70"), new("60", "60"), new("50", "50"), new("40", "40"), new("30", "30"), new("20", "20"), new("10", "10"), new("0", "0"), ]
        ),
        new(OptionConstants.OptionSwapAb, "Swap A/B Buttons", null, null, "disabled", "disabled", [new("disabled", "disabled"), new("enabled", "enabled"), ]),
        new(OptionConstants.OptionDebugLogging, "Debug Logging", null, null, "disabled", "disabled", [new("disabled", "disabled"), new("enabled", "enabled"), ]),

    ];
}