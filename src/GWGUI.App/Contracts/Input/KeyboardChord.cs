using System.Windows.Input;

namespace GWGUI.App.Contracts.Input;

public sealed record KeyboardChord(ModifierKeys Modifiers, IReadOnlyList<Key> Keys);
