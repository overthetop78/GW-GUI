using GWGUI.Emulation;

namespace GWGUI.App.Input;

internal sealed record KeyboardShortcutBinding(KeyboardChord Chord, EmulationKey EmulationKey);
