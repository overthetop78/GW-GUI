using GWGUI.Emulation.Epoch.Emulators.EmuSCV.Constants;

namespace GWGUI.Emulation.Epoch.Emulators.EmuSCV.Dictionaries;

internal static class EmuSCVOptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new(OptionConstants.OptionConsole, "CONSOLE", null, null,
            "AUTO", "AUTO", [new("AUTO", "AUTO"), new("EPOCH", "EPOCH"), new("YENO", "YENO"), new("EPOCHLADY", "EPOCHLADY")]),
        new(OptionConstants.OptionDisplay, "DISPLAY", null, null,
            "AUTO", "AUTO", [new("AUTO", "AUTO"), new("EMUSCV", "EMUSCV"), new("EPOCH", "EPOCH"), new("YENO", "YENO")]),
        new(OptionConstants.OptionPixelaspect, "PIXELASPECT", null, null,
            "AUTO", "AUTO", [new("AUTO", "AUTO"), new("RECTANGULAR", "RECTANGULAR"), new("SQUARE", "SQUARE")]),
        new(OptionConstants.OptionResolution, "RESOLUTION", null, null,
            "AUTO", "AUTO", [new("AUTO", "AUTO"), new("LOW", "LOW"), new("MEDIUM", "MEDIUM"), new("HIGH", "HIGH")]),
        new(OptionConstants.OptionPalette, "PALETTE", null, null,
            "AUTO", "AUTO", [new("AUTO", "AUTO"), new("STANDARD", "STANDARD"), new("OLDNTSC", "OLDNTSC")]),
        new(OptionConstants.OptionFps, "FPS", null, null,
            "AUTO", "AUTO", [new("AUTO", "AUTO"), new("EPOCH60", "EPOCH60"), new("YENO50", "YENO50")]),
        new(OptionConstants.OptionDisplayfullmemory, "DISPLAYFULLMEMORY", null, null,
            "AUTO", "AUTO", [new("AUTO", "AUTO"), new("YES", "YES"), new("NO", "NO")]),
        new(OptionConstants.OptionDisplayinputs, "DISPLAYINPUTS", null, null,
            "AUTO", "AUTO", [new("AUTO", "AUTO"), new("YES", "YES"), new("NO", "NO")]),
        new(OptionConstants.OptionLanguage, "LANGUAGE", null, null,
            "AUTO", "AUTO", [new("AUTO", "AUTO"), new("JP", "JP"), new("FR", "FR"), new("EN", "EN")]),
        new(OptionConstants.OptionCheckbios, "CHECKBIOS", null, null,
            "AUTO", "AUTO", [new("AUTO", "AUTO"), new("YES", "YES"), new("NO", "NO")]),
        new(OptionConstants.OptionKeyboardatstart, "KEYBOARDATSTART", null, null,
            "AUTO", "AUTO", [new("AUTO", "AUTO"), new("YES", "YES"), new("NO", "NO")]),
    ];
}
