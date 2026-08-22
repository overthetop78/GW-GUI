using GWGUI.Emulation;

namespace GWGUI.App.Contracts.Input;

internal sealed record KeyboardShortcutBinding(KeyboardChord Chord, EmulationKey EmulationKey);
